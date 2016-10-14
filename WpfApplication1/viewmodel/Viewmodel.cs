
using WpfPoeChallengeTracker.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation;
using WpfPoeChallengeTracker.viewmodel;
using WpfPoeChallengeTracker.model.persistence;
using System.Text.RegularExpressions;

namespace WpfPoeChallengeTracker.viewmodel
{
    public enum CompletedBehaviour
    {
        DO_NOTHING, SORT_TO_END, HIDE
    }


    public class Viewmodel : INotifyPropertyChanged
    {
        private CompletedBehaviour completedBehaviour;

        public ObservableCollection<LeagueView> AvailableLeagues
        {
            get
            {
                if (!isInitialized)
                {
                    return null;
                }
                return leagueviews;
            }
        }



        public string SyncStatusText { get
            {
                return model.SyncStatus;
            }
        }



        public string LoginStatusText { get
            {
                switch (model.CurrentLoginStatus)
                {
                    case LoginStatus.NoAccountName:
                        return "Not logged in";
                    case LoginStatus.InvalidName:
                        return "The specified account name is invalid";
                    case LoginStatus.ValidNamePrivateProfile:
                        return "Your profile tab is set to private";
                    case LoginStatus.ValidNamePrivateChallenges:
                        return "Your challenges tab is set to private";
                    case LoginStatus.ValidName:
                        return "Logged in as " + model.AccountName;
                    case LoginStatus.NetworkError:
                        return "A network error occured while logging in";
                }
                return "";
            }
        }

        private ObservableCollection<LeagueView> leagueviews;


        public string AccountName
        {
            get { return model.AccountName; }
            set
            {
                if (model.AccountName != value)
                {
                    model.AccountName = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("LoginStatusText");
                } }
        }

        public LoginStatus CurrentLoginStatus
        {
            get
            {
                return model.CurrentLoginStatus;
            }
            set
            {
                if (model.CurrentLoginStatus != value)
                {
                    model.CurrentLoginStatus = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("LoginStatusText");
                }
            }
        }

        public void changeCompletedBehaviour(CompletedBehaviour newBehaviour)
        {
            completedBehaviour = newBehaviour;
       
            var challengeFilter = new RegexFilter(filterText);
            applyNewChallengeFiltering(challengeFilter, newBehaviour);
            if (newBehaviour == CompletedBehaviour.SORT_TO_END)
            {
                doAutoSort();
            }
        }

        private Timer syncProgressTimer;

        public void syncProgress()
        {
            if (syncProgressTimer == null)
            {
                syncProgressTimer = new Timer(syncProgressTimerCallback, null, 0, Timeout.Infinite);
            }
            else
            {
                syncProgressTimer.Change(0, Timeout.Infinite);
            }
            
        }

        private void syncProgressTimerCallback(object state)
        {
            model.syncProgress();
        }

        public void leagueViewClicked(LeagueView view)
        {

            foreach (var item in leagueviews)
            {
                item.setIsCheckedPlain(item == view);
            }
            if (view.Name == model.LeagueInfo.Leaguename)
            {
                return;
            }
            Properties.Settings.Default.SelectedLeague = view.Name;
            isInitialized = false;
            tileVisibility.IsInitialized = false;

            NotifyPropertyChanged("IsInitialized");
            NotifyPropertyChanged("CurrentLeague");
        }

        private ObservableCollection<ChallengeView> challengeViews;

        public ObservableCollection<ChallengeView> ChallengeViews
        {
            get { return challengeViews; }
            set { challengeViews = value; }
        }

        private RemainingCountdown remainingCountdown;

        public RemainingCountdown Remaining
        {
            get { return remainingCountdown; }
            set { remainingCountdown = value; }
        }




        private Model model;

        public Model Model
        {
            get { return model; }
            private set { model = value; }
        }



        internal void changeFilterText(string filter)
        {
            var challengeFilter = new RegexFilter(filter);
            this.filterText = filter;
            applyNewChallengeFiltering(challengeFilter, completedBehaviour);
        }

