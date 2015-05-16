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
        void Subscribe<T>(Action<T> action) where T : IRequest;
        /// <summary>
        /// Unsubscribes an action to publishing request of type T or derived types.
        /// </summary>
        /// <typeparam name="T">The type of the request.</typeparam>
        /// <param name="action">The action to unsubscribe.</param>
        void Unsubscribe<T>(Action<T> action) where T : IRequest;
        /// <summary>
        /// Publish a request of type T.
        /// </summary>
        /// <typeparam name="T">The type of the request.</typeparam>
        /// <param name="request">The request to publish.</param>
        void Publish<T>(T request) where T : IRequest;
    }
}
