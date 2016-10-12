using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfPoeChallengeTracker.model.persistence;

namespace WpfPoeChallengeTracker.model
{
    public class Model : INotifyPropertyChanged
    {

        private List<ChallengeProgress> challengeProgresses;
        public List<ChallengeProgress> ChallengeProgresses
        {
            get { return challengeProgresses; }
            set { challengeProgresses = value; }
        }

        private LoginStatus loginStatus;

        public LoginStatus CurrentLoginStatus

        {
            get { return loginStatus; }
            set
            {
                if (loginStatus != value)
                {
                    loginStatus = value;
                }
            }
        }

        public string AccountName
        {
            get { return Properties.Settings.Default.AccountName; }
            set { Properties.Settings.Default.AccountName = value; }
        }

        private List<int> viewOrder;

        public List<int> ViewOrder
        {
            get { return viewOrder; }
            set
            {
                viewOrder = value;
                hasChanged = true;
            }
        }

        private LeagueInfo leagueInfo;

        public LeagueInfo LeagueInfo
        {
            get { return leagueInfo; }
            set { leagueInfo = value; }
        }

        private List<League> availableLeagues;

        public List<League> AvailableLeagues
        {
            get { return availableLeagues; }
            set { availableLeagues = value; }
        }

        private Timer saveProgressTimer;

        private void loadAvailableLeagues()
        {
            availableLeagues = new List<League>();
            var challengeDir = System.AppDomain.CurrentDomain.BaseDirectory + "\\challengeData";
            var dirInfo = new DirectoryInfo(challengeDir);
            FileInfo[] info = dirInfo.GetFiles("challengeData??.*.xml", SearchOption.TopDirectoryOnly);
            foreach (var item in info)
            {
                string xmlPath = item.FullName;
                var league = new League();
                league.XmlFilePath = xmlPath;
                var filename = item.Name;
                int firstDot = filename.IndexOf('.');
                int lastDot = filename.Length - 5;
                var leaguename = filename.Substring(firstDot + 1, lastDot - firstDot);

                if (filename.Contains("1-Month"))
                {
                    //special rule for torment/bloodlines 1 month events
                    if (filename.Contains("HC"))
                    {
                        league.InAppName = "Torment/Bloodlines 1 Month HC";
                        league.ProgressFileName = "Torment-Bloodlines+1-Month+HC";
                        league.UrlName = "Torment%2BBloodlines+1-Month+HC";
                    }
                    else
                    {
                        league.InAppName = "Torment/Bloodlines 1 Month";
                        league.ProgressFileName = "Torment-Bloodlines+1-Month";
                        league.UrlName = "Torment%2BBloodlines+1-Month";
                    }
                }
                else
                {
                    //other leagues
                    league.UrlName = leaguename.Replace("-", "%2F");
                    league.InAppName = leaguename.Replace("-", "/");
                    league.ProgressFileName = leaguename;
                }
                availableLeagues.Add(league);
            }
        }

        private string syncStatus;

