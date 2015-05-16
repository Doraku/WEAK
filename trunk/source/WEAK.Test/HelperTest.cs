using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public void GetMemberNameTest()
        {
            object arg = null;

            Assert.AreEqual(Helper.GetMemberName(() => arg), "arg");

            Dummy dummy = new Dummy();

            Assert.AreEqual(Helper.GetMemberName(() => dummy.Field), "Field");
            Assert.AreEqual(Helper.GetMemberName(() => dummy.Property), "Property");
        }

        #endregion
    }
}
