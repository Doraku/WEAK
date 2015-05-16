using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WEAK.Communication;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        private class Request : IRequest
        {
            #region IRequest

            public RequestPublishingMode PulishingMode { get; set; }

            #endregion
        }

        private class Dummy
        {
            #region Fields

            public Action Action;

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

            public void On()
            {
            }

            #endregion
        }

        static void Main(string[] args)
        {
            PublishTest();

            Console.ReadLine();
        }

        static WeakReference refe;

        static Action Get(Action action)
        {
            refe = new WeakReference(action.Target);

            return () =>
                {
                    object o = refe.Target;
                    Console.WriteLine(o);
                };
        }

        static void Test()
        {
            Dummy test = new Dummy();

            Action pouet = Get(test.On);

            test = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine(refe.IsAlive);
        }

        public static void PublishTest()
        {
            IPublisher publisher = new EventAggregator();
            {
                Dummy dummy = new Dummy();
                bool done = false;
                dummy.Action = () => done = true;
                WeakReference reference = new WeakReference(dummy);

                publisher.Subscribe<IRequest>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

                Console.WriteLine(done);

                done = false;

                publisher.Unsubscribe<IRequest>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

                Console.WriteLine(!done);

                publisher.Subscribe<Request>(dummy.On);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

                Console.WriteLine(done);

                publisher.Unsubscribe<Request>(dummy.On);

                done = false;
                dummy = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Console.WriteLine(!reference.IsAlive);

                publisher.Publish(new Request { PulishingMode = RequestPublishingMode.Direct });

                Console.WriteLine(!done);
            }
            publisher.Dispose();
        }
    }
}
