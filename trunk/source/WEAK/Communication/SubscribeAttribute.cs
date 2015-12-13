using System;

namespace WEAK.Communication
{
    /// <summary>
    /// Specifies that the method should be automatically subscribed or unsuscribed when its parent type or instance is called with the extension methods HookUp and UnHookUp of IPublisher.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SubscribeAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Gets the PublishingMode of the attribute.
        /// </summary>
        public ExecutionMode PublishingMode { get; }

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialise a new instance of SubscribeAttribute.
        /// </summary>
        /// <param name="publishingMode"></param>
        public SubscribeAttribute(ExecutionMode publishingMode)
        {
            PublishingMode = publishingMode;
        }

        #endregion
    }
}
