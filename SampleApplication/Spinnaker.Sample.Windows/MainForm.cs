using Spinnaker.Bridge.WinForms;
using Spinnaker.Sample.WinForms.ViewModels;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Spinnaker.Core;
using Spinnaker.Log4net;

namespace Spinnaker.Sample.Windows
{
    public partial class MainForm : Form
    {
        private SplashViewModel splashViewModel;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs loadArgs)
        {
			base.OnLoad(loadArgs);

            WinFormsSpinnakerConfiguration.Init();

            // Using log4net with Spinnaker is optional.
            log4net.Config.XmlConfigurator.Configure();
            SpinnakerLog4netAdapter.Init();

            BrowserBridge bridge = SpinnakerConfiguration.CurrentConfig.CreateBrowserBridge(webBrowser);
            splashViewModel = new SampleApplication.Core.SampleApplication().Init();
            bridge.ShowView("SplashView.html", splashViewModel);
            guiTimer.Start();
			splashViewModel.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => 
			{
				if (e.PropertyName == "CurrentPage")
					bridge.ExecuteScriptFunction("setHeroBackground");
			};
		}

        private void HandleGUITimerTick(object sender, EventArgs e)
        {
            splashViewModel.RealtimeViewModel.HandleGUITimerTick();
        }
    }
}
