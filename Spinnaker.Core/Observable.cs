using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Spinnaker.Core
{
    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Observable()
        {
        }

        protected virtual bool SetProperty<T>(string propertyName, ref T propertyField, T newValue)
        {
            bool changed = false;
            if (!Equals(newValue, propertyField))
            {
                changed = true;
                propertyField = newValue;
                RaisePropertyChanged(propertyName);
            }
            return changed;
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
