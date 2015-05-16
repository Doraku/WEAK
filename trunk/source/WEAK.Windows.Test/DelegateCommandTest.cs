using Microsoft.VisualStudio.TestTools.UnitTesting;
using WEAK.Windows;

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

            if (command.CanExecute(null))
            {
                command.Execute(null);
            }

            Assert.IsTrue(done);

            done = false;
            command = new DelegateCommand(Assert.Fail, () => false);

            if (command.CanExecute(null))
            {
                command.Execute(null);
            }

            done = false;
            command = new DelegateCommand(() => done = true, () => true);

            if (command.CanExecute(null))
            {
                command.Execute(null);
            }

            Assert.IsTrue(done);
        }

        [TestMethod]
        public void GenericClassTest()
        {
            bool done = false;
            DelegateCommand<Dummy> command = new DelegateCommand<Dummy>(p => Assert.Fail());
            Dummy parameter = null;

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }

            done = false;
            command = new DelegateCommand<Dummy>(p => done = true);
            parameter = new Dummy();

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }

            Assert.IsTrue(done);

            done = false;
            command = new DelegateCommand<Dummy>(p => Assert.Fail(), p => false);
            parameter = new Dummy();

            if (command.CanExecute(null))
            {
                command.Execute(null);
            }

            done = false;
            command = new DelegateCommand<Dummy>(p => done = true, p => true);
            parameter = new Dummy();

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }

            Assert.IsTrue(done);

            done = false;
            command = new DelegateCommand<Dummy>(p => Assert.Fail(), p => true);

            if (command.CanExecute(this))
            {
                command.Execute(this);
            }
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

            done = false;
            command = new DelegateCommand<int>(p => Assert.Fail(), p => true);

            if (command.CanExecute(this))
            {
                command.Execute(this);
            }
        }

        #endregion
    }
}
