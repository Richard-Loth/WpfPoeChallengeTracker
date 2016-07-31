using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfPoeChallengeTracker.viewmodel
{
    public class TileVisibility : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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
            set
            {
                viewOptions = value;
                NotifyPropertyChanged("IsOptionsVisible");
            }
        }

        private bool viewChallenges;
        private bool isInitialized;

        public bool ViewChallenges
        {
            get { return viewChallenges; }
            set
            {
                viewChallenges = value;
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

        public TileVisibility()
        {
            viewStatus = true;
            viewOptions = true;
            viewChallenges = true;
            isInitialized = false;
        }

        public bool IsInitialized
        {
            get
            {
                return isInitialized;
            }
            set
            {
                isInitialized = value;
            }
        }
    }
}
