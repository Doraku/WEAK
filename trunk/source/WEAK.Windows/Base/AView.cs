using System.Windows.Controls;
using WEAK.Communication;

namespace WEAK.Windows.Base
{
    public class AView : ContentControl
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
                if (_publisher != value)
                {
                    _publisher = value;
                    Publisher.Subscribe(this);
                }
            }
        }

        #endregion

        #region Initalization

        public AView()
        { }

        ~AView()
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
    }
}
