using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WpfPoeChallengeTracker.model.persistence;

namespace WpfPoeChallengeTracker.viewmodel
{
   public class LeagueView : INotifyPropertyChanged
    {
        private League league;

        public League LeagueData
        {
            get { return league; }
            set { league = value; }
        }

        private bool isChecked;
        private Viewmodel viewmodel;

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public void setIsCheckedPlain(bool isChecked)
        {
            if (this.isChecked != isChecked)
            {
                this.isChecked = isChecked;
                NotifyPropertyChanged("IsChecked");
            }
        }

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (IsChecked == value)
                {
                    return;
                }
                isChecked = value;
                viewmodel.leagueViewClicked(this);
            }
        }

        public string Name
        {
            get
            {
                return league.InAppName;
            }
        }

        public LeagueView(Viewmodel viewmodel)
        {
            isChecked = false;
            this.viewmodel = viewmodel;
        }

    }
}
