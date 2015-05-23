using System.ComponentModel;
using System.Runtime.CompilerServices;
using WEAK.Communication;

namespace WEAK.Windows.Base
{
    public abstract class AViewModel : INotifyPropertyChanged
    {
        #region Private fields

        private IPublisher _publisher;

        #endregion

        #region Properties

        public IPublisher Publisher
        {
            get { return _publisher; }
            set
            {
                if (Publisher != value)
                {
                    _publisher = value;
                    Publisher.Subscribe(this);
                }
            }
        }

        #endregion

        #region Initialisation

        public AViewModel()
        { }

        ~AViewModel()
        {
            Publisher = null;
        }

        #endregion

        #region Callbacks

        [Subscribe(ExecutionMode.Context)]
        protected virtual void OnShellClosed(ApplicationExitRequest arg)
        {
            Publisher = null;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
