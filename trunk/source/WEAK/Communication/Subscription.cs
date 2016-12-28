using System;
using System.Threading;

namespace WEAK.Communication
{
    public sealed partial class Publisher
    {
        private sealed class Subscription : IDisposable
        {
            #region Fields

            private readonly int _publisherId;
            private readonly Action<object> _action;

            private int _isDisposed;

            #endregion

            #region Initialisation

            public Subscription(int publisherId, Action<object> action)
            {
                _publisherId = publisherId;
                _action = action;

                _isDisposed = 0;
            }

            #endregion

            #region IDisposable

            void IDisposable.Dispose()
            {
                if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
                {
                    InnerPublisher<object>.Unsubscribe(_publisherId, _action);
                    
                    GC.SuppressFinalize(this);
                }
            }

            #endregion
        }
    }
}
