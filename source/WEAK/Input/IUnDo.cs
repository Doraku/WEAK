﻿namespace WEAK.Input
{
    /// <summary>
    /// Provides methods to execute an action and remove its effect.
    /// </summary>
    public interface IUnDo
    {
        /// <summary>
        /// Does an action.
        /// </summary>
        void Do();
        /// <summary>
        /// Does the opposite of the Do action.
        /// </summary>
        void Undo();
    }
}
