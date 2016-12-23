using System;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides an implementation of the IUnDo interface for custom do and undo action.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    public sealed class UnDo : IUnDo
    {
        #region Fields

        private readonly Action _doAction;
        private readonly Action _undoAction;

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialises an instance of UnDo.
        /// </summary>
        /// <param name="setter">The action called by do.</param>
        /// <param name="oldValue">The action called by undo.</param>
        /// <param name="newValue">The new value.</param>
        /// <exception cref="System.ArgumentNullException">doAction or undoAction is null.</exception>
        public UnDo(Action doAction, Action undoAction)
        {
            doAction.CheckForArgumentNullException(nameof(doAction));
            undoAction.CheckForArgumentNullException(nameof(undoAction));

            _doAction = doAction;
            _undoAction = undoAction;
        }

        #endregion

        #region IUnDo

        public void Do()
        {
            _doAction();
        }

        public void Undo()
        {
            _undoAction();
        }

        #endregion
    }
}
