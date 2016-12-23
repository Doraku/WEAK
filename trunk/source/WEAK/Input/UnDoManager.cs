using System;
using System.Collections.Generic;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides an implementation of the command pattern to execute operations and return to a previous state by undoing them.
    /// </summary>
    public sealed class UnDoManager : IUnDoManager
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

            #endregion

            #region IDisposable

            void IDisposable.Dispose()
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

        #region Methods

        private void AddVersion()
        {
            _doneVersions.Push(++_lastVersion);
            _currentVersion = _lastVersion;
            _undoneVersions.Clear();
        }

        #endregion

        #region IUnDoManager

        public int Version => _currentVersion;

        public bool CanUndo => _doneActions.Count > 0;

        public bool CanRedo => _undoneActions.Count > 0;

        public IDisposable BeginGroup()
        {
            return new Linker(this);
        }

        public void Clear()
        {
            _doneActions.Clear();
            _undoneActions.Clear();
        }

        public void Do(IUnDo command)
        {
            command.CheckForArgumentNullException(nameof(command));

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

        public void Undo()
        {
            if (_linkerCount != 0)
            {
                throw new InvalidOperationException("Cannot perform Undo while a group operation is going on.");
            }

            IUnDo command;
            try
            {
                command = _doneActions.Pop();
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("There is no action to undo.");
            }

            command.Undo();
            _undoneActions.Push(command);

            int version = _doneVersions.Pop();
            _undoneVersions.Push(version);
            _currentVersion = _doneVersions.Peek();
        }

        public void Redo()
        {
            if (_linkerCount != 0)
            {
                throw new InvalidOperationException("Cannot perform Redo while a group operation is going on.");
            }

            IUnDo command;
            try
            {
                command = _undoneActions.Pop();
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("There is no action to redo.");
            }

            command.Do();
            _doneActions.Push(command);

            int version = _undoneVersions.Pop();
            _doneVersions.Push(version);
            _currentVersion = version;
        }

        #endregion
    }
}
