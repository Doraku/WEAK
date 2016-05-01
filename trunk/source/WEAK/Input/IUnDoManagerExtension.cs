using System;
using System.Collections.Generic;
using System.Linq;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides methods to create IUnDo command and add them to an IUnDoManager.
    /// </summary>
    public static class IUnDoManagerExtension
    {
        #region Methods

        public static void DoAdd<T>(this IUnDoManager manager, ICollection<T> source, T value)
        {
            manager.CheckParameter(nameof(manager));
            source.CheckParameter(nameof(source));

            manager.Do(new CollectionUnDo<T>(source, value, true));
        }

        public static void DoClear<T>(this IUnDoManager manager, ICollection<T> source)
        {
            manager.CheckParameter(nameof(manager));
            source.CheckParameter(nameof(source));

            List<T> oldValues = source.ToList();

            manager.Do(source.Clear, () => oldValues.ForEach(v => source.Add(v)));
        }

        public static bool DoRemove<T>(this IUnDoManager manager, ICollection<T> source, T value)
        {
            manager.CheckParameter(nameof(manager));
            source.CheckParameter(nameof(source));

            bool result = false;

            if (source.Contains(value))
            {
                manager.Do(new CollectionUnDo<T>(source, value, false));
                result = true;
            }

            return result;
        }

        public static void DoAdd<TKey, TValue>(this IUnDoManager manager, IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            manager.CheckParameter(nameof(manager));
            source.CheckParameter(nameof(source));
            key.CheckParameter(nameof(key));

            manager.Do(new DictionaryUnDo<TKey, TValue>(source, key, value, true));
        }

        public static bool DoRemove<TKey, TValue>(this IUnDoManager manager, IDictionary<TKey, TValue> source, TKey key)
        {
            manager.CheckParameter(nameof(manager));
            source.CheckParameter(nameof(source));
            key.CheckParameter(nameof(key));

            bool result = false;
            TValue value;
            if (source.TryGetValue(key, out value))
            {

                manager.Do(new DictionaryUnDo<TKey, TValue>(source, key, value, false));
                result = true;
            }

            return result;
        }

        public static void Do<TKey, TValue>(this IUnDoManager manager, IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            manager.CheckParameter(nameof(manager));
            source.CheckParameter(nameof(source));
            key.CheckParameter(nameof(key));

            TValue oldValue;
            if (source.TryGetValue(key, out oldValue))
            {
                manager.Do(v => source[key] = v, oldValue, value);
            }
            else
            {
                manager.Do(() => source[key] = value, () => source.Remove(key));
            }
        }

        public static void DoInsert<T>(this IUnDoManager manager, IList<T> source, int index, T value)
        {
            manager.CheckParameter(nameof(manager));
            source.CheckParameter(nameof(source));

            manager.Do(new ListUnDo<T>(source, index, value, true));
        }

        public static void DoRemoveAt<T>(this IUnDoManager manager, IList<T> source, int index)
        {
            manager.CheckParameter(nameof(manager));
            source.CheckParameter(nameof(source));

            manager.Do(new ListUnDo<T>(source, index, source[index], false));
        }

        public static void Do<T>(this IUnDoManager manager, IList<T> source, int index, T value)
        {
            manager.CheckParameter(nameof(manager));
            source.CheckParameter(nameof(source));

            manager.Do(v => source[index] = v, source[index], value);
        }

        public static void Do(this IUnDoManager manager, Action doAction, Action undoAction)
        {
            manager.CheckParameter(nameof(manager));
            doAction.CheckParameter(nameof(doAction));
            undoAction.CheckParameter(nameof(undoAction));

            manager.Do(new UnDo(doAction, undoAction));
        }

        public static void Do<T>(this IUnDoManager manager, Action<T> setter, T oldValue, T newValue)
        {
            manager.CheckParameter(nameof(manager));
            setter.CheckParameter(nameof(setter));

            manager.Do(new ValueUnDo<T>(setter, oldValue, newValue));
        }

        public static void UndoAll(this IUnDoManager manager)
        {
            manager.CheckParameter(nameof(manager));

            while (manager.CanUndo)
            {
                manager.Undo();
            }
        }

        public static void RedoAll(this IUnDoManager manager)
        {
            manager.CheckParameter(nameof(manager));

            while (manager.CanRedo)
            {
                manager.Redo();
            }
        }

        #endregion
    }
}
