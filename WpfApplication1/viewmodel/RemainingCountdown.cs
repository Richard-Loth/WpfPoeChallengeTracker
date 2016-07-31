using WpfPoeChallengeTracker.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfPoeChallengeTracker.viewmodel
{
    public class RemainingCountdown : INotifyPropertyChanged
    {
        private TimeSpan calculateRemainingTime(DateTime leagueEnd)
        {
            var now = DateTime.UtcNow;
            var span = leagueEnd.Subtract(now);
            return span;
        }

        public RemainingCountdown(Model model)
        {
            this.model = model;
            remainingTime = calculateRemainingTime(model.LeagueInfo.LeagueEndsOn);
            NotifyPropertyChanged("RemainingSeconds");
            NotifyPropertyChanged("RemainingMinutes");
            NotifyPropertyChanged("RemainingHours");
            NotifyPropertyChanged("RemainingDays");
            //});
            remainingTimer = new Timer(remainingTimerCallBack, null, 1000, 1000);
        }

        private Timer remainingTimer;
        Model model;

        private TimeSpan calculateApproxRemainingTime()
        {
            //90 days as standard length
            var approxLeagueLength = new TimeSpan(90, 0, 0, 0);
            var now = DateTime.UtcNow;
            var start = model.LeagueInfo.LeagueStartedOn;
            var end = start.Add(approxLeagueLength);
            return end.Subtract(now);
        }

        public void Dispose()
        {
            remainingTimer.Dispose();
        }

        public string RemainingDays
        {
            get
            {
                return remainingTime?.Days.ToString("00");
            }
        }

        public string RemainingHours
        {
            get
            {
                return remainingTime?.Hours.ToString("00");
            }
        }

        public string RemainingMinutes
        {
            get
            {
                return remainingTime?.Minutes.ToString("00");
            }
        }

        public string RemainingSeconds
        {
            get
            {
                return remainingTime?.Seconds.ToString("00");
            }
        }

        private TimeSpan? remainingTime;

        public event PropertyChangedEventHandler PropertyChanged;

        private void remainingTimerCallBack(object sender)
        {
            var oldMinutes = remainingTime?.Minutes;
            var oldHours = remainingTime?.Hours;
            var oldDays = remainingTime?.Days;
            var remain = calculateRemainingTime(model.LeagueInfo.LeagueEndsOn);
            remainingTime = remain;
            NotifyPropertyChanged("RemainingSeconds");
            if (remain.Minutes != oldMinutes)
            {
                NotifyPropertyChanged("RemainingMinutes");
            }
            if (remain.Hours != oldHours)
            {
                NotifyPropertyChanged("RemainingHours");
            }
            if (remain.Days != oldDays)
            {
                NotifyPropertyChanged("RemainingDays");
            }
        }

        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
