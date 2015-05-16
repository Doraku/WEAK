using System.Windows;
using WEAK.Communication;

namespace WEAK.Windows.Base
{
    public sealed class SetRegionContentRequest : IRequest
    {
        #region Fields

        private readonly string _regionName;
        private readonly UIElement _view;

        #endregion

        #region Properties

        public string RegionName
        {
            get { return _regionName; }
        }
        public UIElement View
        {
            get { return _view; }
        }

        #endregion

        #region Initialisation

        public SetRegionContentRequest(string regionName, UIElement view)
        {
            _regionName = regionName;
            _view = view;
        }

        #endregion

        #region IRequest

        public RequestPublishingMode PulishingMode
        {
            get { return RequestPublishingMode.Context; }
        }

        #endregion
    }

    public sealed class ApplicationExitRequest : IRequest
    {
        #region IRequest

        public RequestPublishingMode PulishingMode
        {
            get { return RequestPublishingMode.Context; }
        }

        #endregion
    }
}
