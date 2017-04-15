using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WEAK.Object;

namespace WEAK.Test.Object
{
    [TestClass]
    public class SingletonTest
    {
        #region Types

        private class Dummy
        { }

        private class DummyFail
        {
            #region Initialisation

            private DummyFail()
            { }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public void SingletonTestValid()
        {
            Dummy instance = Singleton<Dummy>.Instance;

            Assert.IsNotNull(instance, "instance is null");

            Assert.AreSame(instance, Singleton<Dummy>.Instance, "instances are different");
        }

        [TestMethod]
        public void SingletonTestInvalid()
        {
            try
            {
                DummyFail instance = Singleton<DummyFail>.Instance;
                Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }
        }

        #endregion
    }
}
