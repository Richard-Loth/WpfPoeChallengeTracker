using System.ComponentModel;
using System.Runtime.Serialization;

namespace Poe_Challenge_Tracker.model
{
    public class SubChallengeProgress : INotifyPropertyChanged
    {
        private SubChallengeCompletionType completionType;

        public SubChallengeCompletionType CompletionType
        {
            get { return completionType; }
            set
            {
                if (completionType != value)
                {
                    completionType = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("CompletionType"));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public SubChallengeProgress()
        {
            completionType = SubChallengeCompletionType.Not;
        }
    }
}