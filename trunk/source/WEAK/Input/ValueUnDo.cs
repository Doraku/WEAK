using System;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides an implementation of the IUnDo interface for setting value.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    public sealed class ValueUnDo<T> : IUnDo
    {
        #region Fields

        private readonly Action<T> _setter;
        private readonly T _oldValue;
        private readonly T _newValue;

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialises an instance of ValueUnDo.
        /// </summary>
        /// <param name="setter">The action called to set the value.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <exception cref="System.ArgumentNullException">setter is null.</exception>
        public ValueUnDo(Action<T> setter, T oldValue, T newValue)
        {
            setter.CheckForArgumentNullException(nameof(setter));

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
