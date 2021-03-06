﻿using WpfPoeChallengeTracker.viewmodel;
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
using WpfPoeChallengeTracker.view;
using WpfPoeChallengeTracker.model;
using WPF.JoshSmith.ServiceProviders.UI;

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

    public partial class MainWindow : Window
    {

        public MainWindow(Viewmodel viewmodel)
        {
            this.viewmodel = viewmodel;
            filterStringHolder = new FilterStringHolder();
            filterTimer = new Timer(filterTimerCallback, filterStringHolder, Timeout.Infinite, Timeout.Infinite);
            this.InitializeComponent();
            DataContext = viewmodel;
            viewmodel.PropertyChanged += Viewmodel_PropertyChanged;
            if (!Properties.Settings.Default.FirstStart)
            {
                this.Height = Properties.Settings.Default.WindowHeight;
                this.Width = Properties.Settings.Default.WindowWidth;
                this.Left = Properties.Settings.Default.WindowPositionLeft;
                this.Top = Properties.Settings.Default.WindowPositionTop;
            }
            switch (Properties.Settings.Default.CompletedChallenges)
            {
                case CompletedBehaviour.DO_NOTHING:
                    completedChallengesCombobox.SelectedIndex = 0;
                    break;
                case CompletedBehaviour.SORT_TO_END:
                    completedChallengesCombobox.SelectedIndex = 1;
                    break;
                case CompletedBehaviour.HIDE:
                    completedChallengesCombobox.SelectedIndex = 2;
                    break;
                default:
                    break;
            }
            viewmodel.changeCompletedBehaviour(Properties.Settings.Default.CompletedChallenges);
        }

        private void DragDropManager_ProcessDrop(object sender, ProcessDropEventArgs<ChallengeView> e)
        {
            e.ItemsSource.Move(e.OldIndex, e.NewIndex);
            challengesListview.SelectedItem = null;
        }

        internal void persistFirstStart()
        {
            if (Properties.Settings.Default.FirstStart)
            {
                Properties.Settings.Default.FirstStart = false;
                Properties.Settings.Default.WindowPositionTop = this.Top;
                Properties.Settings.Default.WindowPositionLeft = this.Left;
                Properties.Settings.Default.Save();
            }
        }

        private void Viewmodel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentLeague")
            {
                filterTextBox.Text = "";
            }
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
            challengesListview.UpdateLayout();
            e.Handled = false;
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
            viewmodel.changeFilterText(((FilterStringHolder)state).Filter);
        }

        private async void resetButton_Click(object sender, RoutedEventArgs e)
        {
            while (!viewmodel.IsInitialized)
            {
                await Task.Delay(100);
            }


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

        private void unCollapseAllButton_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.collapseAll();

        }

        private void collapseAllButton_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.unCollapseAll();
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

        private void subChallengeInfoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = (ListView)sender;
            listview.SelectedItem = null;
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.suspend();
            Application.Current.Shutdown();
        }

        private void completedChallengesCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sel = (string)((ComboBoxItem)e.AddedItems[0]).Content;
            CompletedBehaviour behave = CompletedBehaviour.DO_NOTHING;
            switch (sel)
            {
                case "Do nothing":
                    behave = CompletedBehaviour.DO_NOTHING;
                    break;
                case "Hide":
                    behave = CompletedBehaviour.HIDE;
                    break;
                case "Sort to end":
                    behave = CompletedBehaviour.SORT_TO_END;
                    break;
            }
            Properties.Settings.Default.CompletedChallenges = behave;
            viewmodel.changeCompletedBehaviour(behave);
        }


        #region Window Events
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var size = e.NewSize;
            Properties.Settings.Default.WindowWidth = size.Width;
            Properties.Settings.Default.WindowHeight = size.Height;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            viewmodel.suspend();
        }
        #endregion

        private void challengesListview_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //this is needed to scroll while the mouse is inside an inner grid
            var listview = (ListView)sender;
            var border = (Border)VisualTreeHelper.GetChild(listview, 0);
            var scrollviewer = (ScrollViewer)border.Child;
            scrollviewer.ScrollToVerticalOffset(scrollviewer.VerticalOffset - e.Delta);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void SyncProgressButton_Click(object sender, RoutedEventArgs e)
        {
            startSyncProgress();
        }

        private void SyncProgressMenuItem_Click(object sender, RoutedEventArgs e)
        {
            startSyncProgress();
        }

        private void startSyncProgress()
        {
            var syncNow = false;
            switch (viewmodel.CurrentLoginStatus)
            {
                case LoginStatus.UnChecked:

                case LoginStatus.NoAccountName:

                case LoginStatus.InvalidName:

                case LoginStatus.ValidNamePrivateProfile:

                case LoginStatus.ValidNamePrivateChallenges:

                case LoginStatus.NetworkError:
                    EnterAccountNameMenuItem_Click(null, null);
                    syncNow = (ViewModel.CurrentLoginStatus == LoginStatus.ValidName);
                    break;
                case LoginStatus.ValidName:
                    syncNow = true;
                    break;
                default:
                    break;
            }
            if (syncNow)
            {
                viewmodel.syncProgress();
            }
        }

        private void EnterAccountNameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new EnterAccountName();
            var result = window.ShowDialog();
            if (result.Value)
            {
                ViewModel.AccountName = window.AccountName;
                ViewModel.CurrentLoginStatus = LoginStatus.ValidName;
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {

            if (sender.GetType() == this.GetType())
            {
                Window thisWindow = (Window)sender;
                Properties.Settings.Default.WindowPositionLeft = thisWindow.Left;
                Properties.Settings.Default.WindowPositionTop = thisWindow.Top;
            }
        }

        private void OpenWTBMessageWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            openChatMessageCreateWindow();
        }

        private void CreateChatMessageButton_Click(object sender, RoutedEventArgs e)
        {
            openChatMessageCreateWindow();
        }

        private void openChatMessageCreateWindow()
        {
            var window = new BuyMessageWindow();
            window.MissingChallenges = viewmodel.ChallengeViews.Where(challengeView => !challengeView.IsDone).ToList();
            window.ShowDialog();
        }

        private void AboutMenuSubItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new AboutDialog();
            window.ShowDialog();
        }
      
        ListViewDragDropManager<ChallengeView> dragDropManager;
        internal void initDragAndDrop()
        {
            Dispatcher.Invoke(() =>
            {
                dragDropManager = new ListViewDragDropManager<ChallengeView>(challengesListview);
                dragDropManager.ShowDragAdorner = true;
                dragDropManager.ProcessDrop += DragDropManager_ProcessDrop;
            });
        }

        private void challengesListview_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListView listview = (ListView)sender;
            listview.SelectedItem = null;
        }
    }
}
