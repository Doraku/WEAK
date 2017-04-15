using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WEAK.Windows
{
    public static class DependencyObjectExtensions
    {
        #region Fields

        private static readonly object _locker;
        private static readonly List<DependencyObject> _isUpdating;

        #endregion

        #region Initialisation

        static DependencyObjectExtensions()
        {
            _locker = new object();
            _isUpdating = new List<DependencyObject>();
        }

        #endregion

        #region Methods

        public static bool BeginUpdate(this DependencyObject target)
        {
            lock (_locker)
            {
                if (_isUpdating.Contains(target))
                {
                    return false;
                }
                else
                {
                    _isUpdating.Add(target);
                    return true;
                }
            }
        }

        public static void EndUpdate(this DependencyObject target)
        {
            lock (_locker)
            {
                _isUpdating.Remove(target);
            }
        }

        public static bool TryGetParent<T>(this DependencyObject reference, out T parent) where T : DependencyObject
        {
            parent = null;
            
            while (reference != null)
            {
                reference = VisualTreeHelper.GetParent(reference);
                if (reference is T)
                {
                    parent = reference as T;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetFirstChild<T>(this DependencyObject reference, out T child) where T : DependencyObject
        {
            child = null;
            if (VisualTreeHelper.GetChildrenCount(reference) > 0)
            {
                child = VisualTreeHelper.GetChild(reference, 0) as T;
            }

            return child != null;
        }

        #endregion
    }
}
