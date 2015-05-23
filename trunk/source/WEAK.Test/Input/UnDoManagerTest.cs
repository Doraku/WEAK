using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class UnDoManagerTest
    {
        #region Methods

        [TestMethod]
        public void DoTestNull()
        {
            UnDoManager manager = new UnDoManager();
            try
            {
                manager.Do(null);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }
        }

        [TestMethod]
        public void DoTest()
        {
            UnDoManager manager = new UnDoManager();
            int value = 0;

            manager.Do((v) => value = v, value, 42);

            Assert.AreEqual(value, 42, "Wrong value.");

            Assert.IsTrue(manager.CanUndo(), "wrong state.");

            Assert.IsFalse(manager.CanRedo(), "wrong state.");

            manager.Do((v) => value = v, value, 1337);

            manager.Undo();

            Assert.AreEqual(value, 42, "Wrong value.");

            Assert.IsTrue(manager.CanRedo(), "wrong state.");

            manager.Redo();

            Assert.AreEqual(value, 1337, "Wrong value.");

            Assert.IsTrue(manager.CanUndo(), "wrong state.");

            Assert.IsFalse(manager.CanRedo(), "wrong state.");

            manager.UndoAll();

            Assert.AreEqual(value, 0, "Wrong value.");

            Assert.IsFalse(manager.CanUndo(), "wrong state.");

            Assert.IsTrue(manager.CanRedo(), "wrong state.");

            manager.RedoAll();

            Assert.AreEqual(value, 1337, "Wrong value.");

            Assert.IsTrue(manager.CanUndo(), "wrong state.");

            Assert.IsFalse(manager.CanRedo(), "wrong state.");

            manager.Clear();

            Assert.IsFalse(manager.CanUndo(), "wrong state.");

            Assert.IsFalse(manager.CanRedo(), "wrong state.");
        }

        [TestMethod]
        public void GroupDoTest()
        {
            UnDoManager manager = new UnDoManager();
            int value = 0;

            using (IDisposable handle = manager.BeginGroup())
            {
                manager.Do((v) => value = v, value, 42);

                using (IDisposable handle2 = manager.BeginGroup())
                {
                    manager.Do((v) => value = v, value, 1337);
                }

                Assert.IsFalse(manager.CanUndo(), "wrong state.");
                Assert.IsFalse(manager.CanRedo(), "wrong state.");
            }

            Assert.IsTrue(manager.CanUndo(), "wrong state.");
            Assert.IsFalse(manager.CanRedo(), "wrong state.");

            manager.Undo();

            Assert.AreEqual(value, 0, "Wrong value.");
        }

        #endregion
    }
}
