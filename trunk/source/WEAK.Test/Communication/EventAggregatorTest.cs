using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WEAK.Communication;

namespace WEAK.Test.Communication
{
    [TestClass]
    public class EventAggregatorTest
    {
        #region Types

        private class Request
        { }

        private class Request2
        { }

        private class Request3
        { }

        private class Request4
        { }

        private class Request5
        { }

        private class Dummy
        {
            #region Fields

            public static Action Action;

            #endregion

            #region Methods

            [Subscribe(ExecutionMode.Direct)]
            private void PrivateStaticHook(Request3 request)
            {
                Action();
            }

            [Subscribe(ExecutionMode.Direct)]
            private void PrivateHook(Request2 request)
            {
                Action();
            }

            [Subscribe(ExecutionMode.Direct)]
            protected virtual void ProtectedHook2(Request4 request)
            { }

            [Subscribe(ExecutionMode.Direct)]
            protected virtual void ProtectedHook3(Request5 request)
            {
                Action();
            }

            protected virtual void ProtectedHook(Request request)
            {
                Action();
            }

            public void On(Request request)
            {
                Action();
            }

            public void On(object request)
            {
                Action();
            }

            public static void OnStatic(Request request)
            {
                Action();
            }

            #endregion
        }

        private class Dummy2 : Dummy
        {
            #region Dummy

            [Subscribe(ExecutionMode.Direct)]
            protected override void ProtectedHook(Request request)
            {
                base.ProtectedHook(request);
            }

            [Subscribe(ExecutionMode.Direct)]
            protected override void ProtectedHook3(Request5 request)
            {
                Action();
            }

            protected override void ProtectedHook2(Request4 request)
            {
                Action();
            }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public void SubscribeTestNull()
        {
            IPublisher publisher = new EventAggregator();

            try
            {
                publisher.Subscribe<object>(null, ExecutionMode.Direct);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }
            finally
            {
                publisher.Dispose();
            }
        }

        [TestMethod]
        public void SubscribeTestDispose()
        {
            IPublisher publisher = new EventAggregator();
            publisher.Dispose();

            try
            {
                publisher.Subscribe<object>(null, ExecutionMode.Direct);
                Assert.Fail("Did not raise ObjectDisposedException.");
            }
            catch (ObjectDisposedException) { }
        }

        [TestMethod]
        public void PublishTestDispose()
        {
            IPublisher publisher = new EventAggregator();
            publisher.Dispose();

            try
            {
                publisher.Publish<object>(null);
                Assert.Fail("Did not raise ObjectDisposedException.");
            }
            catch (ObjectDisposedException) { }
        }

        [TestMethod]
        public void SubscribeTestReference()
        {
            IPublisher publisher = new EventAggregator();
            bool done = false;
            Dummy dummy = new Dummy();
            Dummy.Action = () => done = true;
            WeakReference reference = new WeakReference(dummy);
            Request request = new Request();

            IDisposable register = publisher.Subscribe<Request>(dummy.On, ExecutionMode.Direct);

            publisher.Publish(request);

            Assert.IsTrue(done, "Method haven't executed.");

            done = false;

            dummy = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsFalse(reference.IsAlive, "Instance is still alive.");

            publisher.Publish(request);

            Assert.IsFalse(done, "Mathod have executed.");

            publisher.Dispose();
        }

        [TestMethod]
        public void PublishTestDirect()
        {
            using (IPublisher publisher = new EventAggregator())
            {
                bool done = false;
                Dummy dummy = new Dummy();
                Dummy.Action = () => done = true;

                IDisposable register = publisher.Subscribe<Request>(dummy.On, ExecutionMode.Direct);

                publisher.Publish(new Request());

                Assert.IsTrue(done, "Method haven't executed.");
            }
        }

        [TestMethod]
        public void PublishTestAsync()
        {
            using (IPublisher publisher = new EventAggregator())
            using (ManualResetEvent handle = new ManualResetEvent(false))
            {
                Dummy dummy = new Dummy();
                Dummy.Action = () => handle.Set();

                IDisposable register = publisher.Subscribe<Request>(dummy.On, ExecutionMode.Async);

                publisher.Publish(new Request());

                Assert.IsTrue(handle.WaitOne(1000), "Method haven't executed.");
            }
        }

