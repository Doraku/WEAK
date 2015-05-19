using System;
using System.Collections.Generic;

namespace WEAK.Input
{
    public sealed class ListUnDo<T> : IUnDo
    {
        #region Fields

        private readonly IList<T> _source;
        private readonly int _index;
        private readonly T _element;
        private readonly bool _isAdd;

        #endregion

        #region Initialisation

        public ListUnDo(IList<T> source, int index, T element, bool isAdd)
        {
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