        private bool isInitialized;

        public bool IsInitialized
        {
            get { return isInitialized; }
            set
            {
                if (value != isInitialized)
                {
                    isInitialized = value;
                    NotifyPropertyChanged();
                };
            }
        }
        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private List<ChallengeView> backupList;
        private string filterText;


        private void applyNewChallengeFiltering(ChallengeViewFilter viewfilter, CompletedBehaviour completedBehaviour)

        {
            if (!isInitialized)
            {
                return;
            }
            if (backupList == null)
            {
                backupList = new List<ChallengeView>();
            }
            Application.Current.Dispatcher.Invoke(
             () =>
                 {
                     //restore original list
                     if (backupList.Count > 0 && ChallengeViews.Count < backupList.Count)
                     {
                         challengeViews.Clear();
                         foreach (var item in backupList)
                         {
                             challengeViews.Add(item);
                         }
                     }

                     //fill backup list with all items
                     backupList.Clear();
                     foreach (var item in challengeViews)
                     {
                         backupList.Add(item);
                     }
                     var visibleViews = viewfilter.calculateVisibleViews(backupList);

                     //remove non matching items
                     challengeViews.Clear();
                     foreach (var item in backupList)
                     {
                         if (completedBehaviour == CompletedBehaviour.HIDE && item.IsDone)
                         {
                             continue;
                         }
                         if (visibleViews.Contains(item.Id))
                         {
                             challengeViews.Add(item);
                         }
                     }
                     if (completedBehaviour == CompletedBehaviour.SORT_TO_END)
                     {
                         doAutoSort();
                     }
                 });
        }

        public void suspend()
        {
            model.suspend();
            if (Remaining!= null)
            {
                Remaining.Dispose(); 
            }
            Properties.Settings.Default.Save();
        }

        //public void resume()
        //{
        //    model.resume();
        //    NotifyPropertyChanged("Headerline");
        //}

