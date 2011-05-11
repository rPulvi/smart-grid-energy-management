using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace WPF_StartPeer.ViewModel
{
    class ViewModelBase : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public ViewModelBase()
        {
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //blocca il debug con un errore se il nome della  proprietà  passato non esiste.
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private void VerifyPropertyName(string propname)
        {
            if (TypeDescriptor.GetProperties(this)[propname] == null)
            {
                var msg = "Invalid property name: " + propname;
                Debug.Fail(msg);
            }
        }
    }
}
