using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class DictionaryUnDoTest
    {
        #region Methods

        [TestMethod]
        public void DictionaryUnDoTestNull()
        {
            try
            {
                new DictionaryUnDo<object, int>(null, new object(), 0, false);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }

            try
            {
                new DictionaryUnDo<object, int>(new Dictionary<object, int>(), null, 0, false);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }
        }

        [TestMethod]
        public void DoTest()
        {
            Dictionary<int, object> values = new Dictionary<int, object>();
            object value = new object();
            IUnDo undo = new DictionaryUnDo<int, object>(values, 42, value, true);

            undo.Do();

            Assert.AreSame(values[42], value, "Value is wrong.");
        }

        [TestMethod]
        public void UndoTest()
        {
            Dictionary<int, object> values = new Dictionary<int, object>();
            object value = new object();
            values.Add(42, value);
            IUnDo undo = new DictionaryUnDo<int, object>(values, 42, value, true);

            undo.Undo();

            Assert.IsFalse(values.ContainsKey(42) || values.ContainsValue(value), "Value is wrong.");
        }

        #endregion
    }
}
