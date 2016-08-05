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
                Properties.Settings.Default.ViewStatus = value;
                Properties.Settings.Default.Save();
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
                Properties.Settings.Default.ViewOptions = value;
                Properties.Settings.Default.Save();
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
                Properties.Settings.Default.ViewChallenges = value;
                Properties.Settings.Default.Save();
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
                if (isInitialized && viewChallenges)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public TileVisibility()
        {

            viewStatus = Properties.Settings.Default.ViewStatus;
            viewOptions = Properties.Settings.Default.ViewOptions; ;
            viewChallenges = Properties.Settings.Default.ViewChallenges; ;
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
                if (isInitialized != value)
                {
                    isInitialized = value;
                    NotifyPropertyChanged("IsStatusVisible");
                    NotifyPropertyChanged("IsOptionsVisible");
                    NotifyPropertyChanged("IsChallengesVisible");
                }
            }
        }
    }
}
