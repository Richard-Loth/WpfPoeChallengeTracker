using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfPoeChallengeTracker.model;

namespace WpfPoeChallengeTracker.view
{
    /// <summary>
    /// Interaction logic for EnterAccountName.xaml
    /// </summary>
    public partial class EnterAccountName : Window
    {

        private string accountName;

        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }



        public EnterAccountName()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            accountName = AccountNameTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void WindowContentRendered(object sender, EventArgs e)
        {
            AccountNameTextBox.Text = Properties.Settings.Default.AccountName;
            AccountNameTextBox.Focus();
            AccountNameTextBox.SelectAll();
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            var accName = AccountNameTextBox.Text.Trim();
            statusTextBlock.Visibility = Visibility.Visible;
            var status = LoginStatus.NoAccountName;
            if (accName.Length > 0)
            {
                try
                {
                    status = AccountCheck.checkAccountName(accName);
                }
                catch (System.Net.WebException)
                {
                    statusTextBlock.Foreground = ColorConstants.ErrorTextColor;
                    statusTextBlock.Text = "There is a problem with your internet connection";
                    return;
                }
            }
            switch (status)
            {
                case LoginStatus.NoAccountName:
                    statusTextBlock.Foreground = ColorConstants.ErrorTextColor;
                    statusTextBlock.Text = "Please enter your account name";
                    break;
                case LoginStatus.InvalidName:
                    statusTextBlock.Foreground = ColorConstants.ErrorTextColor;
                    statusTextBlock.Text = "The specified accountname doesn't exist";
                    break;
                case LoginStatus.ValidNamePrivateProfile:
                    statusTextBlock.Foreground = ColorConstants.ErrorTextColor;
                    statusTextBlock.Text = "Your profile is set to private";
                    break;
                case LoginStatus.ValidNamePrivateChallenges:
                    statusTextBlock.Foreground = ColorConstants.ErrorTextColor;
                    statusTextBlock.Text = "Your challenges tab is set to private";
                    break;
                case LoginStatus.ValidName:
                    statusTextBlock.Foreground = ColorConstants.SucessTextColor;
                    statusTextBlock.Text = "Your account name is valid";
                    OkButton.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private void AccountNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OkButton.IsEnabled = false;
            statusTextBlock.Visibility = Visibility.Hidden;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
