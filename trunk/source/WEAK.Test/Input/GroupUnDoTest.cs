using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class GroupUnDoTest
    {
        #region Methods

        [TestMethod]
        public void GroupUnDoTestNull()
        {
            try
            {
                new GroupUnDo(null);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }

            try
            {
                new GroupUnDo(new IUnDo[] { null });
                Assert.Fail("Did not raise ArgumentException.");
            }
            catch (ArgumentException) { }
        }

        [TestMethod]
        public void DoTest()
        {
            List<object> values = new List<object>();
            object value = new object();
            object otherValue = new object();
            IUnDo undo = new GroupUnDo(
                new ListUnDo<object>(values, 0, value, true),
                new ValueUnDo<object>((v) => values[0] = v, value, otherValue));

            undo.Do();

            Assert.AreSame(values[0], otherValue, "Value is wrong.");
        }

        [TestMethod]
        public void UndoTest()
        {
            List<object> values = new List<object>();
            object value = new object();
            object otherValue = new object();
            values.Add(otherValue);
            IUnDo undo = new GroupUnDo(
                new ListUnDo<object>(values, 0, value, true),
                new ValueUnDo<object>((v) => values[0] = v, value, otherValue));

            undo.Undo();

            Assert.IsFalse(values.Contains(value) || values.Contains(otherValue), "Value is wrong.");
        }

        #endregion
    }
}
