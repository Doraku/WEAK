using System;

namespace WEAK.Input
{
    /// <summary>
    /// Provides properties and methods of the command pattern to execute operations and return to a previous state by undoing them.
    /// </summary>
    public interface IUnDoManager
    {
        /// <summary>
        /// Gets an int representing the state of the IUnDoManager.
        /// </summary>
        int Version { get; }
        /// <summary>
        /// Returns a boolean to express if an undo can be executed.
        /// </summary>
        /// <returns>true if there is an IUnDo in the history and no group operation is in progress, else false.</returns>
        bool CanUndo { get; }
        /// <summary>
        /// Returns a boolean to express if a redo can be executed.
        /// </summary>
        /// <returns>true if there is an IUnDo in the undone history and no group poeration is in progress, else false.</returns>
        bool CanRedo { get; }

        /// <summary>
        /// Starts a group of operation and return an IDisposable to stop the group.
        /// If multiple calls to this method are made, the group will be stoped once each IDisposable returned are disposed.
        /// </summary>
        /// <returns>An IDisposable to stop the group operation.</returns>
        IDisposable BeginGroup();
        /// <summary>
        /// Clears the history of IUnDo operations.
        /// </summary>
        void Clear();
        /// <summary>
        /// Executes the IUnDo command and store it in the manager hostory.
        /// </summary>
        /// <param name="command">The IUnDo to execute.</param>
        /// <exception cref="ArgumentNullException">command is null.</exception>
        void Do(IUnDo command);
        /// <summary>
        /// ReDoes the last undone IUnDo commands of the manager history.
        /// </summary>
        void Redo();
        /// <summary>
        /// UnDoes the last executed IUnDo command of the manager history.
        /// </summary>
        void Undo();
    }
}