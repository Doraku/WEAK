using System;

namespace WEAK.Communication
{
    /// <summary>
    /// Defines methods to subscribe, unsubscribe and publish request.
    /// </summary>
    public interface IPublisher : IDisposable
    {
        /// <summary>
        /// Subscribes an action to publishing request of type T or derived types.
        /// </summary>
        /// <typeparam name="T">The type of the request.</typeparam>
        /// <param name="action">The action to subscribe.</param>
        /// <param name="publishingMode">The mode of publication.</param>
        /// <returns>A System.IDisposable to dispose to remove the subscription.</returns>
        IDisposable Subscribe<T>(Action<T> action, PublishingMode publishingMode);
        /// <summary>
        /// Unsubscribes an action to publishing request of type T or derived types.
        /// </summary>
        /// <typeparam name="T">The type of the request.</typeparam>
        /// <param name="action">The action to unsubscribe.</param>
        void Unsubscribe<T>(Action<T> action);
        /// <summary>
        /// Publish an argument of type T.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="arg">The argument to publish.</param>
        void Publish<T>(T arg);
    }
}
