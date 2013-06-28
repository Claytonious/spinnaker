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
	/// This is a trivially simple view model that exposes a string property and an integer property.
	/// It is used simply to show round-tripping values from the GUI to managed code and back to the GUI.
	/// </summary>
    public class SimpleFormViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int intProp = 12;
        private string stringProp = "Hello";
		private City city = City.Houston;

		public enum City
		{
			Houston,
			Phoenix,
			SanDiego,
			Seattle,
			SanFrancisco,
			Llano
		}

		private Dictionary<string,City> cityDisplayChoicesByText;
		private Dictionary<City,string> cityDisplayChoicesByValue;

        public SimpleFormViewModel ()
		{
			// Don't be fancy and use Enum.GetNames() to automatically populate this,
			// because in real applications choices displayed in the UI often need to be localized or otherwise
			// not exactly match the enum names. So we build a simple array of what to display here. In a real app,
			// these might be fetched from a resource file, etc.
			cityDisplayChoicesByText = new Dictionary<string, City> ();
			cityDisplayChoicesByText ["Houston, a' Bubblin' Crude"] = City.Houston;
			cityDisplayChoicesByText ["Phoenix, Life Grows where the Water Flows"] = City.Phoenix;
			cityDisplayChoicesByText ["San Diego, Beautiful but You Can't Afford It"] = City.SanDiego;
			cityDisplayChoicesByText ["Seattle, Cold, Gray, and Gorgeous"] = City.Seattle;
			cityDisplayChoicesByText ["San Francisco, You're not Liberal Enough"] = City.SanFrancisco;
			cityDisplayChoicesByText ["Llano, Home of the Free"] = City.Llano;
			cityDisplayChoicesByValue = new Dictionary<City, string> ();
			foreach (KeyValuePair<string,City> pair in cityDisplayChoicesByText) 
				cityDisplayChoicesByValue[pair.Value] = pair.Key;
        }
		
		public string StringProperty
		{
			get { return stringProp; }
			set
			{
				stringProp = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("StringProperty"));
					PropertyChanged(this, new PropertyChangedEventArgs("CurrentValues"));
				}
			}
		}
		
		public int IntProperty
		{
			get { return intProp; }
			set
			{
				intProp = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("IntProperty"));
					PropertyChanged(this, new PropertyChangedEventArgs("CurrentValues"));
				}
			}
		}
		
		public string CurrentValues
		{
			get { return "String: " + StringProperty + " Int: " + IntProperty + " City: " + city; }
		}

		public City SelectedCity 
		{
			get { return city; }
			set 
			{
				city = value;
				if (PropertyChanged != null) 
				{
					PropertyChanged (this, new PropertyChangedEventArgs ("SelectedCity"));
					PropertyChanged (this, new PropertyChangedEventArgs ("SelectedCityText"));
					PropertyChanged(this, new PropertyChangedEventArgs("CurrentValues"));
				}
			}
		}

		public string SelectedCityText
		{
			get { return cityDisplayChoicesByValue [city]; }
			set 
			{
				if (String.IsNullOrEmpty(value))
					throw new ArgumentException("City is required", "SelectedCityText");
				if (!cityDisplayChoicesByText.TryGetValue(value, out city))
					throw new ArgumentException("Must be a valid city", "SelectedCityText");
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("SelectedCityText"));
					PropertyChanged(this, new PropertyChangedEventArgs("CurrentValues"));
				}
			}
		}

		public IEnumerable<string> CityChoices 
		{
			get 
			{ 
				foreach(string choice in cityDisplayChoicesByText.Keys)
					yield return choice;
			}
		}
    }
}