        public string SyncStatus
        {
            get { return syncStatus; }
            set
            {
                if (syncStatus != value)
                {
                    syncStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }



        internal void syncProgress()
        {
            try
            {
                SyncStatus = "Now synchronizing the progress of the " + currentLeague.InAppName + " league";
                ProgressSyncer.syncProgress(challengeProgresses, AccountName, currentLeague.UrlName);
                SyncStatus = "Last synchronized: " + DateTime.Now.ToShortTimeString();
            }
            catch (WebException e)
            {
                SyncStatus = "There was an error during synchronisation: " + e.Message;
            }
        }

        private League currentLeague;
        public void initModel()
        {
            IsInitialized = false;
            if (availableLeagues == null)
            {
                loadAvailableLeagues();
            }
            currentLeague = availableLeagues.Where(n => n.InAppName == Properties.Settings.Default.SelectedLeague)?.ElementAt(0);
            if (currentLeague == null)
            {
                currentLeague = availableLeagues.Last();
            }

            var accName = Properties.Settings.Default.AccountName;
            if (loginStatus == LoginStatus.UnChecked && accName != null && accName.Length > 0)
            {
                try
                {
                    CurrentLoginStatus = AccountCheck.checkAccountName(accName);
                }
                catch (WebException e)
                {
                    CurrentLoginStatus = LoginStatus.NetworkError;
                }
            }

            var creator = new DataCreator();
            var info = creator.createChallengeDataListFromXml(currentLeague.XmlFilePath);
            this.leagueInfo = info;
            info.Leaguename = currentLeague.InAppName;
            //try to load saved progress
            challengeProgresses = null;
            var container = SaveLoadPersistentData.loadData(currentLeague.ProgressFileName);
            if (container != null)
            {
                challengeProgresses = container.progress;
                viewOrder = container.order;
                foreach (var item in challengeProgresses)
                {
                    item.rewatchSubprogress();
                }
            }

            //create new progress if failed
            if (challengeProgresses == null)
            {
                challengeProgresses = creator.createChallengeProgressListFromChallengeList(info.ChallengeDatas);
            }

            //observe progress items for changes
            foreach (var item in challengeProgresses)
            {
                item.PropertyChanged += ProgressItemPropertyChanged;
            }
            if (saveProgressTimer != null)
            {
                saveProgressTimer.Dispose();
            }
            saveProgressTimer = new Timer(saveProgressTimerCallback, null, saveProgressInterval, saveProgressInterval);

            //sync if logged in
            if (CurrentLoginStatus == LoginStatus.ValidName)
            {
                syncProgress();
            }

            IsInitialized = true;
        }

        private readonly int saveProgressInterval = 1000;
        private bool isInitialized;

        public bool IsInitialized
        {
            get { return isInitialized; }
            set
            {
                if (isInitialized != value)
                {
                    isInitialized = value;
                };
            }
        }

        public Model()
        {
            IsInitialized = false;
            loginStatus = LoginStatus.UnChecked;
        }

        private void saveProgressTimerCallback(object state)
        {
            if (!isInitialized)
            {
                return;
            }
            if (hasChanged)
            {
                try
                {
                    SaveLoadPersistentData.saveProgressAndOrderAsync(challengeProgresses, viewOrder, currentLeague.ProgressFileName);
                    hasChanged = false;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        private bool hasChanged = false;

        public event PropertyChangedEventHandler PropertyChanged;

        private void ProgressItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SubProgress" || e.PropertyName == "IsDone")
            {
                hasChanged = true;
            }
        }

        private void fillWithExampleData()
        {
            ChallengeData cd = new ChallengeData("Kill Beyond Bosses", "Use the Beyond Map Mod or Zana Mod", ChallengeType.Progressable);
            SubChallengeInfo info = new SubChallengeInfo("poe.trade:", "");
            SubChallengeData scd = new SubChallengeData("Bameth");
            scd.Infos.Add(info);
            info = new SubChallengeInfo("Softcore", "http://poe.trade");
            scd.Infos.Add(info);
            info = new SubChallengeInfo("Hardcore", "http://google.de");
            scd.Infos.Add(info);
            SubChallengeData scd2 = new SubChallengeData("Abaxoth");
            cd.SubChallenges.Add(scd);
            cd.SubChallenges.Add(scd2);


        }

        public void suspend()
        {
            saveProgressTimer.Dispose();
            System.Diagnostics.Debug.WriteLine("Model suspendet sich");
        }

        public void resume()
        {
            saveProgressTimer = new Timer(saveProgressTimerCallback, null, saveProgressInterval, saveProgressInterval);
        }


        public void toogleSubChallengeProgress(SubChallengeProgress progress)
        {
            if (progress == null)
            {
                return;
            }
            switch (progress.CurrentCompletion)
            {
                case SubChallengeCompletionType.Not:
                    progress.CurrentCompletion = SubChallengeCompletionType.Manual;
                    break;
                case SubChallengeCompletionType.Auto:
                case SubChallengeCompletionType.Manual:
                    progress.CurrentCompletion = SubChallengeCompletionType.Not;
                    break;
            }
            hasChanged = true;
        }

        internal void toogleChallengeIsDone(ChallengeData data, ChallengeProgress progress)
        {
            switch (data.Type)
            {
                case ChallengeType.Binary:
                    progress.IsDone = !progress.IsDone;
                    break;
                case ChallengeType.Progressable:
                    //Determine current progress
                    var currentProgress = 0;
                    foreach (var item in progress.SubChallengesProgress)
                    {
                        if (item.CurrentCompletion != SubChallengeCompletionType.Not)
                        {
                            currentProgress++;
                        }
                    }

                    if (currentProgress >= data.NeedForCompletion)
                    {
                        //Need to uncomplete subs
                        foreach (var item in progress.SubChallengesProgress.Where(x => x.CurrentCompletion == SubChallengeCompletionType.Auto))
                        {
                            item.CurrentCompletion = SubChallengeCompletionType.Not;
                        }
                    }
                    else
                    {
                        //Need to complete subs
                        foreach (var item in progress.SubChallengesProgress)
                        {
                            if (currentProgress < data.NeedForCompletion && item.CurrentCompletion == SubChallengeCompletionType.Not)
                            {
                                item.CurrentCompletion = SubChallengeCompletionType.Auto;
                                currentProgress++;
                            }
                        }
                    }
                    break;
            }
        }

        internal void resetProgress()
        {
            foreach (var item in challengeProgresses)
            {
                item.IsDone = false;
                item.Progress = 0;
                if (item.SubChallengesProgress != null)
                {
                    foreach (var subItem in item.SubChallengesProgress)
                    {
                        subItem.CurrentCompletion = SubChallengeCompletionType.Not;
                    }
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
