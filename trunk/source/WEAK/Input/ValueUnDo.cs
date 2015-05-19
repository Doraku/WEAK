using System;

namespace WEAK.Input
{
    public sealed class ValueUnDo<T> : IUnDo
    {
        #region Fields

        private readonly Action<T> _setter;
        private readonly T _oldValue;
        private readonly T _newValue;

        #endregion

        #region Initialisation

        public ValueUnDo(Action<T> setter, T oldValue, T newValue)
        {
            if (setter == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => setter));
            }

            _setter = setter;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        #endregion

        #region IUnDo

        void IUnDo.Do()
        {
            _setter(_newValue);
        }

        void IUnDo.Undo()
        {
            _setter(_oldValue);
        }

        #endregion
    }
}