        [TestMethod]
        public void PublishTestLongRunning()
        {
            using (IPublisher publisher = new EventAggregator())
            using (ManualResetEvent handle = new ManualResetEvent(false))
            {
                Dummy dummy = new Dummy();
                Dummy.Action = () => handle.Set();

                IDisposable register = publisher.Subscribe<Request>(dummy.On, ExecutionMode.LongRunning);

                publisher.Publish(new Request());

                Assert.IsTrue(handle.WaitOne(1000), "Method haven't executed.");
            }
        }

        [TestMethod]
        public void PublishTestContextSame()
        {
            using (IPublisher publisher = new EventAggregator())
            using (ManualResetEvent handle = new ManualResetEvent(false))
            {
                Dummy dummy = new Dummy();
                Dummy.Action = () => handle.Set();

                IDisposable register = publisher.Subscribe<Request>(dummy.On, ExecutionMode.Context);

                publisher.Publish(new Request());

                Assert.IsTrue(handle.WaitOne(1000), "Method haven't executed.");
            }
        }

        [TestMethod]
        public void PublishTestContextDifferent()
        {
            using (IPublisher publisher = new EventAggregator())
            using (ManualResetEvent handle = new ManualResetEvent(false))
            {
                Assert.IsTrue(Task.Factory.StartNew(() =>
                {
                    Dummy dummy = new Dummy();
                    Dummy.Action = () => handle.Set();

                    IDisposable register = publisher.Subscribe<Request>(dummy.On, ExecutionMode.Context);

                    publisher.Publish(new Request());

                    Assert.IsTrue(handle.WaitOne(1000), "Method haven't executed.");
                }).Wait(1000), "Task haven't finished.");
            }
        }

        [TestMethod]
        public void PublishTestContextAsync()
        {
            using (IPublisher publisher = new EventAggregator())
            using (ManualResetEvent handle = new ManualResetEvent(false))
            {
                Assert.IsTrue(Task.Factory.StartNew(() =>
                {
                    Dummy dummy = new Dummy();
                    Dummy.Action = () => handle.Set();

                    IDisposable register = publisher.Subscribe<Request>(dummy.On, ExecutionMode.ContextAsync);

                    publisher.Publish(new Request());

                    Assert.IsTrue(handle.WaitOne(1000), "Method haven't executed.");
                }).Wait(1000), "Task haven't finished.");
            }
        }

        [TestMethod]
        public void PublishTestDerived()
        {
            using (IPublisher publisher = new EventAggregator())
            {
                bool done = false;
                Dummy dummy = new Dummy();
                Dummy.Action = () => done = true;

                IDisposable register = publisher.Subscribe<object>(dummy.On, ExecutionMode.Direct);

                publisher.Publish(new Request());

                Assert.IsTrue(done, "Method haven't executed.");

                done = false;

                publisher.Publish(new Request2());

                Assert.IsTrue(done, "Method haven't executed.");

                done = false;

                publisher.Publish(new Request3());

                Assert.IsTrue(done, "Method haven't executed.");
            }
        }

        [TestMethod]
        public void PublishTestBase()
        {
            using (IPublisher publisher = new EventAggregator())
            {
                bool done = false;
                Dummy dummy = new Dummy();
                Dummy.Action = () => done = true;

                IDisposable register = publisher.Subscribe<Request>(dummy.On, ExecutionMode.Direct);

                publisher.Publish(new Request() as object);

                Assert.IsFalse(done, "Method have executed.");
            }
        }

        [TestMethod]
        public void UnsubscribeTestInstance()
        {
            using (IPublisher publisher = new EventAggregator())
            {
                bool done = false;
                Dummy dummy = new Dummy();
                Dummy.Action = () => done = true;

                using (IDisposable register = publisher.Subscribe<Request>(dummy.On, ExecutionMode.Direct))
                {
                    publisher.Publish(new Request());

                    Assert.IsTrue(done, "Method haven't executed.");
                }

                done = false;

                publisher.Publish(new Request());

                Assert.IsFalse(done, "Method have executed.");
            }
        }

