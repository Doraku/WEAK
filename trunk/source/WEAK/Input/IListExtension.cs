using System.Collections.Generic;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides a method to wrap an IList to an UnDo list linked to an UnDoManager to automatically generate IUnDo operations.
    /// </summary>
    public static class IListExtension
    {
        #region Types

        private class UnDoList<T> : ICollectionExtension.UnDoCollection<T>, IList<T>
        {
            #region Fields

            private readonly IUnDoManager _manager;
            private readonly IList<T> _source;

            #endregion

            #region Initialisation

            public UnDoList(IUnDoManager manager, IList<T> source)
                : base(manager, source)
            {
                _manager = manager;
                _source = source;
            }

            #endregion

            #region IList

            int IList<T>.IndexOf(T item)
            {
                return _source.IndexOf(item);
            }

            void IList<T>.Insert(int index, T item)
            {
                _manager.DoInsert(_source, index, item);
            }

            void IList<T>.RemoveAt(int index)
            {
                _manager.DoRemoveAt(_source, index);
            }

            T IList<T>.this[int index]
            {
                get
                {
                    return _source[index];
                }
                set
                {
                    _manager.Do(_source, index, value);
                }
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Wraps an IList to an UnDo list linked to an UnDoManager to automatically generate IUnDo operations.
        /// </summary>
        /// <typeparam name="T">The type of the list.</typeparam>
        /// <param name="source">The list.</param>
        /// <param name="manager">The UnDoManager.</param>
        /// <returns>A wrapped IList.</returns>
        public static IList<T> ToUnDo<T>(this IList<T> source, IUnDoManager manager)
        {
            source.CheckParameter(nameof(source));
            manager.CheckParameter(nameof(manager));

            return new UnDoList<T>(manager, source);
        }

        #endregion
    }
}
