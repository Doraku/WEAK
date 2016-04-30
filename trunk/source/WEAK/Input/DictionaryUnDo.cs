using System;
using System.Collections.Generic;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides an implementation of the IUnDo interface for IDictionary operation.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    public sealed class DictionaryUnDo<TKey, TValue> : IUnDo
    {
        #region Fields

        private readonly IDictionary<TKey, TValue> _source;
        private readonly TKey _key;
        private readonly TValue _element;
        private readonly bool _isAdd;

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialise an instance of DictionaryUnDo.
        /// </summary>
        /// <param name="source">The dictionary on which to perform operation.</param>
        /// <param name="key">The key of the operation.</param>
        /// <param name="element">The value of the operation.</param>
        /// <param name="isAdd">true if the operation is Add, false for Remove.</param>
        /// <exception cref="System.ArgumentNullException">source or key is null.</exception>
        public DictionaryUnDo(IDictionary<TKey, TValue> source, TKey key, TValue element, bool isAdd)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _source = source;
            _key = key;
            _element = element;
            _isAdd = isAdd;
        }

        #endregion

        #region Methods

        private void Action(bool isAdd)
        {
            if (isAdd)
            {
                _source.Add(_key, _element);
            }
            else
            {
                _source.Remove(_key);
            }
        }

        #endregion

        #region IUnDo

        void IUnDo.Do()
        {
            Action(_isAdd);
        }

        void IUnDo.Undo()
        {
            Action(!_isAdd);
        }

        #endregion
    }
}
