using System.Collections.Generic;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides a method to wrap an IDictionary to an UnDo dictionary linked to an UnDoManager to automatically generate IUnDo operations.
    /// </summary>
    public static class IDictionaryExtension
    {
        #region Types

        internal class UnDoDictionary<TKey, TValue> : ICollectionExtension.UnDoCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
        {
            #region Fields

            private readonly IUnDoManager _manager;
            private readonly IDictionary<TKey, TValue> _source;

            #endregion

            #region Initialisation

            public UnDoDictionary(IUnDoManager manager, IDictionary<TKey, TValue> source)
                : base(manager, source)
            {
                _manager = manager;
                _source = source;
            }

            #endregion

            #region IDictionary

            void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
            {
                _manager.DoAdd(_source, key, value);
            }

            bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
            {
                return _source.ContainsKey(key);
            }

            bool IDictionary<TKey, TValue>.Remove(TKey key)
            {
                return _manager.DoRemove(_source, key);
            }

            bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
            {
                return _source.TryGetValue(key, out value);
            }

            TValue IDictionary<TKey, TValue>.this[TKey key]
            {
                get { return _source[key]; }
                set { _manager.Do(_source, key, value); }
            }

            ICollection<TKey> IDictionary<TKey, TValue>.Keys
            {
                get { return _source.Keys; }
            }

            ICollection<TValue> IDictionary<TKey, TValue>.Values
            {
                get { return _source.Values; }
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Wraps an IDictionary to an UnDo dictionary linked to an UnDoManager to automatically generate IUnDo operations.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values.</typeparam>
        /// <param name="source">The dictionary.</param>
        /// <param name="manager">The UnDoManager.</param>
        /// <returns>A wrapped IDictionary.</returns>
        public static IDictionary<TKey, TValue> ToUnDo<TKey, TValue>(this IDictionary<TKey, TValue> source, IUnDoManager manager)
        {
            source.CheckParameter(nameof(source));
            manager.CheckParameter(nameof(manager));

            return new UnDoDictionary<TKey, TValue>(manager, source);
        }

        #endregion
    }
}
