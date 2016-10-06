using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfPoeChallengeTracker.viewmodel;

namespace WpfPoeChallengeTracker.view
{

    public partial class BuyMessageWindow : Window, INotifyPropertyChanged
    {
        private List<ChallengeView> missingChallenges;

        public List<ChallengeView> MissingChallenges
        {
            get { return missingChallenges; }
            set
            {
                if (value != null)
                {
                    missingChallenges = value;
                }
                else
                {
                    missingChallenges = new List<ChallengeView>();
                }
                NotifyPropertyChanged();
            }
        }

        public string HeaderLabel
        {
            get
            {
                return "Select which challenges to include in the message. You can then use this message in the global 820 or trade 820 chat channel.";
            }
        }

        private List<ChallengeView> includeInMsgViews;
        private List<CheckBox> includeChallengeCheckboxList;

        public BuyMessageWindow()
        {
            DataContext = this;
            InitializeComponent();
            includeInMsgViews = new List<ChallengeView>();
            includeChallengeCheckboxList = new List<CheckBox>();
            disableWtbMsgUpdating = false;
        }

        private void IncludeChallengeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            if (cb.DataContext is ChallengeView)
            {
                var view = (ChallengeView)cb.DataContext;
                if (!includeInMsgViews.Contains(view))
                {
                    includeInMsgViews.Add(view);
                }
                updateWtbMessage();
            }
        }

        private void IncludeChallengeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            if (cb.DataContext is ChallengeView)
            {
                var view = (ChallengeView)cb.DataContext;
                if (includeInMsgViews.Contains(view))
                {
                    includeInMsgViews.Remove(view);
                }
                updateWtbMessage();
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string wtbMessage = "LF Challenge Completion:  ";

        public string WtbMessage
        {
            get { return wtbMessage; }
            set
            {
                if (wtbMessage != value)
                {
                    wtbMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //this prevents updating the message for each single checkbox upon clicking on Select All Button
        private bool disableWtbMsgUpdating;

        private void updateWtbMessage()
        {
            if (!disableWtbMsgUpdating)
            {
                var tempMsg = "LF Challenge Completion:  ";

                foreach (var view in includeInMsgViews)
                {
                    tempMsg += view.ChallengeName;
                    if (view.Data.Type == model.ChallengeType.Progressable)
                    {
                        tempMsg += ": ";
                        foreach (var subView in view.SubChallenges.Where(n => !n.IsDone))
                        {
                            tempMsg += subView.Description + ", ";
                        }
                        tempMsg = tempMsg.Substring(0, tempMsg.Length - 2);
                    }
                    tempMsg += "; ";
                }
                tempMsg = tempMsg.Substring(0, tempMsg.Length - 2);
                WtbMessage = tempMsg;
            }
        }

        private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(WtbMessage);
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            disableWtbMsgUpdating = true;
            foreach (var item in includeChallengeCheckboxList)
            {
                item.IsChecked = true;
            }
            disableWtbMsgUpdating = false;
            updateWtbMessage();
        }

        private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            disableWtbMsgUpdating = true;
            foreach (var item in includeChallengeCheckboxList)
            {
                item.IsChecked = false;
            }
            disableWtbMsgUpdating = false;
            updateWtbMessage();
        }

        private void IncludeChallengeCheckbox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                var cb = (CheckBox)sender;
                includeChallengeCheckboxList.Add(cb);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