        private ISet<int> calculateVisibleViews(string filter)
        {
            var set = new HashSet<int>();
            filter = filter.ToLower();
            filter.Trim();
            if (filter.Length > 0)
            {
                var regex = new Regex("*" + filter + "*");
                foreach (var view in backupList)
                {
                    var applies = false;
                    var data = view.Data;
                    //search in name and description

                    //applies = data.Name.ToLower().Contains(filter) || data.Description.ToLower().Contains(filter);
                    var match = regex.Match(data.Name.ToLower() + " " + data.Description.ToLower());

                    applies = match.Success;
                    if (!applies)
                    {
                        //search in subchallenges
                        if (data.SubChallenges != null)
                        {
                            foreach (var subData in data.SubChallenges)
                            {
                                if (subData.Description.ToLower().Contains(filter))
                                {
                                    applies = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (applies)
                    {
                        set.Add(view.Id);
                    }
                }
            }
            else
            {
                foreach (var item in challengeViews)
                {
                    set.Add(item.Id);
                }
            }

            return set;
        }


        public string LeagueName
        {
            get
            {
                var leaguename = model.LeagueInfo?.Leaguename;
                if (leaguename == null || leaguename.Length == 0)
                {
                    return "";
                }
                return leaguename + " League";
            }
        }

        Timer collapseTimer;

        internal void collapseAll()
        {
            collapseTimer = new Timer(collapseCallBack, null, 0, Timeout.Infinite);
        }


        private async void collapseCallBack(object state)
        {
            foreach (var item in challengeViews)
            {
                Application.Current.Dispatcher.Invoke(() =>
           {
               item.IsCollapsed = true;
           });
                await Task.Delay(10);
            }
            collapseTimer.Dispose();
        }




        internal void unCollapseAll()
        {
            foreach (var item in challengeViews)
            {
                item.IsCollapsed = false;
                item.setIsCollapsedWithoutNotify(false);
            }

        }

        public void resetProgressAndOrder()
        {
            resetOrder();
            model.resetProgress();
        }

        private void resetOrder()
        {
            applyNewChallengeFiltering(new ContainsFilter(""), CompletedBehaviour.DO_NOTHING);
            challengeViews.OrderBy(x => x.Id);

            var tempList = new System.Collections.Generic.List<ChallengeView>();
            foreach (var item in challengeViews)
            {
                tempList.Add(item);
            }
            tempList.Sort((x, y) =>
            {
                if (x.Id < y.Id)
                {
                    return -1;
                }
                return 1;
            });
            challengeViews.Clear();
            foreach (var item in tempList)
            {
                challengeViews.Add(item);
            }
        }



        private void doAutoSort()
        {
            if (!isInitialized)
            {
                return;
            }
            //find highest index of undone challenge
            var highestIndex = challengeViews.IndexOf(challengeViews.LastOrDefault(x => !x.IsDone));
            if (highestIndex <= 0)
            {
                //all challenges or all but the first challenge are done, nothing to do
                return;
            }
            //get all done challenges in separate list
            var doneChallenges = new List<ChallengeView>();
            for (int i = highestIndex; i >= 0; i--)
            {
                var item = challengeViews.ElementAt(i);
                if (item.IsDone)
                {
                    doneChallenges.Add(item);
                }
            }
            //remove done challenges from original list
            foreach (var item in doneChallenges)
            {
                challengeViews.Remove(item);
            }
            //place all done challenges at the index of the first undone challenge
            foreach (var item in doneChallenges)
            {
                if (highestIndex + 1 > challengeViews.Count)
                {
                    challengeViews.Add(item);
                }
                else
                {
                    challengeViews.Insert(highestIndex, item);
                }
            }

        }

        public string ChallengesCompleted
        {
            get
            {
                if (challengeViews == null)
                {
                    return "";
                }
                ICollection<ChallengeView> relevantList = null;
                if (backupList == null)
                {
                    relevantList = challengeViews;
                }
                else
                {
                    relevantList = backupList;
                }
                return relevantList.Count(n => n.IsDone) + "/" + relevantList.Count;
            }
        }

        public string CountCompleted
        {
            get
            {
                if (!IsInitialized)
                {
                    return "";
                }
                ICollection<ChallengeView> relevantList = null;
                if (backupList == null)
                {
                    relevantList = challengeViews;
                }
                else
                {
                    relevantList = backupList;
                }
                return relevantList.Count(n => n.IsDone) + "/" + relevantList.Count + " completed";

            }
        }

        public void initViewmodel()
        {
            challengeViewsInitialized = false;
            backupList = null;
            challengeViews = new ObservableCollection<ChallengeView>();
            challengeViews.CollectionChanged += ChallengeViews_CollectionChanged;
            generateChallengeViews();
            if (leagueviews == null)
            {
                generateLeagueViews();
            }

            foreach (var item in challengeViews)
            {
                item.PropertyChanged += ChallengeViewItemChanged;
            }
            orderViews();
            IsInitialized = true;
            TileVisibilityProp.IsInitialized = true;
            if (Remaining != null)
            {
                Remaining.Dispose();
            }
            Remaining = new RemainingCountdown(model);

            applyNewChallengeFiltering(new RegexFilter(""),completedBehaviour);

            //everything has changed
            foreach (var item in this.GetType().GetProperties())
            {
                NotifyPropertyChanged(item.Name);
            }
        }

        private void generateLeagueViews()
        {
            leagueviews = new ObservableCollection<LeagueView>();
            foreach (var item in model.AvailableLeagues)
            {
                var view = new LeagueView(this);
                view.LeagueData = item;
                leagueviews.Add(view);
                if (model.LeagueInfo.Leaguename == item.InAppName)
                {
                    view.setIsCheckedPlain(true);
                }
            }
        }

        public Viewmodel(Model model)
        {
            Model = model;
            IsInitialized = false;
            filterText = "";
            TileVisibilityProp = new TileVisibility();
            TileVisibilityProp.IsInitialized = false;
            model.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SyncStatus")
            {
                NotifyPropertyChanged("SyncStatusText");
            }
        }

        private TileVisibility tileVisibility;

        public TileVisibility TileVisibilityProp
        {
            get { return tileVisibility; }
            set { tileVisibility = value; }
        }

        private void ChallengeViewItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDone")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CountCompleted"));
                if (completedBehaviour == CompletedBehaviour.HIDE)
                {
                    var challengeFilter = new RegexFilter(filterText);
                    applyNewChallengeFiltering(challengeFilter, completedBehaviour);
                }
                if (completedBehaviour == CompletedBehaviour.SORT_TO_END)
                {
                    doAutoSort();
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
        }

        bool challengeViewsInitialized;

        public event PropertyChangedEventHandler PropertyChanged;

        private void orderViews()
        {
            var order = model.ViewOrder;
            if (order != null && order.Count == challengeViews.Count)
            {
                var newList = new List<ChallengeView>();
                foreach (var orderIndex in order)
                {
                    newList.Add(challengeViews.ElementAt(orderIndex));
                }
                challengeViews.Clear();
                foreach (var newOrderedViewItem in newList)
                {
                    challengeViews.Add(newOrderedViewItem);
                }
            }
        }

        private void ChallengeViews_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (challengeViewsInitialized && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var idList = new List<int>(challengeViews.Count);
                foreach (var item in challengeViews)
                {
                    idList.Add(item.Id);
                }
                model.ViewOrder = idList;
            }
        }

        internal void challengeIsDoneClick(ChallengeView view)
        {
            model.toogleChallengeIsDone(view.Data, view.Progress);
        }

        private void generateChallengeViews()
        {
            List<ChallengeData> data = Model.LeagueInfo.ChallengeDatas;
            List<ChallengeProgress> progress = Model.ChallengeProgresses;
            int count = data.Count;
            if (data.Count != progress.Count)
            {
                throw new InvalidOperationException(ErrorMessages.PROGRESS_DIFFERENT_SIZE);
            }

            for (int i = 0; i < count; i++)
            {
                ChallengeData currentData = data.ElementAt(i);
                ChallengeProgress currentProgress = progress.ElementAt(i);
                ChallengeView newView = new ChallengeView(currentData, currentProgress, i);

                if (currentData.Type == ChallengeType.Progressable)
                {
                    int subCount = currentData.SubChallenges.Count;
                    var subProgList = currentProgress.SubChallengesProgress;
                    while (subCount < subProgList.Count)
                    {
                        subProgList.RemoveAt(subProgList.Count - 1);
                        //throw new InvalidOperationException(ErrorMessages.SUBPROGRESS_DIFFERENT_SIZE);
                    }
                    while (subCount > subProgList.Count)
                    {
                        subProgList.Add(new SubChallengeProgress());
                        //throw new InvalidOperationException(ErrorMessages.SUBPROGRESS_DIFFERENT_SIZE);
                    }
                    for (int j = 0; j < subCount; j++)
                    {
                        SubChallengeData subData = currentData.SubChallenges.ElementAt(j);
                        SubChallengeProgress subProgress = currentProgress.SubChallengesProgress.ElementAt(j);
                        SubChallengeView newSubView = new SubChallengeView(subData, subProgress);
                        int subSubCount = subData.Infos.Count;
                        for (int k = 0; k < subSubCount; k++)
                        {
                            newSubView.InfoViews.Add(new SubChallengeInfoView(subData.Infos.ElementAt(k)));
                        }
                        newView.addSubChallengeView(newSubView);
                    }
                }
                challengeViews.Add(newView);
                //newView.PropertyChanged += view_PropertyChanged;
            }
            challengeViewsInitialized = true;
        }

        public void subChallengeDescriptionTapped(SubChallengeView subView)
        {
            model.toogleSubChallengeProgress(subView.Progress);
        }
    }
}
