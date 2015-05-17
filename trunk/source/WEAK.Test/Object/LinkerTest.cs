using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WEAK.Object;

namespace WEAK.Test.Object
{
    [TestClass]
    public class LinkerTest
    {
        #region Types

        private interface INotUsed
        { }

        private interface IDummy
        { }

        private interface IDummySingleton
        { }

        private abstract class ADummy
        { }

        private class Dummy : ADummy, IDummy, IDummySingleton
        { }

        #endregion

        #region Methods

        [ClassInitialize]
        public static void Initialise(TestContext context)
        {
            Linker<IDummy>.Register<Dummy>(false);
            Linker<IDummySingleton>.Register<Dummy>(true);
            Linker<ADummy>.Register<Dummy>(false);
        }

        [TestMethod]
        public void TryRegisterTestFail()
        {
            Assert.IsFalse(Linker<IDummy>.TryRegister<Dummy>(), "dual register");

            Assert.IsFalse(Linker<IDummy>.TryRegister<Dummy>(false), "dual register");

            try
            {
                Linker<Dummy>.TryRegister<Dummy>();
                Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }

            try
            {
                Linker<Dummy>.TryRegister<Dummy>(false);
                Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }
        }

        [TestMethod]
        public void RegisterTestFail()
        {
            try
            {
                Linker<IDummy>.Register<Dummy>();
                Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }

            try
            {
                Linker<Dummy>.Register<Dummy>();
                Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }
        }

        [TestMethod]
        public void ResolveTestFail()
        {
            try
            {
                Linker<Dummy>.Resolve();
                Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }

            try
            {
                Linker<INotUsed>.Resolve();
                Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }

            try
            {
                Linker<INotUsed>.Resolve("test");
                Assert.Fail("InvalidOperationException was not raised");
            }
            catch (InvalidOperationException)
            { }
        }

        [TestMethod]
        public void ResolveTestSingleton()
        {
            IDummySingleton instance = Linker<IDummySingleton>.Resolve();

            Assert.AreSame(instance, Linker<IDummySingleton>.Resolve(), "instances are not the same");

            Assert.AreSame(instance, Linker<IDummySingleton>.Resolve("test"), "instances are not the same");
        }

        [TestMethod]
        public void ResolveTestInterface()
        {
            IDummy instance = Linker<IDummy>.Resolve();

            Assert.AreNotSame(instance, Linker<IDummy>.Resolve(), "instances are the same");

            Assert.AreNotSame(instance, Linker<IDummy>.Resolve("test"), "instances are the same");
        }

        [TestMethod]
        public void ResolveTestAbstract()
        {
            ADummy instance = Linker<ADummy>.Resolve();

            Assert.AreNotSame(instance, Linker<ADummy>.Resolve(), "instances are the same");

            Assert.AreNotSame(instance, Linker<ADummy>.Resolve("test"), "instances are the same");
        }

        [TestMethod]
        public void ResolveTestKeyNullOrEmpty()
        {
            try
            {
                Linker<IDummy>.Resolve(null);
                Assert.Fail("ArgumentException was not raised");
            }
            catch (ArgumentException)
            { }

            try
            {
                Linker<IDummy>.Resolve(string.Empty);
                Assert.Fail("ArgumentException was not raised");
            }
            catch (ArgumentException)
            { }
        }

        [TestMethod]
        public void ResolveTestKey()
        {
            ADummy instance = Linker<ADummy>.Resolve("test");

            Assert.AreSame(instance, Linker<ADummy>.Resolve("test"), "instances are not the same");

            Assert.AreNotSame(instance, Linker<ADummy>.Resolve("test2"), "instances are the same");
        }

        [TestMethod]
        public void ResolveTestReference()
        {
            ADummy instance = Linker<ADummy>.Resolve("test");

            WeakReference reference = new WeakReference(instance);

            instance = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsFalse(reference.IsAlive, "instance is still alive");
        }

        #endregion
    }
}
