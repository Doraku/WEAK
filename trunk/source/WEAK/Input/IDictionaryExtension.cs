using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides a method to wrap an IDictionary to an UnDo dictionary linked to an UnDoManager to automatically generate IUnDo operations.
    /// </summary>
    public static class IDictionaryExtension
    {
        #region Types

        private class UnDoDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
        {
            #region Fields

            private readonly UnDoManager _manager;
            private readonly IDictionary<TKey, TValue> _source;

            #endregion

            #region Initialisation

            public UnDoDictionary(UnDoManager manager, IDictionary<TKey, TValue> source)
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
                get
                {
                    return _source[key];
                }
                set
                {
                    _manager.Do(_source, key, value);
                }
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

            #region ICollection

            void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
            {
                _manager.DoAdd(_source, item.Key, item.Value);
            }

            void ICollection<KeyValuePair<TKey, TValue>>.Clear()
            {
                _manager.DoClear(_source);
            }

            bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            {
                return _source.Contains(item);
            }

            void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                _source.CopyTo(array, arrayIndex);
            }

            int ICollection<KeyValuePair<TKey, TValue>>.Count
            {
                get { return _source.Count; }
            }

            bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
            {
                get { return _source.IsReadOnly; }
            }

            bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
            {
                return _manager.DoRemove(_source, item.Key);
            }

            #endregion

            #region IEnumerator

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return _source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _source.GetEnumerator();
            }

            #endregion

            #region INotifyCollectionChanged

            event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
            {
                add
                {
                    if (_source is INotifyCollectionChanged)
                    {
                        (_source as INotifyCollectionChanged).CollectionChanged += value;
                    }
                }
                remove
                {
                    if (_source is INotifyCollectionChanged)
                    {
                        (_source as INotifyCollectionChanged).CollectionChanged -= value;
                    }
                }
            }

            #endregion

            #region INotifyPropertyChanged

            event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
            {
                add
                {
                    if (_source is INotifyPropertyChanged)
                    {
                        (_source as INotifyPropertyChanged).PropertyChanged += value;
                    }
                }
                remove
                {
                    if (_source is INotifyPropertyChanged)
                    {
                        (_source as INotifyPropertyChanged).PropertyChanged -= value;
                    }
                }
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
        public static IDictionary<TKey, TValue> ToUnDo<TKey, TValue>(this IDictionary<TKey, TValue> source, UnDoManager manager)
        {
            if (source == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => source));
            }
            if (manager == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => manager));
            }

            return new UnDoDictionary<TKey, TValue>(manager, source);
        }

        #endregion
    }
}
