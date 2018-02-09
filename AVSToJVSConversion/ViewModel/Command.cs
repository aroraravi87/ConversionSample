using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AVSToJVSConversion.ViewModel
{
    public class CustomCommand : ICommand
    {
        #region === [ICommand Members ] =========================================

        Action<object> _executeMethod;

        //Func<object, bool> _canExecuteMethod;

        public CustomCommand(Action<object> executeMethod)
        {
            _executeMethod = executeMethod;
            //_canExecuteMethod = canExecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _executeMethod(parameter);
        }

        #endregion
    }

    
}