        [TestMethod]
        public void UnsubscribeTestStatic()
        {
            using (IPublisher publisher = new EventAggregator())
            {
                bool done = false;
                Dummy.Action = () => done = true;

                using (IDisposable register = publisher.Subscribe<Request>(Dummy.OnStatic, ExecutionMode.Direct))
                {
                    publisher.Publish(new Request());

                    Assert.IsTrue(done, "Method haven't executed.");
                }

                done = false;

                publisher.Publish(new Request());

                Assert.IsFalse(done, "Method have executed.");
            }
        }

        [TestMethod]
        public void HookUpTest()
        {
            using (IPublisher publisher = new EventAggregator())
            {
                bool done = false;
                Dummy.Action = () => done = true;
                Dummy dummy = new Dummy2();

                IDisposable registers = publisher.Subscribe(dummy);

                publisher.Publish(new Request());

                Assert.IsTrue(done, "Method haven't executed.");

                done = false;

                publisher.Publish(new Request2());

                Assert.IsTrue(done, "Method haven't executed.");

                done = false;

                publisher.Publish(new Request3());

                Assert.IsTrue(done, "Method haven't executed.");

                done = false;

                publisher.Publish(new Request4());

                Assert.IsTrue(done, "Method haven't executed.");

                done = false;
                int i = 0;
                Dummy.Action = () => ++i;

                publisher.Publish(new Request5());

                Assert.AreEqual(i, 1, "Method have executed more than once.");

                done = false;
                Dummy.Action = () => done = true;

                registers.Dispose();

                publisher.Publish(new Request());

                Assert.IsFalse(done, "Method have executed.");

                publisher.Publish(new Request2());

                Assert.IsFalse(done, "Method have executed.");

                publisher.Publish(new Request3());

                Assert.IsFalse(done, "Method have executed.");

                publisher.Publish(new Request4());

                Assert.IsFalse(done, "Method have executed.");

                publisher.Publish(new Request5());

                Assert.IsFalse(done, "Method have executed.");
            }
        }

        [TestMethod]
        public void SubscribeTestOnce()
        {
            using (IPublisher publisher = new EventAggregator())
            {
                int i = 0;
                Dummy dummy = new Dummy();
                Dummy.Action = () => ++i;

                IDisposable register = publisher.Subscribe<Request>(dummy.On, ExecutionMode.Direct);
                IDisposable register2 = publisher.Subscribe<Request>(dummy.On, ExecutionMode.Direct);

                publisher.Publish(new Request());

                Assert.AreEqual(i, 1, "Method haven't executed only once.");
            }
        }

        [TestMethod]
        public void PublishTestDirectPerf()
        {
            using (IPublisher publisher1 = new EventAggregator())
            using (IPublisher publisher2 = new EventAggregator())
            using (IPublisher publisher = new EventAggregator())
            using (IPublisher publisher3 = new EventAggregator())
            using (IPublisher publisher4 = new EventAggregator())
            {
                int total = 1000000;
                int i = 0;
                Dummy dummy = new Dummy();
                Dummy.Action = () => ++i;

                IDisposable register = publisher.Subscribe<object>(dummy.On, ExecutionMode.Direct);

                Request request = new Request();

                while (i < total)
                {
                    publisher.Publish(request);
                }

                i = 0;
                Stopwatch watch = Stopwatch.StartNew();

                while (i < total)
                {
                    publisher.Publish(request);
                }
                watch.Stop();
                long pubTime = watch.ElapsedMilliseconds;

                i = 0;
                while (i < total)
                {
                    dummy.On(request);
                }

                i = 0;
                watch.Restart();

                while (i < total)
                {
                    dummy.On(request);
                }
                watch.Stop();

                double ratio = (double)pubTime / (double)watch.ElapsedMilliseconds;

                Trace.WriteLine(string.Format("Ratio is {0}", ratio));

                Assert.IsTrue(ratio < 9, string.Format("Ratio is {0}", ratio));

                register.Dispose();
            }
        }

