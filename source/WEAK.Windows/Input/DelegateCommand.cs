using System;
using System.Windows.Input;

namespace WEAK.Windows.Input
{
    public sealed class DelegateCommand : ICommand
    {
        #region Fields

        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        #endregion

        #region Initialisation

        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        public DelegateCommand(Action execute)
            : this(execute, null)
        { }

        #endregion

        #region Methods

        public bool CanExecute()
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute()
        {
            _execute();
        }

        #endregion

        #region ICommand

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }

        #endregion
    }

    public sealed class DelegateCommand<T> : ICommand
    {
        #region Fields

        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;
        private readonly bool _isClass;

        #endregion

        #region Initialisation

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = execute;
            _canExecute = canExecute;
            _isClass = typeof(T).IsClass;
        }

        public DelegateCommand(Action<T> execute)
            : this(execute, null)
        { }

        #endregion

        #region Methods

        public bool CanExecute(T parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(T parameter)
        {
            _execute(parameter);
        }

        #endregion

        #region ICommand

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (!(parameter is T)
                || (parameter == null && !_isClass))
            {
                return false;
            }

            return CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }

        #endregion
    }
}
