using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WpfPoeChallengeTracker.model
{
    public class ChallengeProgress : INotifyPropertyChanged
    {
        //only for ProgressableNoSubs challenges
        private int progress;

        public int Progress
        {
            get
            {
                switch (type)
                {
                    case ChallengeType.Binary:
                        return isDone ? 1 : 0;
                    case ChallengeType.Progressable:
                        var completedSubs = 0;
                        foreach (var subProgress in subChallengesProgress)
                        {
                            if (subProgress.CurrentCompletion != SubChallengeCompletionType.Not)
                            {
                                completedSubs++;
                            }
                        }
                        return completedSubs;
                    case ChallengeType.ProgressableNoSubs:
                        return progress;
                }
                return 0;
            }

            set
            {
                if (progress!= value)
                {
                    progress = value;
                    NotifyPropertyChanged();
                }
            }
        }
        


        private List<SubChallengeProgress> subChallengesProgress;

        public List<SubChallengeProgress> SubChallengesProgress
        {
            get { return subChallengesProgress; }
            set { subChallengesProgress = value; }
        }

        public void AddSubChallengeProgress(SubChallengeProgress subProgress)
        {
            if (subProgress == null)
            {
                return;
            }
            subChallengesProgress.Add(subProgress);
            subProgress.PropertyChanged += SubProgress_PropertyChanged;
        }

        /// <summary>
        /// When the progress is loaded from deserialized file, subprogress is no longer watched. 
        /// So we need to watch it again.
        /// </summary>

        public void rewatchSubprogress()
        {
            foreach (var item in subChallengesProgress)
            {
                item.PropertyChanged += SubProgress_PropertyChanged;
            }
        }

        private void SubProgress_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("SubProgress");
        }



        private ChallengeType type;

        public ChallengeType Type
        {
            get { return type; }
            set { type = value; }
        }

        private bool isDone;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool IsDone
        {
            get { return isDone; }
            set
            {
                if (isDone != value)
                {
                    isDone = value;
                    NotifyPropertyChanged(); 
                }
            }
        }

        public ChallengeProgress()
        {

        }

        public ChallengeProgress(ChallengeType type)
        {
            this.type = type;
            switch (type)
            {
                case ChallengeType.Binary:
                    isDone = false;
                    break;
                case ChallengeType.Progressable:
                    subChallengesProgress = new List<SubChallengeProgress>();
                    break;
                case ChallengeType.ProgressableNoSubs:
                    isDone = false;
                    progress = 0;
                    break;
            }
        }
    }
}
