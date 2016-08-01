using WpfPoeChallengeTracker.model;
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

namespace WpfPoeChallengeTracker
{
    
    public partial class App : Application

    {
        private Timer appInitTimer;
        private Model model;
        private Viewmodel viewmodel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            model = new Model();
            viewmodel = new Viewmodel(model);
            appInitTimer = new Timer(appInitTimerCallback, null, 100, Timeout.Infinite);
            var window = new MainWindow(viewmodel);
            window.Title = "Poe Challenge Tracker";
            var uri = new Uri("pack://application:,,,/resources/logo.png");
            var bitmap = BitmapFrame.Create(uri);
            window.Icon = bitmap;
            window.Show();

        }
        private async void appInitTimerCallback(object state)
        {
            var uri = new Uri("pack://application:,,,/challengedata/ChallengeData.prophecy.xml");
            model.initModel(uri);
            viewmodel.initViewmodel(uri);
            appInitTimer.Dispose();
        }



        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }
    }


}



