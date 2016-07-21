using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Poe_Challenge_Tracker.model
{
    public class ChallengeProgress : INotifyPropertyChanged
    {
        public int Progress
        {
            get
            {
                if (type == ChallengeType.Binary)
                {
                    return isDone ? 1 : 0;
                }
                var progress = 0;
                foreach (var subProgress in subChallengesProgress)
                {
                    if (subProgress.CompletionType != SubChallengeCompletionType.Not)
                    {
                        progress++;
                    }
                }
                return progress;
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

        private void SubProgress_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("SubProgress");
        }

        private ChallengeType type;
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
                isDone = value;
                NotifyPropertyChanged("IsDone");
            }
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
                default:
                    break;
            }
        }
    }
}
