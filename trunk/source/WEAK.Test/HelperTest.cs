using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WEAK.Test
{
    [TestClass]
    public class HelperTest
    {
        #region Types

        private class Dummy
        {
            #region Fields

            public object Field;

            #endregion

            #region Properties

            public object Property { get; set; }

            #endregion

            #region Initialisation

            public Dummy()
            {
                Field = null;
                Property = null;
            }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public void GetMemberNameTestVariable()
        {
            object arg = null;

            Assert.AreEqual(Helper.GetMemberName(() => arg), "arg");
        }

        [TestMethod]
        public void GetMemberNameTestField()
        {
            Dummy dummy = new Dummy();

            Assert.AreEqual(Helper.GetMemberName(() => dummy.Field), "Field");
        }

        [TestMethod]
        public void GetMemberNameTestProperty()
        {
            Dummy dummy = new Dummy();

            Assert.AreEqual(Helper.GetMemberName(() => dummy.Property), "Property");
        }

        [TestMethod]
        public void GetMemberNameTestNull()
        {
            try
            {
                Helper.GetMemberName<string>(null);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }
        }

        [TestMethod]
        public void GetMemberNameTestNotMemberExpression()
        {
            try
            {
                Helper.GetMemberName<Action>(() => GetMemberNameTestNotMemberExpression);
                Assert.Fail("Did not raise ArgumentException.");
            }
            catch (ArgumentException) { }
        }

        #endregion
    }
}
