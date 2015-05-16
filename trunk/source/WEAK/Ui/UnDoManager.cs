using System;
using System.Collections.Generic;

namespace WEAK.Ui
{
    public sealed class UnDoManager
    {
        #region Types

        private class Linker : IDisposable
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
                        }
                        else if (_manager._linkedCommands.Count > 0)
                        {
                            _manager._doneActions.Push(new GroupUnDo(_manager._linkedCommands.ToArray()));
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

        private int _linkerCount;

        #endregion

        #region Initialisation

        public UnDoManager()
        {
            _doneActions = new Stack<IUnDo>();
            _undoneActions = new Stack<IUnDo>();
            _linkedCommands = new List<IUnDo>();
        }

        #endregion

        #region Methods

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
            if (command == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => command));
            }

            command.Do();

            if (_linkerCount > 0)
            {
                _linkedCommands.Add(command);
            }
            else
            {
                _doneActions.Push(command);
            }

            _undoneActions.Clear();
        }

        public bool CanUndo()
        {
            return _doneActions.Count > 0 && _linkerCount == 0;
        }

        public void Undo()
        {
            if (CanUndo())
            {
                IUnDo command = _doneActions.Pop();
                command.Undo();
                _undoneActions.Push(command);
            }
        }

        public void UndoAll()
        {
            while (CanUndo())
            {
                Undo();
            }
        }

        public bool CanRedo()
        {
            return _undoneActions.Count > 0 && _linkerCount == 0;
        }

        public void Redo()
        {
            if (CanRedo())
            {
                IUnDo command = _undoneActions.Pop();
                command.Do();
                _doneActions.Push(command);
            }
        }

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
