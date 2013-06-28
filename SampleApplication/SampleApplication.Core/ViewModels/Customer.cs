using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spinnaker.Sample.WinForms.ViewModels
{
	/// <summary>
	/// Represents a customer in the sample application. Since we intend to use each Customer in the view,
	/// we implement INotifyPropertyChanged so that our view can bind to Customer's properties. Even though we
	/// have lots of customers in our application, each of them is its own view model.
	/// 
	/// In a real application, this Customer view model might be backed by a persistent business object that
	/// is loaded and saved with a database or fetched and stored from web services. Or you might implement
	/// INotifyPropertyChanged on that real business object itself without an intermediate "view model" 
	/// representation of it.
	/// </summary>
    public class Customer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string firstName, lastName, email, address, profilePicture;
        private decimal balance;
        private bool isDirty;

        public Customer()
        {
        }

		/// <summary>
		/// We track whether or not a Customer is "dirty" (indicating whether or not it has been changed since it was
		/// loaded) so that we can offer the user a "Save" command when the Customer is dirty. This is not something
		/// that is required by Spinnaker, but is an example of how this common use case might be implemented.
		/// </summary>
        public bool IsDirty
        {
            get { return isDirty; }
        }

        public void ClearDirty()
        {
            isDirty = false;
        }

		/// <summary>
		/// We wrap raising property changes in a method so that we can always make this Customer dirty when
		/// one of its properties changes.
		/// </summary>
        private void RaisePropertyChanged(string propName)
        {
            isDirty = true;
            if (PropertyChanged != null)
            {
				// We can raise PropertyChanged events on as many properties as we like for any reason
				// at any point in the code. The GUI will be notified. It's safe to do this from any thread.
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
                PropertyChanged(this, new PropertyChangedEventArgs("IsDirty"));
            }
        }

		/// <summary>
		/// Something like a primary key for a database.
		/// </summary>
        public int CustomerId { get; set; }

		/// <summary>
		/// This would save the changes made to the Customer to a database or web service. In this sample,
		/// we don't really do any persistence, so we simply clear the IsDirty flag to pretend that the
		/// Customer has been saved.
		/// </summary>
        public void Save()
        {
            isDirty = false;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public string FirstName
        {
            get { return firstName; }
            set
            {
                if (value != firstName)
                {
                    if (String.IsNullOrEmpty(value))
                        throw new ArgumentException("Cannot be blank", "FirstName");
                    firstName = value;
                    RaisePropertyChanged("FirstName");
                }
            }
        }

        public string LastName
        {
            get { return lastName; }
            set
            {
                if (value != lastName)
                {
                    if (String.IsNullOrEmpty(value))
                        throw new ArgumentException("Cannot be blank", "LastName");
                    lastName = value;
                    RaisePropertyChanged("LastName");
                }
            }
        }

        public string Email
        {
            get { return email; }
            set
            {
                if (value != email)
                {
                    if (String.IsNullOrEmpty(value))
                        throw new ArgumentException("Cannot be blank", "Email");
                    else if (value.Length < 3 || !value.Contains("@"))
                        throw new ArgumentException("Must be an email address", "Email");
                    email = value;
                    RaisePropertyChanged("Email");
                }
            }
        }

        public string Address
        {
            get { return address; }
            set
            {
                if (value != address)
                {
                    if (String.IsNullOrEmpty(value))
                        throw new ArgumentException("Cannot be blank", "Address");
                    address = value;
                    RaisePropertyChanged("Address");
                }
            }
        }

		/// <summary>
		/// This is one way to accomplish custom marshaling of user-entered values into specific types.
		/// </summary>
        public string BalanceDisplay
        {
            get { return String.Format("{0:C}", balance); }
            set
            {
                if (Decimal.TryParse(value, out balance) && PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("BalanceDisplay"));
                else 
                    throw new ArgumentException("Must be a monetary figure", "BalanceDisplay");
            }
        }

		/// <summary>
		/// Balance isn't directly bound in the view. Instead, BalanceDisplay is bound and parsed to populate this property.
		/// </summary>
        public decimal Balance
        {
            get { return balance; }
            set
            {
                balance = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Balance"));
                    PropertyChanged(this, new PropertyChangedEventArgs("BalanceDisplay"));
                }
            }
        }

        public string ProfilePicture
        {
            get { return profilePicture; }
            set
            {
                profilePicture = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ProfilePicture"));
            }
        }
    }
}
