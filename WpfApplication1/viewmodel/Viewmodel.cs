
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

namespace WpfPoeChallengeTracker.viewmodel
{
    public enum CompletedBehaviour
    {
        DO_NOTHING, SORT_TO_END, HIDE
    }


    public class Viewmodel : INotifyPropertyChanged
    {
        private CompletedBehaviour completedBehaviour;

        public void changeCompletedBehaviour(CompletedBehaviour newBehaviour)
        {
            completedBehaviour = newBehaviour;
            applyNewChallengeFiltering(filterText, newBehaviour);
            if (newBehaviour == CompletedBehaviour.SORT_TO_END)
            {
                doAutoSort();
            }
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
            this.filterText = filter;
            applyNewChallengeFiltering(filter, completedBehaviour);
        }



        public bool? AutoSortEnabled
        {
            get
            {
                return model.AutoSortEnabled;
            }
            set
            {
                bool? b = value;
                if (b == null)
                {
                    model.AutoSortEnabled = false;
                }
                else
                {
                    model.AutoSortEnabled = (bool)value;
                    if (model.AutoSortEnabled)
                    {
                        doAutoSort();
                    }


                }
            }
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


        private void applyNewChallengeFiltering(string filter, CompletedBehaviour completedBehaviour)

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
                     var visibleViews = calculateVisibleViews(filter);

                     //remove non matching items
                     challengeViews.Clear();
                     foreach (var item in backupList)
                     {
                         if (completedBehaviour == CompletedBehaviour.HIDE && item.IsDone)
                         {
                             continue;
                         }
                         if (filter.Length == 0 || visibleViews.Contains(item.Id))
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
            Remaining.Dispose();
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
                foreach (var view in backupList)
                {
                    var applies = false;
                    var data = view.Data;
                    //search in name and description

                    applies = data.Name.ToLower().Contains(filter) || data.Description.ToLower().Contains(filter);
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
            applyNewChallengeFiltering("", CompletedBehaviour.DO_NOTHING);
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

        
        public async Task initViewmodel(Uri xmlUri)
        {
            challengeViewsInitialized = false;
            challengeViews = new ObservableCollection<ChallengeView>();
            challengeViews.CollectionChanged += ChallengeViews_CollectionChanged;

            try
            {
                generateChallengeViews();
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("InvalidOperation: " + e.GetBaseException());
                if (e.Message == ErrorMessages.PROGRESS_DIFFERENT_SIZE || e.Message == ErrorMessages.SUBPROGRESS_DIFFERENT_SIZE)

                {
                    await SaveLoadPersistentData.deleteSavedProgress(model.LeagueInfo.Leaguename);
                    await model.initModel(xmlUri);
                    generateChallengeViews();
                }

            }
            foreach (var item in challengeViews)
            {
                item.PropertyChanged += ChallengeViewItemChanged;
            }
            orderViews();
            IsInitialized = true;
            NotifyPropertyChanged("Leaguename");
            NotifyPropertyChanged("ChallengeViews");
            NotifyPropertyChanged("LeagueName");
            NotifyPropertyChanged("CountCompleted");
            NotifyPropertyChanged("AutoSortEnabled");
           

            Remaining = new RemainingCountdown(model);
            NotifyPropertyChanged("Remaining");

            TileVisibilityProp.IsInitialized = true;
            NotifyPropertyChanged("TileVisibilityProp");
            NotifyPropertyChanged("IsStatusVisible");
            NotifyPropertyChanged("IsOptionsVisible");
            NotifyPropertyChanged("IsChallengesVisible");
        }

        public Viewmodel(Model model)
        {
            Model = model;
            IsInitialized = false;
            filterText = "";
            TileVisibilityProp = new TileVisibility();
            TileVisibilityProp.IsInitialized = false;
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
                    applyNewChallengeFiltering(filterText, completedBehaviour);
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
                    if (subCount != currentProgress.SubChallengesProgress.Count)
                    {
                        throw new InvalidOperationException(ErrorMessages.SUBPROGRESS_DIFFERENT_SIZE);
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
