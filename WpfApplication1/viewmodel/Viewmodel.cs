
using Poe_Challenge_Tracker.model;
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

namespace Poe_Challenge_Tracker.viewmodel
{
    public class Viewmodel : INotifyPropertyChanged
    {

        private ObservableCollection<ChallengeView> challengeViews;

        public ObservableCollection<ChallengeView> ChallengeViews
        {
            get { return challengeViews; }
            set { challengeViews = value; }
        }


        private Model model;

        public Model Model
        {
            get { return model; }
            private set { model = value; }
        }

        private bool viewStatus;

        public bool ViewStatus
        {
            get { return viewStatus; }
            set
            {
                viewStatus = value;
                NotifyPropertyChanged("IsStatusVisible");
            }
        }

        private bool viewOptions;

        public bool ViewOptions
        {
            get { return viewOptions; }
            set { viewOptions = value;
                NotifyPropertyChanged("IsOptionsVisible");
            }
        }

        private bool viewChallenges;

        public bool ViewChallenges
        {
            get { return viewChallenges; }
            set { viewChallenges = value;
                NotifyPropertyChanged("IsChallengesVisible");
            }
        }

        public Visibility IsStatusVisible
        {
            get
            {
                if (isInitialized && viewStatus)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        public Visibility IsOptionsVisible
        {
            get
            {
                if (isInitialized && viewOptions)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility IsChallengesVisible
        {
            get
            {
                if (isInitialized && viewChallenges) return Visibility.Visible;
                return Visibility.Collapsed;
            }
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

        public void applyNewFilterText(string filter)

        {

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
                         if (filter.Length == 0 || visibleViews.Contains(item.Id))
                         {
                             challengeViews.Add(item);
                         }
                     }
                     if (model.AutoSortEnabled)
                     {
                         doAutoSort();
                     }

                 });
        }

        public void suspend()
        {
            model.suspend();
            updateHeaderTimer.Dispose();
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
            applyNewFilterText("");
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


        private TimeSpan calculateRemainingTime(DateTime leagueEnd)
        {
            var now = DateTime.UtcNow;
            var span = leagueEnd.Subtract(now);
            return span;
        }

        private TimeSpan calculateApproxRemainingTime()
        {
            //90 days as standard length
            var approxLeagueLength = new TimeSpan(90, 0, 0, 0);
            var now = DateTime.UtcNow;
            var start = model.LeagueInfo.LeagueStartedOn;
            var end = start.Add(approxLeagueLength);
            return end.Subtract(now);
        }
        Timer updateHeaderTimer;

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

        public string RemainingDays
        {
            get
            {
                return remainingTime?.Days.ToString("00");
            }
        }

        public string RemainingHours
        {
            get
            {
                return remainingTime?.Hours.ToString("00");
            }
        }

        public string RemainingMinutes
        {
            get
            {
                return remainingTime?.Minutes.ToString("00");
            }
        }

        public string RemainingSeconds
        {
            get
            {
                return remainingTime?.Seconds.ToString("00");
            }
        }

        private TimeSpan? remainingTime;

        public string Headerline
        {
            get
            {
                if (!IsInitialized)
                {
                    return "";
                }
                var endTime = model.LeagueInfo.LeagueEndsOn;
                bool isApprox;
                bool hasEnded = false; ;
                TimeSpan remaining;
                //UpdateTile(challengePart);
                if (DateTime.MinValue == endTime)
                {
                    isApprox = true;
                    remaining = calculateApproxRemainingTime();
                }
                else
                {
                    isApprox = false;
                    remaining = calculateRemainingTime(endTime);
                }
                string remainingPart = "";
                var days = remaining.Days;

                if (!hasEnded)
                {
                    remainingPart += " remaining " + (isApprox ? "(approx.)" : "");
                }
                else
                {
                    remainingPart = "League has ended";
                }
                return remainingPart;
            }
        }




        public async Task initViewodel(Uri xmlUri)
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
            // Application.Current.Dispatcher.Invoke(
            //  () =>
            //{
            IsInitialized = true;
            NotifyPropertyChanged("Leaguename");
            NotifyPropertyChanged("ChallengeViews");
            NotifyPropertyChanged("LeagueName");
            NotifyPropertyChanged("CountCompleted");
            NotifyPropertyChanged("AutoSortEnabled");
            NotifyPropertyChanged("IsStatusVisible");
            NotifyPropertyChanged("IsOptionsVisible");
            NotifyPropertyChanged("IsChallengesVisible");

            //remaining time things
            remainingTime = calculateRemainingTime(model.LeagueInfo.LeagueEndsOn);
            NotifyPropertyChanged("RemainingSeconds");
            NotifyPropertyChanged("RemainingMinutes");
            NotifyPropertyChanged("RemainingHours");
            NotifyPropertyChanged("RemainingDays");
            //});
            remainingTimer = new Timer(remainingTimerCallBack, null, 1000, 1000);
        }

        private Timer remainingTimer;

        private void remainingTimerCallBack(object sender)
        {
            var oldMinutes = remainingTime?.Minutes;
            var oldHours = remainingTime?.Hours;
            var oldDays = remainingTime?.Days;
            var remain = calculateRemainingTime(model.LeagueInfo.LeagueEndsOn);
            remainingTime = remain;
            NotifyPropertyChanged("RemainingSeconds");
            if (remain.Minutes != oldMinutes)
            {
                NotifyPropertyChanged("RemainingMinutes");
            }
            if (remain.Hours != oldHours)
            {
                NotifyPropertyChanged("RemainingHours");
            }
            if (remain.Days != oldDays)
            {
                NotifyPropertyChanged("RemainingDays");
            }
        }



        public Viewmodel(Model model)
        {
            Model = model;
            IsInitialized = false;
            viewStatus = true;
            viewOptions = true;
            viewChallenges = true;
        }

        private void ChallengeViewItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDone")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CountCompleted"));
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
                newView.PropertyChanged += view_PropertyChanged;
            }
            challengeViewsInitialized = true;
        }

        private void view_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDone" && model.AutoSortEnabled)
            {
                doAutoSort();
            }
        }

        public void subChallengeDescriptionTapped(SubChallengeView subView)
        {
            model.toogleSubChallengeProgress(subView.Progress);
        }


    }
}
