﻿using System;

namespace WEAK.Communication
{
    /// <summary>
    /// Specifies that the method should be automatically subscribed or unsuscribed when its parent type or instance is called with the extension methods HookUp and UnHookUp of IPublisher.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AutoHookUpAttribute : Attribute
    {
        #region Fields

        private readonly ExecutionMode _publishingMode;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the PublishingMode of the attribute.
        /// </summary>
        public ExecutionMode PublishingMode
        {
            get { return _publishingMode; }
        }

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialise a new instance of AutoHookUpAttribute.
        /// </summary>
        /// <param name="publishingMode"></param>
        public AutoHookUpAttribute(ExecutionMode publishingMode)
        {
            _publishingMode = publishingMode;
        }

        #endregion
    }
}
