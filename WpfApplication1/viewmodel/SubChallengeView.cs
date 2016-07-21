using Poe_Challenge_Tracker.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Poe_Challenge_Tracker.viewmodel
{
    public class SubChallengeView : INotifyPropertyChanged
    {

        SubChallengeData data;
        internal SubChallengeData Data
        {
            get { return data; }
            set { data = value; }
        }

        SubChallengeProgress progress;
        internal SubChallengeProgress Progress
        {
            get { return progress; }
            set { progress = value; }
        }

        public bool IsDone
        {
            get
            {
                return (Progress.CompletionType == SubChallengeCompletionType.Auto)
                  || (Progress.CompletionType == SubChallengeCompletionType.Manual);
            }
        }

        public string Description
        {
            get { return Data.Description; }
        }

        private List<SubChallengeInfoView> infoViews;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public List<SubChallengeInfoView> InfoViews
        {
            get { return infoViews; }
        }

        public SubChallengeView(SubChallengeData data, SubChallengeProgress progress)
        {
            Data = data;
            Progress = progress;
            infoViews = new List<SubChallengeInfoView>();
            progress.PropertyChanged += Progress_PropertyChanged;
        }

        private void Progress_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("IsDone");
        }
    }
}
