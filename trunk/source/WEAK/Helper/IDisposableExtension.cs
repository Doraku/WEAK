using System;
using System.Collections.Generic;
using System.Linq;

namespace WEAK.Helper
{
    /// <summary>
    /// Provides methods to merge multiple IDisposable into a single IDisposable.
    /// Note that Dispose method need to be called explicitly, it will not be called by finaliser.
    /// </summary>
    public static class IDisposableExtension
    {
        #region Types

        private class DisposableGroup : IDisposable
        {
            #region Fields

            private readonly List<IDisposable> _disposables;

            private bool _isDisposed;

            #endregion

            #region Initialisation

            public DisposableGroup(IEnumerable<IDisposable> disposables)
            {
                _isDisposed = false;

                _disposables = new List<IDisposable>();

                foreach (IDisposable disposable in disposables)
                {
                    if (disposable is DisposableGroup)
                    {
                        InsertChild(disposable as DisposableGroup);
                    }
                    else
                    {
                        _disposables.Add(disposable);
                    }
                }
            }

            ~DisposableGroup()
            {
                Dispose();
            }

            #endregion

            #region Methods

            private void InsertChild(DisposableGroup multiDisposable)
            {
                foreach (IDisposable disposable in multiDisposable._disposables)
                {
                    if (disposable is DisposableGroup)
                    {
                        InsertChild(disposable as DisposableGroup);
                    }
                    else
                    {
                        _disposables.Add(disposable);
                    }
                }
            }

            #endregion

            #region IDisposable

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    List<Exception> errors = new List<Exception>();

                    for (int i = _disposables.Count - 1; i >= 0; --i)
                    {
                        try
                        {
                            _disposables[i].Dispose();
                        }
                        catch (Exception damn)
                        {
                            errors.Add(damn);
                        }
                    }

                    if (errors.Count > 0)
                    {
                        throw new AggregateException("An error occured while disposing one or more elements.", errors);
                    }

                    _isDisposed = true;
                    GC.SuppressFinalize(this);
                }
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Merges IDisposable instances into a single IDisposable.
        /// Note that Dispose method need to be called explicitly, it will not be called by finaliser.
        /// Dispose method of instances will be called in reverse order of the argument.
        /// </summary>
        /// <param name="disposables">The instances to merge.</param>
        /// <returns>A single IDisposable.</returns>
        public static IDisposable Merge(this IEnumerable<IDisposable> disposables)
        {
            if (disposables == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => disposables));
            }
            
            return new DisposableGroup(disposables);
        }

        /// <summary>
        /// Merges IDisposable instances into a single IDisposable.
        /// Note that Dispose method need to be called explicitly, it will not be called by finaliser.
        /// Dispose method of instances will be called in reverse order of the arguments.
        /// </summary>
        /// <param name="disposable">An instance to merge.</param>
        /// <param name="disposables">An array of instances to merge.</param>
        /// <returns>A single IDisposable.</returns>
        public static IDisposable Merge(this IDisposable disposable, params IDisposable[] disposables)
        {
            if (disposable == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => disposable));
            }
            if (disposables == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => disposables));
            }

            return Merge(new[] { disposable }.Concat(disposables));
        }

        #endregion
    }
}
