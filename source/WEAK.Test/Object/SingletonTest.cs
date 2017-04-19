using System;
using WEAK.Object;
using Xunit;

namespace WEAK.Test.Object
{
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

        [Fact]
        public void SingletonTestValid()
        {
            Dummy instance = Singleton<Dummy>.Instance;

            //Assert.IsNotNull(instance, "instance is null");

            //Assert.Equals(instance, Singleton<Dummy>.Instance, "instances are different");
        }

        [Fact]
        public void SingletonTestInvalid()
        {
            try
            {
                DummyFail instance = Singleton<DummyFail>.Instance;
                //Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }
        }

        #endregion
    }
}
