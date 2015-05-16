using System.Collections.Generic;

namespace WEAK.Ui
{
    public sealed class CollectionUnDo<T> : IUnDo
    {
        #region Fields

        private readonly ICollection<T> _source;
        private readonly T _element;
        private readonly bool _isAdd;

        #endregion

        #region Initialisation

        public CollectionUnDo(ICollection<T> source, T element, bool isAdd)
        {
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
