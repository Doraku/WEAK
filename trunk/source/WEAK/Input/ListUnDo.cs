using System;
using System.Collections.Generic;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides an implementation of the IUnDo interface for IList operation.
    /// </summary>
    /// <typeparam name="T">The type of the list.</typeparam>
    public sealed class ListUnDo<T> : IUnDo
    {
        #region Fields

        private readonly IList<T> _source;
        private readonly int _index;
        private readonly T _element;
        private readonly bool _isAdd;

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialises an instance of ListUnDo.
        /// </summary>
        /// <param name="source">The list on which the operation is performed.</param>
        /// <param name="index">The index of the operation.</param>
        /// <param name="element">The argument of the operation.</param>
        /// <param name="isAdd">true if the operation is an Insert, else false for a RemoveAt.</param>
        /// <exception cref="System.ArgumentNullException">source is null.</exception>
        public ListUnDo(IList<T> source, int index, T element, bool isAdd)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            _source = source;
            _index = index;
            _element = element;
            _isAdd = isAdd;
        }

        #endregion

        #region Methods

        private void Action(bool isAdd)
        {
            if (isAdd)
            {
                _source.Insert(_index, _element);
            }
            else
            {
                _source.RemoveAt(_index);
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
