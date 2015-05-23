using System;
using System.ComponentModel;
using System.Windows.Controls;
using WEAK.Communication;

namespace WEAK.Windows.Base
{
    public class AViewDataTemplate<T> : ContentControl
    {
        #region Fields

        private IPublisher _publisher;
        private bool _initialized = false;

        #endregion

        #region Properties

        public IPublisher Publisher
        {
            get { return _publisher; }
            set
            {
                if (_publisher != value)
                {
                    _publisher = value;
                    Publisher.HookUp(this);
                }
            }
        }

        public T ViewModel
        {
            get { return DataContext is T ? (T)DataContext : default(T); }
        }

        #endregion

        #region Initalization

        public AViewDataTemplate()
        {
            DependencyPropertyDescriptor.FromProperty(DataContextProperty, GetType()).AddValueChanged(this, OnDataContextChanged);
        }

        ~AViewDataTemplate()
        {
            Publisher = null;
        }

        protected virtual void Initialize(T data)
        { }

        #endregion

        #region Callbacks

        private void OnDataContextChanged(object sender, EventArgs e)
        {
            if (!_initialized && DataContext is T)
            {
                Initialize((T)DataContext);
                _initialized = true;
            }
        }

        [AutoHookUp(ExecutionMode.Context)]
        protected virtual void OnShellClosed(ApplicationExitRequest arg)
        {
            Publisher = null;
        }

        #endregion
    }
}
