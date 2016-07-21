using Poe_Challenge_Tracker.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Poe_Challenge_Tracker.viewmodel
{
    public class ChallengeView : INotifyPropertyChanged
    {

        public string Description
        {
            get
            {
                return data.Description;
            }
        }

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }


        private bool isCollapsed;
        public bool IsCollapsed
        {
            get { return isCollapsed; }
            set
            {
                if (isCollapsed != value)
                {
                    isCollapsed = value;
                    NotifyPropertyChanged(); 
                }
            }
        }

        public void setIsCollapsedWithoutNotify(bool isCollapsed)
        {
            this.isCollapsed = isCollapsed;
        }

        private ChallengeData data;
        public ChallengeData Data
        {
            get { return data; }
            set { data = value; }
        }

        private ChallengeProgress progress;
        public ChallengeProgress Progress
        {
            get { return progress; }
            set { progress = value; }
        }

        public string ChallengeName
        {
            get { return Data.Name; }
            private set
            {
                if (value != Data.Name)
                {
                    Data.Name = value;
                    NotifyPropertyChanged("ChallengeName");
                }
            }
        }

        public string CurrentProgress
        {
            get
            {
                if (Data.Type == ChallengeType.Binary)
                {
                    return "";
                }
                if (Data.Type == ChallengeType.Progressable)
                {
                    return Progress.Progress + "/" + Data.NeedForCompletion;
                }

                return "";
            }
            //private set
            //{
            //    if (value != progress)
            //    {
            //        progress = value;
            //        NotifyPropertyChanged("Progress");
            //    }
            //}
        }


        public bool IsDone
        {
            get
            {
                if (Data.Type == ChallengeType.Binary)
                {
                    return Progress.Progress == 1;
                }
                if (Data.Type == ChallengeType.Progressable)
                {
                    return Progress.Progress >= Data.NeedForCompletion;
                }
                return false;
            }
        }

        private ObservableCollection<SubChallengeView> subChallenges;
        public ObservableCollection<SubChallengeView> SubChallenges
        {
            get { return subChallenges; }
            set { subChallenges = value; }
        }

        public void addSubChallengeView(SubChallengeView subView)
        {
            subView.PropertyChanged += SubView_PropertyChanged;
            subChallenges.Add(subView);
        }

        private void SubView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("CurrentProgress");
            NotifyPropertyChanged("IsDone");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ChallengeView(ChallengeData data, ChallengeProgress progress, int id)
        {
            Data = data;
            Progress = progress;
            progress.PropertyChanged += SubView_PropertyChanged;
            Id = id;
            if (data.Type == ChallengeType.Progressable)
            {
                subChallenges = new ObservableCollection<SubChallengeView>();
            }
        }

    }


}
