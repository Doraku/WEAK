using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class ListUnDoTest
    {
        #region Methods

        [TestMethod]
        public void ListUnDoTestNull()
        {
            try
            {
                new ListUnDo<int>(null, 0, 0, false);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }
        }

        [TestMethod]
        public void DoTest()
        {
            List<object> values = new List<object>();
            object value = new object();
            IUnDo undo = new ListUnDo<object>(values, 0, value, true);

            undo.Do();

            Assert.AreSame(values[0], value, "Value is wrong.");
        }

        [TestMethod]
        public void UndoTest()
        {
            List<object> values = new List<object>();
            object value = new object();
            values.Add(value);
            IUnDo undo = new ListUnDo<object>(values, 0, value, true);

            undo.Undo();

            Assert.IsFalse(values.Contains(value), "Value is wrong.");
        }

        #endregion
    }
}
