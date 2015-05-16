using System;
using System.Collections.Generic;
using System.Linq;

namespace WEAK.Ui
{
    public static class UnDoManagerExtension
    {
        #region Methods

        public static void Do<T>(this UnDoManager manager, Action<T> setter, T oldValue, T newValue)
        {
            if ((!ReferenceEquals(oldValue, null) && !oldValue.Equals(newValue))
                || (!ReferenceEquals(newValue, null) && !newValue.Equals(oldValue)))
            {
                manager.Do(new ValueUnDo<T>(setter, oldValue, newValue));
            }
        }

        public static void DoAdd<T>(this UnDoManager manager, ICollection<T> source, T value)
        {
            manager.Do(new CollectionUnDo<T>(source, value, true));
        }

        public static bool DoRemove<T>(this UnDoManager manager, ICollection<T> source, T value)
        {
            if (source.Contains(value))
            {
                manager.Do(new CollectionUnDo<T>(source, value, false));

                return true;
            }

            return false;
        }

        public static void DoClear<T>(this UnDoManager manager, ICollection<T> source)
        {
            using (manager.BeginGroup())
            {
                while (source.Count > 0)
                {
                    manager.DoRemove(source, source.First());
                }
            }
        }

        public static void DoAdd<T>(this UnDoManager manager, IList<T> source, T value)
        {
            manager.Do(new ListUnDo<T>(source, source.Count, value, true));
        }

        public static void DoInsert<T>(this UnDoManager manager, IList<T> source, int index, T value)
        {
            manager.Do(new ListUnDo<T>(source, index, value, true));
        }

        public static void Do<T>(this UnDoManager manager, IList<T> source, int index, T value)
        {
            manager.Do(v => source[index] = v, source[index], value);
        }

        public static bool DoRemove<T>(this UnDoManager manager, IList<T> source, T value)
        {
            if (source.Contains(value))
            {
                manager.Do(new ListUnDo<T>(source, source.IndexOf(value), value, false));

                return true;
            }

            return false;
        }

        public static void DoRemoveAt<T>(this UnDoManager manager, IList<T> source, int index)
        {
            manager.Do(new ListUnDo<T>(source, index, source[index], false));
        }

        public static void DoClear<T>(this UnDoManager manager, IList<T> source)
        {
            using (manager.BeginGroup())
            {
                while (source.Count > 0)
                {
                    manager.DoRemoveAt(source, 0);
                }
            }
        }

        public static void DoAdd<TKey, TValue>(this UnDoManager manager, IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            manager.Do(new DictionaryUnDo<TKey, TValue>(source, key, value, true));
        }

        public static void Do<TKey, TValue>(this UnDoManager manager, IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            if (source.ContainsKey(key))
            {
                manager.Do(v => source[key] = v, source[key], value);
            }
            else
            {
                manager.DoAdd(source, key, value);
            }
        }

        public static bool DoRemove<TKey, TValue>(this UnDoManager manager, IDictionary<TKey, TValue> source, TKey key)
        {
            if (source.ContainsKey(key))
            {
                manager.Do(new DictionaryUnDo<TKey, TValue>(source, key, source[key], false));

                return true;
            }

            return false;
        }

        public static void DoClear<TKey, TValue>(this UnDoManager manager, IDictionary<TKey, TValue> source)
        {
            using (manager.BeginGroup())
            {
                while (source.Count > 0)
                {
                    manager.DoRemove(source, source.Keys.First());
                }
            }
        }

        #endregion
    }
}
