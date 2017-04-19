using System;
using WEAK.Object;
using Xunit;

namespace WEAK.Test.Object
{
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

        [Fact]
        public void GetTestNull()
        {
            try
            {
                ReferenceManager<string, Dummy>.Get(null);
                //Assert.Fail("ArgumentNullException was not raised");
            }
            catch (ArgumentNullException)
            { }
        }

        [Fact]
        public void GetOrCreateTestNull()
        {
            try
            {
                ReferenceManager<string, Dummy>.GetOrCreate(null);
                //Assert.Fail("ArgumentNullException was not raised");
            }
            catch (ArgumentNullException)
            { }

            try
            {
                ReferenceManager<string, Dummy>.GetOrCreate(string.Empty, null);
                //Assert.Fail("ArgumentNullException was not raised");
            }
            catch (ArgumentNullException)
            { }
        }

        [Fact]
        public void ReleaseTestNull()
        {
            try
            {
                ReferenceManager<string, Dummy>.Release(null);
                //Assert.Fail("ArgumentNullException was not raised");
            }
            catch (ArgumentNullException)
            { }
        }

        [Fact]
        public void TestReference()
        {
            Dummy instance = ReferenceManager<string, Dummy>.GetOrCreate("TestReference");
            WeakReference reference = new WeakReference(instance);

            instance = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            //Assert.IsFalse(reference.IsAlive, "instance still alive");
        }

        [Fact]
        public void GetOrCreateTest()
        {
            Dummy instance = ReferenceManager<string, Dummy>.GetOrCreate("GetOrCreateTest");
            WeakReference reference = new WeakReference(instance);

            //Assert.IsNotNull(instance, "instance is null");
            //Assert.AreSame(instance, ReferenceManager<string, Dummy>.GetOrCreate("GetOrCreateTest"));

            instance = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            instance = ReferenceManager<string, Dummy>.GetOrCreate("GetOrCreateTest");

            //Assert.AreNotSame(instance, reference.Target, "instances are the same");
        }

        [Fact]
        public void GetOrCreateTestCreator()
        {
            Dummy instance = ReferenceManager<string, Dummy>.GetOrCreate("GetOrCreateTestCreator", (k) => new Dummy(k));

            //Assert.IsNotNull(instance, "instance is null");
            //Assert.AreEqual(instance.Field, "GetOrCreateTestCreator");
        }

        [Fact]
        public void GetTest()
        {
            Dummy instance = ReferenceManager<string, Dummy>.Get("GetTest");

            //Assert.IsNull(instance, "instance is not null");

            instance = ReferenceManager<string, Dummy>.GetOrCreate("GetTest");

            //Assert.AreSame(instance, ReferenceManager<string, Dummy>.Get("GetTest"));
        }

        [Fact]
        public void ReleaseTest()
        {
            Dummy instance = ReferenceManager<string, Dummy>.GetOrCreate("ReleaseTest");

            //Assert.IsNotNull(instance, "instance is null");

            //Assert.AreSame(instance, ReferenceManager<string, Dummy>.Release("ReleaseTest"), "instances are not the same");

            //Assert.IsNull(ReferenceManager<string, Dummy>.Get("ReleaseTest"));
        }

        #endregion
    }
}
