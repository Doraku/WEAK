using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WEAK.Ui
{
    public static class ICollectionExtension
    {
        #region Types

        private class UnDoCollection<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
        {
            #region Fields

            private readonly UnDoManager _manager;
            private readonly ICollection<T> _source;

            #endregion

            #region Initialisation

            public UnDoCollection(UnDoManager manager, ICollection<T> source)
            {
                _manager = manager;
                _source = source;
            }

            #endregion

            #region ICollection

            void ICollection<T>.Add(T item)
            {
                _manager.DoAdd(_source, item);
            }

            void ICollection<T>.Clear()
            {
                _manager.DoClear(_source);
            }

            bool ICollection<T>.Contains(T item)
            {
                return _source.Contains(item);
            }

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                _source.CopyTo(array, arrayIndex);
            }

            int ICollection<T>.Count
            {
                get { return _source.Count; }
            }

            bool ICollection<T>.IsReadOnly
            {
                get { return _source.IsReadOnly; }
            }

            bool ICollection<T>.Remove(T item)
            {
                return _manager.DoRemove(_source, item);
            }

            #endregion

            #region IEnumerator

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
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

        public static ICollection<T> ToUnDo<T>(this ICollection<T> source, UnDoManager manager)
        {
            if (source == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => source));
            }
            if (manager == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => manager));
            }

            return new UnDoCollection<T>(manager, source);
        }

        #endregion
    }
}
