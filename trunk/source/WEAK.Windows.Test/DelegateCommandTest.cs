using Microsoft.VisualStudio.TestTools.UnitTesting;
using WEAK.Windows.Input;

namespace WEAK.Windows.Test
{
    [TestClass]
    public class DelegateCommandTest
    {
        #region Types

        private class Dummy
        { }

        #endregion

        #region Methods

        [TestMethod]
        public void NoTypeTest()
        {
            bool done = false;
            DelegateCommand command = new DelegateCommand(() => done = true);

            if (command.CanExecute())
            {
                command.Execute();
            }

            Assert.IsTrue(done);

            done = false;
            command = new DelegateCommand(Assert.Fail, () => false);

            if (command.CanExecute())
            {
                command.Execute();
            }

            done = false;
            command = new DelegateCommand(() => done = true, () => true);

            if (command.CanExecute())
            {
                command.Execute();
            }

            Assert.IsTrue(done);
        }

        [TestMethod]
        public void GenericClassTest()
        {
            bool done = false;
            DelegateCommand<Dummy> command = new DelegateCommand<Dummy>(p => done = true);
            Dummy parameter = new Dummy();

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }

            Assert.IsTrue(done);

            done = false;
            command = new DelegateCommand<Dummy>(p => done = true, p => true);
            parameter = new Dummy();

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }

            Assert.IsTrue(done);
        }

        [TestMethod]
        public void GenericStructTest()
        {
            bool done = false;
            DelegateCommand<int> command = new DelegateCommand<int>(p => done = true);
            int parameter = 42;

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }

            Assert.IsTrue(done);

            done = false;
            command = new DelegateCommand<int>(p => Assert.Fail(), p => false);

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }

            done = false;
            command = new DelegateCommand<int>(p => done = true, p => true);

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }

            Assert.IsTrue(done);
        }

        #endregion
    }
}
