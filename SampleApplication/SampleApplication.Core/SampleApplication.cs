using Spinnaker.Sample.WinForms.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApplication.Core
{
    public class SampleApplication
    {
        public SampleApplication()
        {
        }

        public SplashViewModel Init()
        {
            SplashViewModel splashViewModel = new SplashViewModel();
            splashViewModel.CustomersViewModel = new CustomersViewModel();
            splashViewModel.CustomersViewModel.LoadCustomers();
            splashViewModel.RealtimeViewModel = new RealtimeViewModel();
            splashViewModel.SimpleFormViewModel = new SimpleFormViewModel();
            return splashViewModel;
        }
    }
}
