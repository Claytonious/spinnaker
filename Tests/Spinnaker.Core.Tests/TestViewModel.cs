using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Spinnaker.Core.Tests
{
	public class TestViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private int intValue;
		private string stringValue;
		private TestChildViewModel childViewModel;
		private List<TestChildViewModel> childViewModels = new List<TestChildViewModel>();

		public TestViewModel()
		{
		}

		public TestChildViewModel ChildViewModel 
		{
			get { return childViewModel; }
			set
			{
				childViewModel = value;
				if (PropertyChanged != null)
					PropertyChanged (this, new PropertyChangedEventArgs("ChildViewModel"));
			}
		}

		public List<TestChildViewModel> ChildViewModels 
		{
			get { return childViewModels; }
			set 
			{
				childViewModels = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("ChildViewModels"));
			}
		}

		public int IntProperty 
		{
			get { return intValue; }
			set 
			{
				intValue = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("IntProperty"));
			}
		}

		public string StringProperty 
		{
			get { return stringValue; }
			set 
			{
				stringValue = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs ("StringProperty"));
			}
		}
	}
}

