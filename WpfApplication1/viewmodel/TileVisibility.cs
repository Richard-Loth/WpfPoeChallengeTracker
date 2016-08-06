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

        public bool ShowMinutesSeconds
        {
            get { return Properties.Settings.Default.ShowMinutesSeconds; }
            set
            {
                Properties.Settings.Default.ShowMinutesSeconds = value;
                NotifyPropertyChanged();
            }
        }

        public bool ViewStatus
        {
            get { return Properties.Settings.Default.ViewStatus; }
            set
            {
                Properties.Settings.Default.ViewStatus = value;
                NotifyPropertyChanged("IsStatusVisible");
            }
        }

        public bool ViewOptions
        {
            get { return Properties.Settings.Default.ViewOptions; }
            set
            {
                Properties.Settings.Default.ViewOptions = value;
                NotifyPropertyChanged("IsOptionsVisible");
            }
        }

        private bool isInitialized;

        public bool ViewChallenges
        {
            get { return Properties.Settings.Default.ViewChallenges; }
            set
            {
                Properties.Settings.Default.ViewChallenges = value;
                NotifyPropertyChanged("IsChallengesVisible");
            }
        }

        public Visibility IsStatusVisible
        {
            get
            {
                if (isInitialized && Properties.Settings.Default.ViewStatus)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        public Visibility IsOptionsVisible
        {
            get
            {
                if (isInitialized && Properties.Settings.Default.ViewOptions)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }



        public Visibility IsChallengesVisible
        {
            get
            {
                if (isInitialized && Properties.Settings.Default.ViewChallenges)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public TileVisibility()
        {
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
                    foreach (var item in this.GetType().GetProperties())
                    {
                        NotifyPropertyChanged(item.Name);
                    }
                }
            }
        }
    }
}
