using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace WPF_Resolver.Command
{
    public class DelegateCommand : ICommand
    {
        #region Delegates
        // delegato nel quale è scritto il codice che si deve eseguire quando si richiama il metodo Execute di ICommand. 
        private readonly Action<object> executeMethod = null;

        // delegato utilizzato da WPF per sapere quando un comando può essere eseguito
        private readonly Func<object, bool> canExecuteMethod = null;
        #endregion

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecuteMethod == null) return true;
            return this.canExecuteMethod(parameter);
        }

        public void Execute(object parameter)
        {
            if (executeMethod == null) return;
            this.executeMethod(parameter);
        }

    }
}