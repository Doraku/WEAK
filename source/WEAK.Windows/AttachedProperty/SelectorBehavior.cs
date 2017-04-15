using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WEAK.Windows.AttachedProperty
{
    static public class SelectorBehavior
    {
        #region Internal types

        private class CollectionSyncher : IWeakEventListener
        {
            public WeakReference collection;

            #region IWeakEventListener

            public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                NotifyCollectionChangedEventArgs arg = e as NotifyCollectionChangedEventArgs;
                ObservableCollection<object> col = collection.Target as ObservableCollection<object>;
                if (arg != null && col != null)
                {
                    int index = arg.NewStartingIndex + 1;
                    switch (arg.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (object o in arg.NewItems)
                            {
                                col.Insert(index++, o);
                            }
                            break;

                        case NotifyCollectionChangedAction.Move:
                            col.Move(arg.OldStartingIndex + 1, arg.NewStartingIndex + 1);
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            foreach (object o in arg.OldItems)
                            {
                                col.RemoveAt(arg.OldStartingIndex + 1);
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            foreach (object o in arg.NewItems)
                            {
                                col[index++] = o;
                            }
                            break;

                        case NotifyCollectionChangedAction.Reset:
                            col.Clear();
                            col.Add(NullValue);
                            break;
                    }
                }

                return true;
            }

            #endregion
        }

        #endregion

        static private readonly ConditionalWeakTable<INotifyCollectionChanged, CollectionSyncher> _listeners = new ConditionalWeakTable<INotifyCollectionChanged, CollectionSyncher>();

        static public readonly object NullValue = new object();

        #region ItemsSourceWithNull property

        static public readonly DependencyProperty ItemsSourceWithNullProperty = DependencyProperty.RegisterAttached("ItemsSourceWithNull", typeof(IEnumerable), typeof(SelectorBehavior), new PropertyMetadata(null, OnItemsSourceWithNullChanged));

        static public void SetItemsSourceWithNull(this DependencyObject source, IEnumerable value)
        {
            source.SetValue(ItemsSourceWithNullProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Selector))]
        static public IEnumerable GetItemsSourceWithNull(this DependencyObject source)
        {
            return source.GetValue(ItemsSourceWithNullProperty) as IEnumerable;
        }

        static public void OnItemsSourceWithNullChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            Selector control = source as Selector;
            if (control == null)
            {
                throw new ArgumentException("Source is not a Selector");
            }

            if (e.OldValue != null)
            {
                INotifyCollectionChanged collection = e.OldValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    CollectionChangedEventManager.RemoveListener(collection, _listeners.GetOrCreateValue(collection));
                }
                control.SelectionChanged -= OnSelectedItemChanged;
            }

            if (e.NewValue is IEnumerable<object>)
            {
                ObservableCollection<object> itemsSourceWithNull = new ObservableCollection<object>(e.NewValue as IEnumerable<object>);
                itemsSourceWithNull.Insert(0, NullValue);

                INotifyCollectionChanged collection = e.NewValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    CollectionSyncher syncher = _listeners.GetOrCreateValue(collection);
                    syncher.collection = new WeakReference(itemsSourceWithNull);
                    CollectionChangedEventManager.AddListener(collection, _listeners.GetOrCreateValue(collection));
                }

                control.SelectionChanged += OnSelectedItemChanged;
                control.ItemsSource = itemsSourceWithNull;
                control.SelectedItem = control.SelectedItem ?? NullValue;
            }
            else
            {
                control.ItemsSource = null;
            }
        }

        #endregion

        #region SelectedItemWithNull property

        static public readonly DependencyProperty SelectedItemWithNullProperty = DependencyProperty.RegisterAttached("SelectedItemWithNull", typeof(object), typeof(SelectorBehavior), new PropertyMetadata(null, OnSelectedItemWithNullChanged));

        static public void SetSelectedItemWithNull(this DependencyObject source, object value)
        {
            source.SetValue(SelectedItemWithNullProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Selector))]
        static public object GetSelectedItemWithNull(this DependencyObject source)
        {
            return source.GetValue(SelectedItemWithNullProperty);
        }

        static public void OnSelectedItemWithNullChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            Selector control = source as Selector;
            if (control == null)
            {
                throw new ArgumentException("Source is not a Selector");
            }

            control.SelectedItem = e.NewValue ?? NullValue;
        }

        #endregion

        #region Callbacks

        static private void OnSelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            Selector control = sender as Selector;
            if (e.AddedItems.Count != 0 && e.AddedItems[0] != NullValue)
            {
                control.SetSelectedItemWithNull(e.AddedItems[0]);
            }
            else
            {
                control.SetSelectedItemWithNull(null);
            }
        }

        #endregion
    }
}
