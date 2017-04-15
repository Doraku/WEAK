using System;
using System.Collections.Generic;
using WEAK.Helper;

namespace WEAK.Linq
{
    public static class IEnumerableExtension
    {
        #region Methods

        private static IEnumerable<T> Get<T>(this IEnumerator<T> enumerator, int size)
        {
            do
            {
                yield return enumerator.Current;
            }
            while (--size > 0 && enumerator.MoveNext());
        }

        public static void Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            source.CheckForArgumentNullException(nameof(source));
            action.CheckForArgumentNullException(nameof(action));

            foreach (T element in source)
            {
                action(element);
            }
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            source.CheckForArgumentNullException(nameof(source));
            size.CheckForArgumentException(nameof(size), s => s > 0, "size must be superior to zero");

            IEnumerator<T> enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return Get(enumerator, size);
            }
        }

        #endregion
    }
}
