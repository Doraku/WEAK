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

        /// <summary>
        /// Does a CollectionUnDo command on the manager to add the value to the source.
        /// </summary>
        /// <typeparam name="T">The type of the ICollection elements</typeparam>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="source">The ICollection.</param>
        /// <param name="value">The value to add.</param>
        /// <exception cref="ArgumentNullException">manager or source is null.</exception>
        public static void DoAdd<T>(this IUnDoManager manager, ICollection<T> source, T value)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            source.CheckForArgumentNullException(nameof(source));

            manager.Do(new CollectionUnDo<T>(source, value, true));
        }

        /// <summary>
        /// Does a UnDo command on the manager to clear the source and to add back values as the undo operation.
        /// </summary>
        /// <typeparam name="T">The type of the ICollection elements</typeparam>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="source">The ICollection.</param>
        /// <exception cref="ArgumentNullException">manager or source is null.</exception>
        public static void DoClear<T>(this IUnDoManager manager, ICollection<T> source)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            source.CheckForArgumentNullException(nameof(source));

            List<T> oldValues = source.ToList();

            manager.Do(source.Clear, () => oldValues.ForEach(v => source.Add(v)));
        }

        /// <summary>
        /// Does a CollectionUndo command on the manager to remove the value from the source.
        /// </summary>
        /// <typeparam name="T">The type of the ICollection elements</typeparam>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="source">The ICollection.</param>
        /// <param name="value">The value to remove.</param>
        /// <returns>true if the command has been created, false if not because the source did not contained the value.</returns>
        /// <exception cref="ArgumentNullException">manager or source is null.</exception>
        public static bool DoRemove<T>(this IUnDoManager manager, ICollection<T> source, T value)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            source.CheckForArgumentNullException(nameof(source));

            bool result = false;

            if (source.Contains(value))
            {
                manager.Do(new CollectionUnDo<T>(source, value, false));
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Does a DictionaryUnDo command on the manager to add the key value to the source.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="source">The IDictionary.</param>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        /// <exception cref="ArgumentNullException">manager, source or key is null.</exception>
        public static void DoAdd<TKey, TValue>(this IUnDoManager manager, IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            source.CheckForArgumentNullException(nameof(source));
            key.CheckForArgumentNullException(nameof(key));

            manager.Do(new DictionaryUnDo<TKey, TValue>(source, key, value, true));
        }

        /// <summary>
        /// Does a DictionaryUnDo command on the manager to remove the key from the source.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="source">The IDictionary.</param>
        /// <param name="key">The key to remove.</param>
        /// <returns>true if the command has been created, false if not because the source did not contained the key.</returns>
        /// <exception cref="ArgumentNullException">manager, source or key is null.</exception>
        public static bool DoRemove<TKey, TValue>(this IUnDoManager manager, IDictionary<TKey, TValue> source, TKey key)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            source.CheckForArgumentNullException(nameof(source));
            key.CheckForArgumentNullException(nameof(key));

            bool result = false;
            TValue value;
            if (source.TryGetValue(key, out value))
            {
                manager.Do(new DictionaryUnDo<TKey, TValue>(source, key, value, false));
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Does a ValueUnDo command on the manager to set the value on the key if the source contains already the key.
        /// Else does a UnDo command on the manager to set the value on the key and remove the key as the undo operation.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="source">The IDictionary.</param>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to add.</param>
        /// <exception cref="ArgumentNullException">manager, source or key is null.</exception>
        public static void Do<TKey, TValue>(this IUnDoManager manager, IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            source.CheckForArgumentNullException(nameof(source));
            key.CheckForArgumentNullException(nameof(key));

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

        /// <summary>
        /// Does a ListUnDo command on the manager to insert the value at the specified index on the source.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="source">The IList.</param>
        /// <param name="index">The index at which the value is inserted.</param>
        /// <param name="value">The value to add.</param>
        /// <exception cref="ArgumentNullException">manager or source is null.</exception>
        public static void DoInsert<T>(this IUnDoManager manager, IList<T> source, int index, T value)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            source.CheckForArgumentNullException(nameof(source));

            manager.Do(new ListUnDo<T>(source, index, value, true));
        }

        /// <summary>
        /// Does a ListUnDo command on the manager to remove the value at the specified index on the source.
        /// </summary>
        /// <typeparam name="T">The type of the IList elements.</typeparam>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="source">The IList.</param>
        /// <param name="index">The index at which the value is remove.</param>
        /// <exception cref="ArgumentNullException">manager or source is null.</exception>
        public static void DoRemoveAt<T>(this IUnDoManager manager, IList<T> source, int index)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            source.CheckForArgumentNullException(nameof(source));

            manager.Do(new ListUnDo<T>(source, index, source[index], false));
        }

        /// <summary>
        /// Does a ValueUnDo command on the manager to change the value of the source at the specified index.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="source">The IList.</param>
        /// <param name="index">The index at which the value is changed.</param>
        /// <param name="value">The new value.</param>
        /// <exception cref="ArgumentNullException">manager or source is null.</exception>
        public static void Do<T>(this IUnDoManager manager, IList<T> source, int index, T value)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            source.CheckForArgumentNullException(nameof(source));

            manager.Do(v => source[index] = v, source[index], value);
        }

        /// <summary>
        /// Does a UnDo command on the manager with the specified doAction and undoAction.
        /// </summary>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="doAction">The action performed by the Do of the command.</param>
        /// <param name="undoAction">The action performed by the Undo of the command.</param>
        /// <exception cref="ArgumentNullException">manager, doAction or undoAction is null.</exception>
        public static void Do(this IUnDoManager manager, Action doAction, Action undoAction)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            doAction.CheckForArgumentNullException(nameof(doAction));
            undoAction.CheckForArgumentNullException(nameof(undoAction));

            manager.Do(new UnDo(doAction, undoAction));
        }

        /// <summary>
        /// Does a ValueUnDo command on the manager to change a value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="manager">The IUnDoManager.</param>
        /// <param name="setter">The action used to change the value.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <exception cref="ArgumentNullException">manager or setter is null.</exception>
        public static void Do<T>(this IUnDoManager manager, Action<T> setter, T oldValue, T newValue)
        {
            manager.CheckForArgumentNullException(nameof(manager));
            setter.CheckForArgumentNullException(nameof(setter));

            manager.Do(new ValueUnDo<T>(setter, oldValue, newValue));
        }

        /// <summary>
        /// Undoes all command in the manager.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <exception cref="ArgumentNullException">manager is null.</exception>
        public static void UndoAll(this IUnDoManager manager)
        {
            manager.CheckForArgumentNullException(nameof(manager));

            while (manager.CanUndo)
            {
                manager.Undo();
            }
        }

        /// <summary>
        /// Redoes all command in the manager.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <exception cref="ArgumentNullException">manager is null.</exception>
        public static void RedoAll(this IUnDoManager manager)
        {
            manager.CheckForArgumentNullException(nameof(manager));

            while (manager.CanRedo)
            {
                manager.Redo();
            }
        }

        #endregion
    }
}
