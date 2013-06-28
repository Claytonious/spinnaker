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
	/// This view model demonstrates changing values in the GUI continuously. For performance reasons,
	/// it uses a GUI timer (Timer on Windows and NSTimer on Mac) rather than a background thread, but
	/// threads are also safe to use in Spinnaker.
	/// </summary>
    public class RealtimeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool isActive;
        private double boxTop = 400.0;
        private double boxLeft = 500.0;
        private double animationTime;
		private bool isBoxVisible;

        public RealtimeViewModel()
        {
		}

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                if (isActive)
                    animationTime = Environment.TickCount;
            }
        }

		public bool IsBoxVisible 
		{
			get { return isBoxVisible; }
			set
			{
				if (isBoxVisible != value)
				{
					isBoxVisible = value;
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs("IsBoxVisible"));
				}
			}
		}

        public void HandleGUITimerTick()
        {
            if (isActive)
            {
                animationTime = (double)(Environment.TickCount - animationTime) / 1000.0;
                boxLeft = 400.0 + 100.0 * Math.Cos(animationTime);
                boxTop = 400.0 + 100.0 * Math.Sin(animationTime);
				IsBoxVisible = true;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentTime"));
                    PropertyChanged(this, new PropertyChangedEventArgs("BoxLeft"));
                    PropertyChanged(this, new PropertyChangedEventArgs("BoxTop"));
                }
            }
        }

        public string BoxTop
        {
            get
            {
                return (int)boxTop + "px";
            }
        }

        public string BoxLeft
        {
            get
            {
                return (int)boxLeft + "px";
            }
        }

        public string CurrentTime
        {
            get { return DateTime.Now.ToLongTimeString(); }
        }
    }
}
