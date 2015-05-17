using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEAK.Object;

namespace WEAK.Test.Object
{
    [TestClass]
    public class ReferenceManagerTest
    {
        #region Types

        private class Dummy
        {
            #region Fields

            public string Field;

            #endregion

            #region Initialisation

            public Dummy(string field)
            {
                Field = field;
            }

            public Dummy()
                : this(string.Empty)
            { }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public void GetTestNull()
        {
            try
            {
                ReferenceManager<string, Dummy>.Get(null);
                Assert.Fail("ArgumentNullException was not raised");
            }
            catch (ArgumentNullException)
            { }
        }

        [TestMethod]
        public void GetOrCreateTestNull()
        {
            try
            {
                ReferenceManager<string, Dummy>.GetOrCreate(null);
                Assert.Fail("ArgumentNullException was not raised");
            }
            catch (ArgumentNullException)
            { }

            try
            {
                ReferenceManager<string, Dummy>.GetOrCreate(string.Empty, null);
                Assert.Fail("ArgumentNullException was not raised");
            }
            catch (ArgumentNullException)
            { }
        }

        [TestMethod]
        public void ReleaseTestNull()
        {
            try
            {
                ReferenceManager<string, Dummy>.Release(null);
                Assert.Fail("ArgumentNullException was not raised");
            }
            catch (ArgumentNullException)
            { }
        }

        [TestMethod]
        public void TestReference()
        {
            Dummy instance = ReferenceManager<string, Dummy>.GetOrCreate("TestReference");
            WeakReference reference = new WeakReference(instance);

            instance = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsFalse(reference.IsAlive, "instance still alive");
        }

        [TestMethod]
        public void GetOrCreateTest()
        {
            Dummy instance = ReferenceManager<string, Dummy>.GetOrCreate("GetOrCreateTest");
            WeakReference reference = new WeakReference(instance);

            Assert.IsNotNull(instance, "instance is null");
            Assert.AreSame(instance, ReferenceManager<string, Dummy>.GetOrCreate("GetOrCreateTest"));

            instance = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            instance = ReferenceManager<string, Dummy>.GetOrCreate("GetOrCreateTest");

            Assert.AreNotSame(instance, reference.Target, "instances are the same");
        }

        [TestMethod]
        public void GetOrCreateTestCreator()
        {
            Dummy instance = ReferenceManager<string, Dummy>.GetOrCreate("GetOrCreateTestCreator", (k) => new Dummy(k));

            Assert.IsNotNull(instance, "instance is null");
            Assert.AreEqual(instance.Field, "GetOrCreateTestCreator");
        }

        [TestMethod]
        public void GetTest()
        {
            Dummy instance = ReferenceManager<string, Dummy>.Get("GetTest");

            Assert.IsNull(instance, "instance is not null");

            instance = ReferenceManager<string, Dummy>.GetOrCreate("GetTest");

            Assert.AreSame(instance, ReferenceManager<string, Dummy>.Get("GetTest"));
        }

        [TestMethod]
        public void ReleaseTest()
        {
            Dummy instance = ReferenceManager<string, Dummy>.GetOrCreate("ReleaseTest");

            Assert.IsNotNull(instance, "instance is null");

            Assert.AreSame(instance, ReferenceManager<string, Dummy>.Release("ReleaseTest"), "instances are not the same");

            Assert.IsNull(ReferenceManager<string, Dummy>.Get("ReleaseTest"));
        }

        #endregion
    }
}
