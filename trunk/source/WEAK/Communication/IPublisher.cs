using System;

namespace WEAK.Communication
{
    public interface IPublisher : IDisposable
    {
        void Subscribe<T>(Action<T> action) where T : IRequest;
        void Unsubscribe<T>(Action<T> action) where T : IRequest;
        void Publish<T>(T arg) where T : IRequest;
    }
}
