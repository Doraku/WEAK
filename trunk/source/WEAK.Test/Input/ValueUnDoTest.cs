using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class ValueUnDoTest
    {
        #region Methods

        [TestMethod]
        public void ValueUnDoTestNull()
        {
            try
            {
                new ValueUnDo<int>(null, 0, 0);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }
        }

        [TestMethod]
        public void DoTest()
        {
            int value = 0;
            IUnDo undo = new ValueUnDo<int>(v => value = v, 42, 1337);

            undo.Do();

            Assert.AreEqual(value, 1337, "Value is wrong.");
        }

        [TestMethod]
        public void UndoTest()
        {
            int value = 0;
            IUnDo undo = new ValueUnDo<int>(v => value = v, 42, 1337);

            undo.Undo();

            Assert.AreEqual(value, 42, "Value is wrong.");
        }

        #endregion
    }
}
