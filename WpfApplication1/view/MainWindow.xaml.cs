using Poe_Challenge_Tracker.viewmodel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace WpfPoeChallengeTracker
{



    internal class FilterStringHolder
    {
        private string filter;

        public string Filter
        {
            get { return filter; }
            set { filter = value; }
        }


        public FilterStringHolder()
        {
            filter = "";
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow(Viewmodel viewmodel)
        {
            this.viewmodel = viewmodel;
            filterStringHolder = new FilterStringHolder();
            filterTimer = new Timer(filterTimerCallback, filterStringHolder, Timeout.Infinite, Timeout.Infinite);
            this.InitializeComponent();
            DataContext = viewmodel;
        }

        private Timer filterTimer;
        private FilterStringHolder filterStringHolder;
        private Viewmodel viewmodel;

        public Viewmodel ViewModel
        {
            get { return viewmodel; }
        }

        private void collapseBtnClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            ChallengeView c = (ChallengeView)btn.DataContext;
            c.IsCollapsed = !c.IsCollapsed;
        }



        private void ChallengeIsDoneClick(object sender, RoutedEventArgs e)
        {
            var subView = (ChallengeView)((Button)sender).DataContext;
            viewmodel.challengeIsDoneClick(subView);
        }


        private void filterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            filterTimer.Change(500, -1);
            filterStringHolder.Filter = filterTextBox.Text;
        }


        private void filterTimerCallback(object state)
        {
            viewmodel.applyNewFilterText(((FilterStringHolder)state).Filter);
        }

        private async void resetButton_Click(object sender, RoutedEventArgs e)
        {
            while (!viewmodel.IsInitialized)
            {
                await Task.Delay(100);
            }
            //TODO important
            //var msg = "Do you really want to reset the progress and the ordering of the challenges of the current league?";
            //var dialog = new Windows.UI.Popups.MessageDialog(msg);

            //dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
            //dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });

            //dialog.DefaultCommandIndex = 1;
            //dialog.CancelCommandIndex = 1;

            //var result = await dialog.ShowAsync();
            //if ((int)result.Id == 0)
            //{
            //    await viewmodel.resetProgressAndOrder();
            //}
            
            var msg = "Do you really want to reset the progress and the ordering of the challenges of the current league?";

            string caption = "Reset Progress";
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(msg, caption, button, icon);
            switch (result)
            {
                case MessageBoxResult.OK:
                    viewmodel.resetProgressAndOrder();
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }

        private void changeLeagueButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void unCollapseAllButton_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.unCollapseAll();

        }

        private void collapseAllButton_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.collapseAll();
        }

        private void subChallengeItemClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void subChallengesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                object maybeSubView = e.AddedItems[0];
                if (maybeSubView is SubChallengeView)
                {
                    var subView = (SubChallengeView)e.AddedItems[0];
                    if (subView != null)
                    {
                        viewmodel.subChallengeDescriptionTapped(subView);
                    }
                }
                var listview = (ListView)sender;
                listview.SelectedItem = null;
            }
        }

        private void challengesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = (ListView)sender;
            listview.SelectedItem = null;
        }

        private void subChallengeInfoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = (ListView)sender;
            listview.SelectedItem = null;
        }

        //private void subChallengesListView_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    
        //}

        //private void subChallengesListItemBorder_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    Debug.WriteLine("subChallengesListView_ItemClick: " + e);
        //}
    }
}
