using System;
using System.Collections.Generic;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides an implementation of the command pattern to execute operations and return to a previous state by undoing them.
    /// </summary>
    public sealed class UnDoManager
    {
        #region Types

        private sealed class Linker : IDisposable
        {
            #region Fields

            private readonly UnDoManager _manager;

            private bool _isDisposed;

            #endregion

            #region Initialisation

            public Linker(UnDoManager manager)
            {
                _manager = manager;
                _isDisposed = false;

                ++_manager._linkerCount;
            }

            ~Linker()
            {
                Dispose();
            }

            #endregion

            #region IDisposable

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    if (--_manager._linkerCount == 0)
                    {
                        if (_manager._linkedCommands.Count == 1)
                        {
                            _manager._doneActions.Push(_manager._linkedCommands[0]);
                            _manager.AddVersion();
                        }
                        else if (_manager._linkedCommands.Count > 0)
                        {
                            _manager._doneActions.Push(new GroupUnDo(_manager._linkedCommands.ToArray()));
                            _manager.AddVersion();
                        }

                        _manager._linkedCommands.Clear();
                    }

                    _isDisposed = true;
                    GC.SuppressFinalize(this);
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly Stack<IUnDo> _doneActions;
        private readonly Stack<IUnDo> _undoneActions;
        private readonly List<IUnDo> _linkedCommands;
        private readonly Stack<int> _doneVersions;
        private readonly Stack<int> _undoneVersions;

        private int _currentVersion;
        private int _lastVersion;
        private int _linkerCount;

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialises an instance of UnDoManager.
        /// </summary>
        public UnDoManager()
        {
            _doneActions = new Stack<IUnDo>();
            _undoneActions = new Stack<IUnDo>();
            _linkedCommands = new List<IUnDo>();
            _doneVersions = new Stack<int>();
            _undoneVersions = new Stack<int>();

            _currentVersion = 0;
            _lastVersion = 0;

            _doneVersions.Push(_currentVersion);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an int representing the state of the UnDoManager.
        /// </summary>
        public int Version
        {
            get { return _currentVersion; }
        }

        #endregion

        #region Methods

        private void AddVersion()
        {
            _doneVersions.Push(++_lastVersion);
            _currentVersion = _lastVersion;
            _undoneVersions.Clear();
        }

        /// <summary>
        /// Starts a group of operation and return an IDisposable to stop the group.
        /// If multiple calls to this method are made, the group will be stoped once each IDisposable returned are disposed.
        /// </summary>
        /// <returns>An IDisposable to stop the group operation.</returns>
        public IDisposable BeginGroup()
        {
            return new Linker(this);
        }

        /// <summary>
        /// Clears the history of IUnDo operations.
        /// </summary>
        public void Clear()
        {
            _doneActions.Clear();
            _undoneActions.Clear();
        }

        /// <summary>
        /// Executes the IUnDo command and store it in the manager hostory.
        /// </summary>
        /// <param name="command">The IUnDo to execute.</param>
        public void Do(IUnDo command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command.Do();

            if (_linkerCount > 0)
            {
                _linkedCommands.Add(command);
            }
            else
            {
                _doneActions.Push(command);
                AddVersion();
            }

            _undoneActions.Clear();
        }

        /// <summary>
        /// Returns a boolean to express if an undo can be executed.
        /// </summary>
        /// <returns>true if there is an IUnDo in the history and no group operation is in progress, else false.</returns>
        public bool CanUndo()
        {
            return _doneActions.Count > 0 && _linkerCount == 0;
        }

        /// <summary>
        /// UnDoes the last executed IUnDo command of the manager history.
        /// </summary>
        public void Undo()
        {
            if (CanUndo())
            {
                IUnDo command = _doneActions.Pop();
                command.Undo();
                _undoneActions.Push(command);

                int version = _doneVersions.Pop();
                _undoneVersions.Push(version);
                _currentVersion = _doneVersions.Peek();
            }
        }

        /// <summary>
        /// UnDoes all executed IUnDo commands of the manager history.
        /// </summary>
        public void UndoAll()
        {
            while (CanUndo())
            {
                Undo();
            }
        }

        /// <summary>
        /// Returns a boolean to express if a redo can be executed.
        /// </summary>
        /// <returns>true if there is an IUnDo in the undone history and no group poeration is in progress, else false.</returns>
        public bool CanRedo()
        {
            return _undoneActions.Count > 0 && _linkerCount == 0;
        }

        /// <summary>
        /// ReDoes the last undone IUnDo commands of the manager history.
        /// </summary>
        public void Redo()
        {
            if (CanRedo())
            {
                IUnDo command = _undoneActions.Pop();
                command.Do();
                _doneActions.Push(command);

                int version = _undoneVersions.Pop();
                _doneVersions.Push(version);
                _currentVersion = version;
            }
        }

        /// <summary>
        /// ReDoes all undone IUnDo commands of the manager history.
        /// </summary>
        public void RedoAll()
        {
            while (CanRedo())
            {
                Redo();
            }
        }

        #endregion
    }
}
