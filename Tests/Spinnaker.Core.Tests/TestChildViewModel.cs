using System;
using System.ComponentModel;

namespace Spinnaker.Core.Tests
{
	public class TestChildViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private int intValue;
		private string stringValue;

		public TestChildViewModel()
		{
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

