using System.Linq;
using WEAK.Helper;

namespace WEAK.Input
{
    /// <summary>
    /// Provides an implementation of the IUnDo interface for a group of operations.
    /// </summary>
    public sealed class GroupUnDo : IUnDo
    {
        #region Fields

        public readonly IUnDo[] _commands;

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialise an instance of GroupUnDo.
        /// </summary>
        /// <param name="commands">The sequence of IUnDo contained by the instance.</param>
        /// <exception cref="System.ArgumentNullException">commands is null.</exception>
        /// <exception cref="System.ArgumentException">commands contains null elements.</exception>
        public GroupUnDo(params IUnDo[] commands)
        {
            commands.CheckForArgumentNullException(nameof(commands));
            commands.CheckForArgumentException(nameof(commands), c => !c.Any(i => i == null), "IUnDo sequence contains null elements.");

            _commands = commands;
        }

        #endregion

        #region IUnDo

        void IUnDo.Do()
        {
            foreach (IUnDo command in _commands)
            {
                command.Do();
            }
        }

        void IUnDo.Undo()
        {
            for (int i = _commands.Length - 1; i >= 0; --i)
            {
                _commands[i].Undo();
            }
        }

        #endregion
    }
}
