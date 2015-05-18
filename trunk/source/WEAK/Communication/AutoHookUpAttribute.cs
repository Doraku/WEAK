using System;

namespace WEAK.Communication
{
    /// <summary>
    /// Specifies that the method should be automatically subscribed or unsuscribed when its parent type or instance is called with the extension methods HookUp and UnHookUp of IPublisher.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AutoHookUpAttribute : Attribute
    {
        #region Fields

        private readonly PublishingMode _publishingMode;

        #endregion

        #region Properties

        public PublishingMode PublishingMode
        {
            get { return _publishingMode; }
        }

        #endregion

        #region Initialisation

        public AutoHookUpAttribute(PublishingMode publishingMode)
        {
            _publishingMode = publishingMode;
        }

        #endregion
    }
}
