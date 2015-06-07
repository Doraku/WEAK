using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WEAK.Object;

namespace WEAK.Test.Object
{
    [TestClass]
    public class FactoryTest
    {
        #region Types

        private interface IDummy
        { }

        private abstract class ADummy
        { }

        private class Dummy : ADummy, IDummy
        {
            #region Initialisation

            public Dummy()
            {
                Thread.Sleep(10);
            }

            #endregion
        }

        private class DummyFail
        {
            #region Initialisation

            private DummyFail()
            { }

            #endregion
        }

        private class DummyCompositeSubFail
        {
            #region Initialisation

            public DummyCompositeSubFail(DummyCompositeFail fail)
            { }

            #endregion
        }

        private class DummyCompositeFail
        {
            #region Initialisation

            public DummyCompositeFail(DummyCompositeSubFail fail)
            { }

            #endregion
        }

        private class DummyComposite
        {
            #region Fields

            public readonly Dummy O1;
            public readonly IDummy O2;
            public readonly ADummy O3;

            #endregion

            #region Initialisation

            public DummyComposite(Dummy o1, IDummy o2, ADummy o3)
            {
                O1 = o1;
                O2 = o2;
                O3 = o3;
            }

            #endregion
        }

        private class DummyWithKey
        {
            #region Fields

            public readonly IDummy Param;
            public readonly object Param2;

            #endregion

            public DummyWithKey([Key("test")]IDummy param, [Key("test2")]object param2)
            {
                Param = param;
                Param2 = param2;
            }
        }

        #endregion

        #region Methods

        [ClassInitialize]
        public static void Initialise(TestContext context)
        {
            Linker<IDummy>.Register<Dummy>();
            Linker<ADummy>.Register<Dummy>();
        }

        [TestMethod]
        public void CreateInstanceTest()
        {
            Dummy instance = Factory<Dummy>.CreateInstance();

            Assert.IsNotNull(instance, "instance is null");

            Assert.AreNotSame(instance, Factory<Dummy>.CreateInstance(), "instance are the same");
        }

        [TestMethod]
        public void FactoryTestInvalid()
        {
            try
            {
                DummyFail instance = Factory<DummyFail>.CreateInstance();
                Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }
        }

        [TestMethod]
        public void CreateInstanceTestComposite()
        {
            DummyComposite instance = Factory<DummyComposite>.CreateInstance();

            Assert.IsNotNull(instance, "instance is null");
            Assert.IsNotNull(instance.O1, "instance is null");
            Assert.IsNotNull(instance.O2, "instance is null");
            Assert.IsNotNull(instance.O3, "instance is null");
        }

        [TestMethod]
        public void CreateInstanceTestCyclic()
        {
            try
            {
                DummyCompositeFail instance = Factory<DummyCompositeFail>.CreateInstance();
                Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }
        }

        [TestMethod]
        public void CreateInstanceTestCompositeThreadSafe()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 1000; ++i)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    DummyComposite instance = Factory<DummyComposite>.CreateInstance();

                    Assert.IsNotNull(instance, "instance is null");
                    Assert.IsNotNull(instance.O1, "instance is null");
                    Assert.IsNotNull(instance.O2, "instance is null");
                    Assert.IsNotNull(instance.O3, "instance is null");
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }

        [TestMethod]
        public void CreateInstanceTestInterface()
        {
            IDummy instance = Factory<IDummy>.CreateInstance();

            Assert.IsNotNull(instance, "instance is null");
        }

        [TestMethod]
        public void CreateInstanceWithKey()
        {
            DummyWithKey instance1 = Factory<DummyWithKey>.CreateInstance();
            DummyWithKey instance2 = Factory<DummyWithKey>.CreateInstance();

            Assert.AreNotSame(instance1, instance2, "same object");
            Assert.AreSame(instance1.Param, instance2.Param, "different object");
            Assert.AreSame(instance1.Param2, instance2.Param2, "different object");
        }

        #endregion
    }
}
