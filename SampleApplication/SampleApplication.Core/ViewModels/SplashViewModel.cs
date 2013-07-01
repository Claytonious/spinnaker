using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spinnaker.Sample.WinForms.ViewModels
{
	/// <summary>
	/// This is the root view model of the sample application. Notice that it contains several
	/// child view models of its own, all of which can be bound to in the GUI.
	/// 
	/// This view model primarily manages the navigation of the sample application, exposing the concept
	/// of a "current page" that the user is on and reacting to user requests to change the current page.
	/// This concept of "pages" within the sample application is not something inherent to Spinnaker - it
	/// is simply one optional way of binding things within a GUI.
	/// 
	/// Actual navigation within the GUI is accomplished by binding the visibility of sections of the GUI to
	/// this view model's "CurrentPage" property.
	/// </summary>
    public class SplashViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Page currentPage = Page.SplashPage;

        public enum Page
        {
            SplashPage,
            RealtimePage,
            DataPage,
            SimpleFormPage
        }

        public SplashViewModel()
        {
        }

        public void GotoSplashPage()
        {
            CurrentPage = Page.SplashPage;
        }

        public void GotoRealtimePage()
        {
            CurrentPage = Page.RealtimePage;
        }

        public void GotoDataPage()
        {
            CurrentPage = Page.DataPage;
        }

        public void GotoSimpleFormPage()
        {
            CurrentPage = Page.SimpleFormPage;
        }

        public CustomersViewModel CustomersViewModel { get; set; }
        public RealtimeViewModel RealtimeViewModel { get; set; }
        public SimpleFormViewModel SimpleFormViewModel { get; set; }

        public Page CurrentPage
        {
            get { return currentPage; }
            set
            {
                currentPage = value;
                RealtimeViewModel.IsActive = currentPage == Page.RealtimePage;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentPage"));
            }
        }
    }
}
