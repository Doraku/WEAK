using Microsoft.VisualStudio.TestTools.UnitTesting;
using WEAK.Supervision;

namespace WEAK.Test.Supervision
{
    [TestClass]
    public class LinkerTest
    {
        #region Types

        private interface IDummy
        { }

        private class Dummy : IDummy
        { }

        #endregion

        #region Methods

        [TestMethod]
        public void LinkNoSingleton()
        {
            Linker<IDummy>.Register<Dummy>(false);

            IDummy instance1 = Linker<IDummy>.Resolve("test");

            Assert.IsNotNull(instance1);

            Assert.AreEqual(instance1, Linker<IDummy>.Resolve("test"));
        }

        #endregion
    }
}
