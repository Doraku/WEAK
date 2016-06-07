using System;
using System.Collections.Generic;
using System.Linq;

namespace WEAK.Helper
{
    /// <summary>
    /// Provides extension methods on IDisposable.
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
                _disposables = new List<IDisposable>();

                _isDisposed = false;

                InsertChild(disposables);
            }

            #endregion

            #region Methods

            private void InsertChild(IEnumerable<IDisposable> disposables)
            {
                foreach (IDisposable disposable in disposables)
                {
                    if (disposable is DisposableGroup)
                    {
                        InsertChild((disposable as DisposableGroup)._disposables);
                    }
                    else
                    {
                        _disposables.Add(disposable);
                    }
                }
            }

            #endregion

            #region IDisposable

            void IDisposable.Dispose()
            {
                if (!_isDisposed)
                {
                    for (int i = _disposables.Count - 1; i >= 0; --i)
                    {
                        _disposables[i]?.Dispose();
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
        /// <exception cref="ArgumentNullException">disposables is null.</exception>
        public static IDisposable Merge(this IEnumerable<IDisposable> disposables)
        {
            disposables.CheckForArgumentNullException(nameof(disposables));

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
        /// <exception cref="ArgumentNullException">disposables is null.</exception>
        public static IDisposable Merge(this IDisposable disposable, params IDisposable[] disposables)
        {
            disposables.CheckForArgumentNullException(nameof(disposables));

            return Merge(new[] { disposable }.Concat(disposables));
        }

        #endregion
    }
}
