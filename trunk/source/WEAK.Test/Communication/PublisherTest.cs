﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using NSubstitute;
using WEAK.Communication;

namespace WEAK.Test.Communication
{
    [TestClass]
    public class PublisherTest
    {
        #region Types

        private class Dummy
        {
            public static int StaticCallCount = 0;
            public int CallCount = 0;

            public static void StaticDo(object o)
            {
                ++StaticCallCount;
            }

            public void Do(object o)
            {
                ++CallCount;
            }
        }

        private class DummyDerived : Dummy
        { }

        #endregion

        #region Methods

        [TestMethod]
        public void Subscribe_Should_throw_ArgumentNullException_When_action_is_null()
        {
            using (IPublisher publisher = new Publisher())
            {
                Action<object> action = null;

                Check
                    .ThatCode(() => publisher.Subscribe(action, ExecutionOption.None))
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
                    .ThatCode(() => publisher.Subscribe(action, ExecutionOption.None))
                    .Throws<ObjectDisposedException>();
            }
        }

        [TestMethod]
        public void Publish_Should_throw_ObjectDisposedException_When_disposed()
        {
            using (IPublisher publisher = new Publisher())
            {
                publisher.Dispose();

                Check
                    .ThatCode(() => publisher.Publish(new object()))
                    .Throws<ObjectDisposedException>();
            }
        }

        [TestMethod]
        public void Subscribe_Publish_Should_execute()
        {
            using (IPublisher publisher = new Publisher())
            {
                int threadId = 0;
                Action<object> action = o => threadId = Thread.CurrentThread.ManagedThreadId;

                publisher.Subscribe(action, ExecutionOption.None);

                publisher.Publish(new object());

                Check.That(Thread.CurrentThread.ManagedThreadId).IsEqualTo(threadId);
            }
        }

        [TestMethod]
        public void Subscribe_Publish_Should_not_execute()
        {
            using (IPublisher publisher = new Publisher())
            {
                bool done = false;
                Action<int> action = o => done = true;

                publisher.Subscribe(action, ExecutionOption.None);

                publisher.Publish(new object());

                Check.That(done).IsFalse();

                action(0);
            }
        }

        [TestMethod]
        public void Subscribe_Publish_static_Should_execute()
        {
            using (IPublisher publisher = new Publisher())
            {
                publisher.Subscribe<object>(Dummy.StaticDo, ExecutionOption.None);

                publisher.Publish(new object());

                Check.That(Dummy.StaticCallCount).IsEqualTo(1);
            }
        }

        [TestMethod]
        public void Subscribe_Publish_Async_Should_execute()
        {
            using (IPublisher publisher = new Publisher())
            using (ManualResetEvent waitHandle = new ManualResetEvent(false))
            {
                Action<object> action = o => { Thread.Sleep(100); waitHandle.Set(); };

                publisher.Subscribe(action, ExecutionOption.Async);

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

                publisher.Subscribe(action, ExecutionOption.LongRunning);

                publisher.Publish(new object());

                Check.That(waitHandle.WaitOne(1000)).IsTrue();
            }
        }

        [TestMethod]
        public void Subscribe_Publish_Context_Should_execute()
        {
            SynchronizationContext context = Substitute.For<SynchronizationContext>();
            bool called = false;

            context
                .When(c => c.Send(Arg.Any<SendOrPostCallback>(), Arg.Any<object>()))
                .Do(_ => called = true);

            using (IPublisher publisher = new Publisher(context))
            {
                Action<object> action = o => { };

                publisher.Subscribe(action, ExecutionOption.Context);

                publisher.Publish(new object());

                Check.That(called).IsTrue();
            }
        }

        [TestMethod]
        public void Subscribe_Publish_Context_Aync_Should_execute()
        {
            SynchronizationContext context = Substitute.For<SynchronizationContext>();
            bool called = false;

            context
                .When(c => c.Post(Arg.Any<SendOrPostCallback>(), Arg.Any<object>()))
                .Do(_ => called = true);

            using (IPublisher publisher = new Publisher(context))
            {
                Action<object> action = o => { };

                publisher.Subscribe(action, ExecutionOption.Context | ExecutionOption.Async);

                publisher.Publish(new object());

                Check.That(called).IsTrue();
            }
        }

        [TestMethod]
        public void Subscribe_WeakReference_Should_not_keep_alive()
        {
            using (IPublisher publisher = new Publisher())
            {
                Dummy dummy = new Dummy();
                WeakReference reference = new WeakReference(dummy);

                publisher.Subscribe<object>(dummy.Do, ExecutionOption.WeakReference);

                publisher.Publish(new object());

                Check.That(dummy.CallCount).IsEqualTo(1);

                dummy = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                publisher.Publish(new object());

                Check.That(reference.IsAlive).IsFalse();
            }
        }

