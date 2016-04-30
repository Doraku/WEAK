using System;
using System.Collections.Generic;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides an implementation of the IUnDo interface for ICollection operation.
    /// </summary>
    /// <typeparam name="T">The type of the ICollection.</typeparam>
    public sealed class CollectionUnDo<T> : IUnDo
    {
        #region Fields

        private readonly ICollection<T> _source;
        private readonly T _element;
        private readonly bool _isAdd;

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialise an instance of CollectionUnDo.
        /// </summary>
        /// <param name="source">The collection on which to perform operation.</param>
        /// <param name="element">The argument of the operation.</param>
        /// <param name="isAdd">true if the operation is an Add, false for a Remove.</param>
        public CollectionUnDo(ICollection<T> source, T element, bool isAdd)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            _source = source;
            _element = element;
            _isAdd = isAdd;
        }

        #endregion

        #region Methods

        private void Action(bool isAdd)
        {
            if (isAdd)
            {
                _source.Add(_element);
            }
            else
            {
                _source.Remove(_element);
            }
        }

        #endregion

        #region IUnDo

        void IUnDo.Undo()
        {
            Action(!_isAdd);
        }

        void IUnDo.Do()
        {
            Action(_isAdd);
        }

        #endregion
    }
}
