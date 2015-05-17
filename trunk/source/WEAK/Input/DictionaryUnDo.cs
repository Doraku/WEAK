using System.Collections.Generic;

namespace WEAK.Input
{
    public sealed class DictionaryUnDo<TKey, TValue> : IUnDo
    {
        #region Fields

        private readonly IDictionary<TKey, TValue> _source;
        private readonly TKey _key;
        private readonly TValue _element;
        private readonly bool _isAdd;

        #endregion

        #region Initialisation

        public DictionaryUnDo(IDictionary<TKey, TValue> source, TKey key, TValue element, bool isAdd)
        {
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
