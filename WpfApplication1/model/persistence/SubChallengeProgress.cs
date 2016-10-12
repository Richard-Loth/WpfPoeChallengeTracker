using System.ComponentModel;
using System.Runtime.Serialization;

namespace WpfPoeChallengeTracker.model
{
    public class SubChallengeProgress : INotifyPropertyChanged
    {
        private SubChallengeCompletionType completionType;

        public SubChallengeCompletionType CurrentCompletion
        {
            get { return completionType; }
            set
            {
                if (completionType != value)
                {
                    completionType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CompletionType"));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private int progress;

        public int CurrentProgress
        {
            get { return progress; }
            set
            {
                if (progress != value)
                {
                    progress = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
                }
            }
        }


        public SubChallengeProgress()
        {
            progress = 0;
        }
    }
}