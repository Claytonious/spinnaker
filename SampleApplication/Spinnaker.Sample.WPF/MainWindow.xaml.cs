using Spinnaker.Bridge.WPF;
using Spinnaker.Core;
using Spinnaker.Log4net;
using Spinnaker.Sample.WinForms.ViewModels;
using System;
using System.Collections.Generic;
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

namespace Spinnaker.Sample.WPF
{
    public partial class MainWindow : Window
    {
        private BrowserBridge bridge;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += HandleLoaded;            
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            if (bridge == null)
            {
                WPFSpinnakerConfiguration.Init();

                // Using log4net with Spinnaker is optional.
                log4net.Config.XmlConfigurator.Configure();
                SpinnakerLog4netAdapter.Init();

                bridge = SpinnakerConfiguration.CurrentConfig.CreateBrowserBridge(webBrowser);
                SplashViewModel splashViewModel = new SampleApplication.Core.SampleApplication().Init();
                bridge.ShowView("SplashView.html", splashViewModel);
                splashViewModel.PropertyChanged += (propSender,propChangeEvent) =>
                {
                    if (propChangeEvent.PropertyName == "CurrentPage")
                        bridge.ExecuteScriptFunction("setHeroBackground");
                };

                System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler((o, tickArgs) =>
                {
                    splashViewModel.RealtimeViewModel.HandleGUITimerTick();
                });
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
                dispatcherTimer.Start();
            }
        }
    }
}
