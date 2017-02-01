#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WEAK.Windows.Input
{
    public sealed class AsyncDelegateCommand : ICommand
    {
        #region Fields

        private readonly Func<Task> _executeAsync;
        private readonly Func<bool> _canExecute;

        private volatile bool _isExecuting;

        #endregion

        #region Initialisation

        public AsyncDelegateCommand(Func<Task> executeAsync, Func<bool> canExecute)
        {
            if (executeAsync == null)
            {
                throw new ArgumentNullException(nameof(executeAsync));
            }

            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public AsyncDelegateCommand(Func<Task> executeAsync)
            : this(executeAsync, null)
        { }

        #endregion

        #region Methods

        public bool CanExecute()
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async Task ExecuteAsync()
        {
            _isExecuting = true;

            await _executeAsync();

            _isExecuting = false;
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
            ExecuteAsync();
        }

        #endregion
    }

    public sealed class AsyncDelegateCommand<T> : ICommand
    {
        #region Fields

        private readonly Func<T, Task> _executeAsync;
        private readonly Predicate<T> _canExecute;
        private readonly bool _isClass;

        private volatile bool _isExecuting;

        #endregion

        #region Initialisation

        public AsyncDelegateCommand(Func<T, Task> executeAsync, Predicate<T> canExecute)
        {
            if (executeAsync == null)
            {
                throw new ArgumentNullException(nameof(executeAsync));
            }

            _executeAsync = executeAsync;
            _canExecute = canExecute;
            _isClass = typeof(T).IsClass;
        }

        public AsyncDelegateCommand(Func<T, Task> executeAsync)
            : this(executeAsync, null)
        { }

        #endregion

        #region Methods

        public bool CanExecute(T parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
        }

        public async Task ExecuteAsync(T parameter)
        {
            _isExecuting = true;

            await _executeAsync(parameter);

            _isExecuting = false;
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
            ExecuteAsync((T)parameter);
        }

        #endregion
    }
}
