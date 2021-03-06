﻿using WpfPoeChallengeTracker.model;
using WpfPoeChallengeTracker.viewmodel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;

namespace WpfPoeChallengeTracker
{

    public partial class App : Application

    {
        private Timer appInitTimer;
        private Model model;
        private Viewmodel viewmodel;
        private MainWindow mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            model = new Model();
            viewmodel = new Viewmodel(model);
            viewmodel.PropertyChanged += Viewmodel_PropertyChanged;
            appInitTimer = new Timer(appInitTimerCallback, null, 0, Timeout.Infinite);
            mainWindow = new MainWindow(viewmodel);
            mainWindow.Title = "Poe Challenge Tracker";
            var uri = new Uri("pack://application:,,,/resources/logo.png");
            var bitmap = BitmapFrame.Create(uri);
            mainWindow.Icon = bitmap;
            mainWindow.Show();
            mainWindow.persistFirstStart();
        }



        private void appInitTimerCallback(object state)
        {
            model.initModel();
            viewmodel.initViewmodel();
            mainWindow.initDragAndDrop();
            appInitTimer.Dispose();
        }

        private void Viewmodel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentLeague")
            {
                appInitTimer = new Timer(appInitTimerCallback, null, 0, Timeout.Infinite);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }
    }


}



