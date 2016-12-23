using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using WEAK.Communication;

namespace WEAK.Test.Communication
{
    [TestClass]
    public class PublisherTest
    {
        #region Fields

        #endregion

        #region Methods

        [TestMethod]
        public void Subscribe_Should_throw_ArgumentNullException_When_action_is_null()
        {
            using (IPublisher publisher = new Publisher())
            {
                Action<object> action = null;

                Check
                    .ThatCode(() => publisher.Subscribe(action, ExecutionMode.Direct))
                    .Throws<ArgumentNullException>();
            }
        }

        [TestMethod]
        public void Subscribe_Should_throw_ObjectDisposedException_When_disposed()
        {
            using (IPublisher publisher = new Publisher())
            {
                Action<object> action = null;

                publisher.Dispose();

                Check
                    .ThatCode(() => publisher.Subscribe(action, ExecutionMode.Direct))
                    .Throws<ObjectDisposedException>();
            }
        }

        [TestMethod]
        public void Subscribe_Publish_Direct_Should_execute()
        {
            using (IPublisher publisher = new Publisher())
            {
                int threadId = 0;
                Action<object> action = o => threadId = Thread.CurrentThread.ManagedThreadId;

                publisher.Subscribe(action, ExecutionMode.Direct);

                publisher.Publish(new object());

                Check.That(Thread.CurrentThread.ManagedThreadId).IsEqualTo(threadId);
            }
        }

        [TestMethod]
        public void Subscribe_Publish_Async_Should_execute()
        {
            using (IPublisher publisher = new Publisher())
            using (ManualResetEvent waitHandle = new ManualResetEvent(false))
            {
                Action<object> action = o => { Thread.Sleep(100); waitHandle.Set(); };

                publisher.Subscribe(action, ExecutionMode.Async);

                publisher.Publish(new object());

                Check.That(waitHandle.WaitOne(1000)).IsTrue();
            }
        }

        [TestMethod]
        public void Subscribe_Publish_LongRunning_Should_execute()
        {
            using (IPublisher publisher = new Publisher())
            using (ManualResetEvent waitHandle = new ManualResetEvent(false))
            {
                Action<object> action = o => { Thread.Sleep(100); waitHandle.Set(); };

                publisher.Subscribe(action, ExecutionMode.LongRunning);

                publisher.Publish(new object());

                Check.That(waitHandle.WaitOne(1000)).IsTrue();
            }
        }

        [TestMethod]
        public void Subscribe_Publish_Context_Should_execute()
        {
            using (IPublisher publisher = new Publisher())
            using (ManualResetEvent waitHandle = new ManualResetEvent(false))
            {
                Action<object> action = o => { Thread.Sleep(100); waitHandle.Set(); };

                publisher.Subscribe(action, ExecutionMode.LongRunning);

                 publisher.Publish(new object());

                Check.That(waitHandle.WaitOne(1000)).IsTrue();
            }
        }






        [TestMethod, TestCategory("Performance")]
        public void GetTopic_Performance()
        {
            IPublisher publisher = new Publisher();

            Stopwatch same = new Stopwatch();
            Stopwatch random = new Stopwatch();

            Type topic;

            //for (int i = 0; i < 1000; ++i)
            //{
            //    string randomTopic = i.ToString();
            //    random.Start();
            //    topic = Publisher.RootTopic.GetTopicType(randomTopic, ".");
            //    random.Stop();
            //}

            //for (int i = 0; i < 1000000; ++i)
            //{
            //    same.Start();
            //    topic = Publisher.RootTopic.GetTopicType("same", ".");
            //    same.Stop();
            //}

            Console.WriteLine($"same: { (int)(1000000 / same.Elapsed.TotalSeconds) } / s");
            Console.WriteLine($"random: { (int)(1000 / random.Elapsed.TotalSeconds) } / ms");
        }

        #endregion
    }
}
