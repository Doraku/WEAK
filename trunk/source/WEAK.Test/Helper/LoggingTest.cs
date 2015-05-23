using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WEAK.Helper;

namespace WEAK.Test.Helper
{
    [TestClass]
    public class LoggingTest
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

            Assert.AreEqual(Logging.GetMemberName(() => arg), "arg");
        }

        [TestMethod]
        public void GetMemberNameTestField()
        {
            Dummy dummy = new Dummy();

            Assert.AreEqual(Logging.GetMemberName(() => dummy.Field), "Field");
        }

        [TestMethod]
        public void GetMemberNameTestProperty()
        {
            Dummy dummy = new Dummy();

            Assert.AreEqual(Logging.GetMemberName(() => dummy.Property), "Property");
        }

        [TestMethod]
        public void GetMemberNameTestNull()
        {
            try
            {
                Logging.GetMemberName<string>(null);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }
        }

        [TestMethod]
        public void GetMemberNameTestNotMemberExpression()
        {
            try
            {
                Logging.GetMemberName<Action>(() => GetMemberNameTestNotMemberExpression);
                Assert.Fail("Did not raise ArgumentException.");
            }
            catch (ArgumentException) { }
        }

        #endregion
    }
}
