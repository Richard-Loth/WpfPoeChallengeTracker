using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Poe_Challenge_Tracker.model
{
    public class Model
    {

        private List<ChallengeProgress> challengeProgresses;
        public List<ChallengeProgress> ChallengeProgresses
        {
            get { return challengeProgresses; }
            set { challengeProgresses = value; }
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

        public bool AutoSortEnabled
        {
            get
            {
                if (settings == null)
                {
                    return false;
                }
                return settings.AutoSortCompleted;
            }
            set
            {
                if (settings.AutoSortCompleted != value)
                {
                    settings.AutoSortCompleted = value;
                    hasChanged = true;
                }

            }
        }


        private LeagueInfo leagueInfo;

        public LeagueInfo LeagueInfo
        {
            get { return leagueInfo; }
            set { leagueInfo = value; }
        }

        private Settings settings;

        private Timer saveProgressTimer;

        public async Task initModel(Uri xmlUri)
        {
            var creator = new DataCreator();
            var info = creator.createChallengeDataListFromXml(xmlUri);
            this.leagueInfo = info;

            //try to load saved progress
            var container =  SaveLoadPersistentData.loadData(info.Leaguename);
            if (container != null)
            {
                challengeProgresses = container.progress;
                viewOrder = container.order;
                settings = container.settings;
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
            //create new settings if failed
            if (settings == null)
            {
                settings = new Settings();
                settings.AutoSortCompleted = false;
            }

            //observe progress items for changes
            foreach (var item in challengeProgresses)
            {
                item.PropertyChanged += ProgressItemPropertyChanged;
            }
            saveProgressTimer = new Timer(saveProgressTimerCallback, null, saveProgressInterval, saveProgressInterval);
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
        }

        private void saveProgressTimerCallback(object state)
        {
            if (hasChanged)
            {
                try
                {

                    SaveLoadPersistentData.saveProgressAndOrderAsync(challengeProgresses, viewOrder, settings, LeagueInfo.Leaguename);
                    hasChanged = false;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        private bool hasChanged = false;

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
            switch (progress.CompletionType)
            {
                case SubChallengeCompletionType.Not:
                    progress.CompletionType = SubChallengeCompletionType.Manual;
                    break;
                case SubChallengeCompletionType.Auto:
                case SubChallengeCompletionType.Manual:
                    progress.CompletionType = SubChallengeCompletionType.Not;
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
                        if (item.CompletionType != SubChallengeCompletionType.Not)
                        {
                            currentProgress++;
                        }
                    }

                    if (currentProgress >= data.NeedForCompletion)
                    {
                        //Need to uncomplete subs
                        foreach (var item in progress.SubChallengesProgress.Where(x => x.CompletionType == SubChallengeCompletionType.Auto))
                        {
                            item.CompletionType = SubChallengeCompletionType.Not;
                        }
                    }
                    else
                    {
                        //Need to complete subs
                        foreach (var item in progress.SubChallengesProgress)
                        {
                            if (currentProgress < data.NeedForCompletion && item.CompletionType == SubChallengeCompletionType.Not)
                            {
                                item.CompletionType = SubChallengeCompletionType.Auto;
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
                if (item.SubChallengesProgress != null)
                {
                    foreach (var subItem in item.SubChallengesProgress)
                    {
                        subItem.CompletionType = SubChallengeCompletionType.Not;
                    }
                }
            }
        }
    }
}
