using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class CollectionUnDoTest
    {
        #region Methods

        [TestMethod]
        public void CollectionUnDoTestNull()
        {
            try
            {
                new CollectionUnDo<int>(null, 0, false);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }
        }

        [TestMethod]
        public void DoTest()
        {
            List<object> values = new List<object>();
            object value = new object();
            IUnDo undo = new CollectionUnDo<object>(values, value, true);

            undo.Do();

            Assert.IsTrue(values.Contains(value), "Value is wrong.");
        }

        [TestMethod]
        public void UndoTest()
        {
            List<object> values = new List<object>();
            object value = new object();
            values.Add(value);
            IUnDo undo = new CollectionUnDo<object>(values, value, true);

            undo.Undo();

            Assert.IsFalse(values.Contains(value), "Value is wrong.");
        }

        #endregion
    }
}