        [TestMethod]
        public void Publish_Should_not_execute_When_subscription_is_disposed()
        {
            using (IPublisher publisher = new Publisher())
            {
                Dummy dummy = new Dummy();

                publisher.Subscribe<object>(dummy.Do, ExecutionOption.None).Dispose();

                publisher.Publish(new object());

                Check.That(dummy.CallCount).IsEqualTo(0);
            }
        }

        [TestMethod]
        public void Publish_Should_execute_on_derived_types()
        {
            using (IPublisher publisher = new Publisher())
            {
                bool done = false;
                Action<Dummy> action = _ => done = true;

                publisher.Subscribe(action, ExecutionOption.None);

                publisher.Publish(new DummyDerived());

                Check.That(done).IsTrue();
            }
        }

        [TestMethod]
        public void Publish_Should_execute_on_implemented_interfaces()
        {
            using (IPublisher publisher = new Publisher())
            {
                bool done = false;
                Action<IList> action = _ => done = true;

                publisher.Subscribe(action, ExecutionOption.None);

                publisher.Publish(new List<object>());

                Check.That(done).IsTrue();
            }
        }

        [TestMethod]
        public void Publish_Should_execute_on_object_for_interfaces()
        {
            using (IPublisher publisher = new Publisher())
            {
                bool done = false;
                Action<object> action = _ => done = true;

                publisher.Subscribe(action, ExecutionOption.None);

                publisher.Publish(new List<object>() as IList);

                Check.That(done).IsTrue();
            }
        }

        [TestMethod, TestCategory("Performance")]
        public void Publish_None_Performance()
        {
            bool temp = false;

            Action<bool> action = b => temp = !b;

            using (IPublisher publisher = new Publisher())
            using (publisher.Subscribe(action, ExecutionOption.None))
            {
                Stopwatch wAction = new Stopwatch();
                Stopwatch wPublisher = new Stopwatch();

                for (int i = 0; i < 1000000; ++i)
                {
                    wAction.Start();
                    action(temp);
                    wAction.Stop();

                    wPublisher.Start();
                    publisher.Publish(temp);
                    wPublisher.Stop();
                }

                Console.WriteLine($"publisher to action ratio: {(double)wPublisher.ElapsedTicks / wAction.ElapsedTicks}");
            }
        }

        [TestMethod, TestCategory("Performance")]
        public void Publish_WeakReference_Performance()
        {
            object temp = new object();
            Dummy dummy = new Dummy();

            using (IPublisher publisher = new Publisher())
            using (publisher.Subscribe<object>(dummy.Do, ExecutionOption.WeakReference))
            {
                Stopwatch wAction = new Stopwatch();
                Stopwatch wPublisher = new Stopwatch();

                for (int i = 0; i < 1000000; ++i)
                {
                    wAction.Start();
                    dummy.Do(temp);
                    wAction.Stop();

                    wPublisher.Start();
                    publisher.Publish(temp);
                    wPublisher.Stop();
                }

                Console.WriteLine($"publisher to action ratio: {(double)wPublisher.ElapsedTicks / wAction.ElapsedTicks}");
            }
        }

        [TestMethod, TestCategory("Performance")]
        public void Subscribe_None_Performance()
        {
            using (IPublisher publisher = new Publisher())
            {
                Stopwatch watch = new Stopwatch();

                int count = 1000000;

                for (int i = 0; i < count; ++i)
                {
                    watch.Start();
                    publisher.Subscribe<object>(_ => { }, ExecutionOption.None);
                    watch.Stop();
                }

                publisher.Publish(new object());

                Console.WriteLine($"subscribes/second: {count/watch.Elapsed.TotalSeconds}");
            }
        }

        [TestMethod, TestCategory("Performance")]
        public void Subscribe_WeakReference_Performance()
        {
            using (IPublisher publisher = new Publisher())
            {
                Stopwatch watch = new Stopwatch();

                int count = 1000000;

                for (int i = 0; i < count; ++i)
                {
                    watch.Start();
                    publisher.Subscribe<object>(new Dummy().Do, ExecutionOption.WeakReference);
                    watch.Stop();
                }

                publisher.Publish(new object());

                Console.WriteLine($"subscribes/second: {count / watch.Elapsed.TotalSeconds}");
            }
        }

        #endregion
    }
}
