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

        private class Dummy
        {
            #region Fields

            public static Action Action;

            #endregion

            #region Methods

            public void On(Request request)
            {
                Action();
            }

            public void On(IRequest request)
            {
                Action();
            }

            [AutoHookUp]
            private void PrivateHook(IRequest request)
            {
                Action();
            }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public void PublishTest()
        {
            IPublisher publisher = new EventAggregator();

            bool done = false;
            Dummy dummy = new Dummy();
            Dummy.Action = () => done = true;
            WeakReference reference = new WeakReference(dummy);

            publisher.Subscribe<IRequest>(dummy.On);

            publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

            Assert.IsTrue(done);

            done = false;

            publisher.Unsubscribe<IRequest>(dummy.On);

            publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

            Assert.IsFalse(done);

            done = false;
            publisher.HookUp(dummy);

            publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

            Assert.IsTrue(done);

            done = false;
            publisher.UnHookUp(dummy);

            publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

            Assert.IsTrue(!done);

            publisher.Subscribe<Request>(dummy.On);

            publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

            Assert.IsTrue(done);

            done = false;
            dummy = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsFalse(reference.IsAlive);

            publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

            Assert.IsFalse(done);

            publisher.Dispose();
        }

        [TestMethod]
        public void PerformanceDirect()
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

                Assert.IsTrue(ratio < 15, string.Format("shit is too slow, ratio is {0}", ratio));
            }
        }

        [TestMethod]
        public void PerformanceContext()
        {
            int total = 1000000;
            using (IPublisher publisher = new EventAggregator())
            {
                int i = 0;
                Dummy dummy = new Dummy();
                Dummy.Action = () => ++i;

                publisher.Subscribe<IRequest>(dummy.On);

                Request request = new Request { PulishingMode = RequestPublishingMode.Context };

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

                Assert.IsTrue(ratio < 15, string.Format("shit is too slow, ratio is {0}", ratio));
            }
        }

        [TestMethod]
        public void PerformanceAsync()
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

                Assert.IsTrue(ratio < 1.5, string.Format("shit is too slow, ratio is {0}", ratio));
            }
        }

        #endregion
    }
}