        [TestMethod]
        public void PublishTestAsyncPerf()
        {
            int total = 1000000;
            using (IPublisher publisher = new EventAggregator())
            using (CountdownEvent countDown = new CountdownEvent(total))
            {
                int i = 0;
                Dummy dummy = new Dummy();
                Dummy.Action = () =>
                {
                    countDown.Signal();
                };

                using (IDisposable register = publisher.Subscribe<object>(dummy.On, ExecutionMode.Async))
                {
                    Request request = new Request();

                    for (i = 0; i < total; ++i)
                    {
                        publisher.Publish(request);
                    }

                    countDown.Wait();

                    countDown.Reset(total);
                    Stopwatch watch = Stopwatch.StartNew();

                    for (i = 0; i < total; ++i)
                    {
                        publisher.Publish(request);
                    }
                    countDown.Wait();
                    watch.Stop();
                    long pubTime = watch.ElapsedMilliseconds;

                    countDown.Reset(total);

                    for (i = 0; i < total; ++i)
                    {
                        Task.Factory.StartNew(() => dummy.On(request));
                    }
                    countDown.Wait();

                    countDown.Reset(total);
                    watch.Restart();

                    for (i = 0; i < total; ++i)
                    {
                        Task.Factory.StartNew(() => dummy.On(request));
                    }
                    countDown.Wait();
                    watch.Stop();

                    double ratio = (double)pubTime / (double)watch.ElapsedMilliseconds;

                    Trace.WriteLine(string.Format("Ratio is {0}", ratio));

                    Assert.IsTrue(ratio < 1.1, string.Format("Ratio is {0}", ratio));
                }
            }
        }

        [TestMethod]
        public void SubscriptionTest()
        {
            using (IPublisher publisher = new EventAggregator())
            {
                bool done = false;
                Dummy dummy = new Dummy();
                Dummy.Action = () => done = true;
                using (IDisposable subscription = publisher.Subscribe<object>(dummy.On, ExecutionMode.Direct))
                { }

                publisher.Publish(new object());

                Assert.IsFalse(done, "Method have executed.");
            }
        }

        [TestMethod]
        public void ExecutionOrderTest()
        {
            using (IPublisher publisher = new EventAggregator())
            {
                ManualResetEvent handle = new ManualResetEvent(false);

                using (IDisposable s1 = publisher.Subscribe<Dummy>(o => { if (!handle.WaitOne(1000)) Assert.Fail("Order not respected"); }, ExecutionMode.Direct))
                using (IDisposable s2 = publisher.Subscribe<object>(o => handle.Set(), ExecutionMode.Async))
                {
                    publisher.Publish(new Dummy2());
                }

                handle.Reset();

                using (IDisposable s1 = publisher.Subscribe<object>(o => { if (!handle.WaitOne(1000)) Assert.Fail("Order not respected"); }, ExecutionMode.Direct))
                using (IDisposable s2 = publisher.Subscribe<Dummy>(o => handle.Set(), ExecutionMode.Async))
                {
                    publisher.Publish(new Dummy2());
                }

                handle.Reset();

                using (IDisposable s1 = publisher.Subscribe<object>(o => { if (!handle.WaitOne(1000)) Assert.Fail("Order not respected"); }, ExecutionMode.Direct))
                using (IDisposable s2 = publisher.Subscribe<object>(o => handle.Set(), ExecutionMode.Async))
                {
                    publisher.Publish(new object());
                }

                handle.Reset();

                using (IDisposable s1 = publisher.Subscribe<object>(o => { if (!handle.WaitOne(1000)) Assert.Fail("Order not respected"); }, ExecutionMode.Direct))
                using (IDisposable s2 = publisher.Subscribe<object>(o => handle.Set(), ExecutionMode.Async))
                {
                    publisher.Publish(new object());
                }
            }
        }

        #endregion
    }
}
