namespace WEAK.Ui
{
    public class GroupUnDo : IUnDo
    {
        #region Fields

        public readonly IUnDo[] _commands;

        #endregion

        #region Initialisation

        public GroupUnDo(params IUnDo[] commands)
        {
            _commands = commands;
        }

        #endregion

        #region IUnDo

        void IUnDo.Undo()
        {
            for (int i = _commands.Length - 1; i >= 0; --i)
            {
                _commands[i].Undo();
            }
        }

        void IUnDo.Do()
        {
            foreach (IUnDo command in _commands)
            {
                command.Do();
            }
        }

        #endregion
    }
}
