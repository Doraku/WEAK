using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

        private class Request : IRequest
        {
            #region IRequest

            public RequestPublishingMode PulishingMode { get; set; }

            #endregion
        }

        private class Request2 : IRequest
        {
            #region IRequest

            public RequestPublishingMode PulishingMode
            {
                get { return RequestPublishingMode.Direct; }
            }

            #endregion
        }

        private class Request3 : IRequest
        {
            #region IRequest

            public RequestPublishingMode PulishingMode
            {
                get { return RequestPublishingMode.Direct; }
            }

            #endregion
        }

        private class Request4 : IRequest
        {
            #region IRequest

            public RequestPublishingMode PulishingMode
            {
                get { return RequestPublishingMode.Direct; }
            }

            #endregion
        }

        private class Request5 : IRequest
        {
            #region IRequest

            public RequestPublishingMode PulishingMode
            {
                get { return RequestPublishingMode.Direct; }
            }

            #endregion
        }

        private class Dummy
        {
            #region Fields

            public static Action Action;

            #endregion

            #region Methods

            [AutoHookUp]
            private void PrivateStaticHook(Request3 request)
            {
                Action();
            }

            [AutoHookUp]
            private void PrivateHook(Request2 request)
            {
                Action();
            }

            [AutoHookUp]
            protected virtual void ProtectedHook2(Request4 request)
            { }

            [AutoHookUp]
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

            public void On(IRequest request)
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

            [AutoHookUp]
            protected override void ProtectedHook(Request request)
            {
                base.ProtectedHook(request);
            }

            [AutoHookUp]
            protected virtual void ProtectedHook3(Request5 request)
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
                publisher.Subscribe<IRequest>(null);
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
                publisher.Subscribe<IRequest>(null);
                Assert.Fail("Did not raise ObjectDisposedException.");
            }
            catch (ObjectDisposedException) { }
        }

        [TestMethod]
        public void UnsubscribeTestNull()
        {
            IPublisher publisher = new EventAggregator();

            try
            {
                publisher.Unsubscribe<IRequest>(null);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }
            finally
            {
                publisher.Dispose();
            }
        }

        [TestMethod]
        public void UnsubscribeTestDispose()
        {
            IPublisher publisher = new EventAggregator();
            publisher.Dispose();

            try
            {
                publisher.Unsubscribe<IRequest>(null);
                Assert.Fail("Did not raise ObjectDisposedException.");
            }
            catch (ObjectDisposedException) { }
        }

        [TestMethod]
        public void PublishTestNull()
        {
            IPublisher publisher = new EventAggregator();

            try
            {
                publisher.Publish<IRequest>(null);
                Assert.Fail("Did not raise ArgumentNullException.");
            }
            catch (ArgumentNullException) { }
            finally
            {
                publisher.Dispose();
            }
        }

        [TestMethod]
        public void PublishTestDispose()
        {
            IPublisher publisher = new EventAggregator();
            publisher.Dispose();

            try
            {
                publisher.Publish<IRequest>(null);
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
            Request request = new Request { PulishingMode = RequestPublishingMode.Direct };

            publisher.Subscribe<Request>(dummy.On);

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

                publisher.Subscribe<Request>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

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

                publisher.Subscribe<Request>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Async });

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

                publisher.Subscribe<Request>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.LongRunning });

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

                publisher.Subscribe<Request>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Context });

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

                    publisher.Subscribe<Request>(dummy.On);

                    publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Context });

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

                publisher.Subscribe<IRequest>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

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

                publisher.Subscribe<Request>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct } as IRequest);

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

                publisher.Subscribe<Request>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

                Assert.IsTrue(done, "Method haven't executed.");

                publisher.Unsubscribe<Request>(dummy.On);
                done = false;

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

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

                publisher.Subscribe<Request>(Dummy.OnStatic);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

                Assert.IsTrue(done, "Method haven't executed.");

                publisher.Unsubscribe<Request>(Dummy.OnStatic);
                done = false;

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

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

                publisher.HookUp(dummy);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

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

                publisher.UnHookUp(dummy);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

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

                publisher.Subscribe<Request>(dummy.On);
                publisher.Subscribe<Request>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

                Assert.AreEqual(i, 1, "Method haven't executed only once.");
            }
        }

        [TestMethod]
        public void PublishTestDirectPerf()
        {
            using (IPublisher publisher = new EventAggregator())
            {
                int total = 1000000;
                int i = 0;
                Dummy dummy = new Dummy();
                Dummy.Action = () => ++i;

                publisher.Subscribe<IRequest>(dummy.On);

                Request request = new Request { PulishingMode = RequestPublishingMode.Direct };

                Stopwatch watch = Stopwatch.StartNew();

                while (i < total)
                {
                    publisher.Publish(request);
                }
                watch.Stop();
                long pubTime = watch.ElapsedMilliseconds;

                i = 0;
                watch.Restart();

                while (i < total)
                {
                    dummy.On(request);
                }
                watch.Stop();

                double ratio = (double)pubTime / (double)watch.ElapsedMilliseconds;

                Trace.WriteLine(string.Format("Ratio is {0}", ratio));

                Assert.IsTrue(ratio < 12, string.Format("Ratio is {0}", ratio));
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
                Dummy.Action = () => countDown.Signal();

                publisher.Subscribe<IRequest>(dummy.On);

                Request request = new Request { PulishingMode = RequestPublishingMode.Async };

                Stopwatch watch = Stopwatch.StartNew();

                for (i = 0; i < total; ++i)
                {
                    publisher.Publish(request);
                }
                countDown.Wait();
                watch.Stop();
                long pubTime = watch.ElapsedMilliseconds;

                countDown.Reset();
                watch.Restart();

                for (i = 0; i < total; ++i)
                {
                    Task.Factory.StartNew(() => dummy.On(request));
                }
                countDown.Wait();
                watch.Stop();

                double ratio = (double)pubTime / (double)watch.ElapsedMilliseconds;

                Trace.WriteLine(string.Format("Ratio is {0}", ratio));

                Assert.IsTrue(ratio < 1.2, string.Format("Ratio is {0}", ratio));
            }
        }

        #endregion
    }
}
