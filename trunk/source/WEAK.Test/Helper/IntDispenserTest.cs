using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using WEAK.Helper;

namespace WEAK.Test.Helper
{
    [TestClass]
    public class IntDispenserTest
    {
        #region Methods

        [TestMethod]
        public void LastInt_Should_return_startInt_after_creation()
        {
            IntDispenser dispender = new IntDispenser(42);

            Check.That(dispender.LastInt).IsEqualTo(42);
        }

        [TestMethod]
        public void GetFreeInt_Should_return_next_int()
        {
            IntDispenser dispender = new IntDispenser(42);

            Check.That(dispender.GetFreeInt()).IsEqualTo(43);
        }

        [TestMethod]
        public void GetFreeInt_Should_be_multithread_safe()
        {
            IntDispenser dispender = new IntDispenser(42);

            ConcurrentBag<int> ids = new ConcurrentBag<int>();

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 10; ++i)
            {
                tasks.Add(Task.Run(
                    () =>
                    {
                        for (int j = 0; j < 1000; ++j)
                        {
                            ids.Add(dispender.GetFreeInt());
                        }
                    }));
            }

            Task.WaitAll(tasks.ToArray());

            Check.That(ids.Count).IsEqualTo(ids.Distinct().Count());
        }

        [TestMethod]
        public void LastInt_Should_return_same_int_as_last_GetFreeInt()
        {
            IntDispenser dispender = new IntDispenser(42);

            Check.That(dispender.GetFreeInt()).IsEqualTo(dispender.LastInt);
        }

        [TestMethod]
        public void ReleaseInt_Should_put_releasedInt_to_be_reused()
        {
            IntDispenser dispender = new IntDispenser(42);

            int id = dispender.GetFreeInt();

            dispender.GetFreeInt();
            dispender.ReleaseInt(id);

            Check.That(dispender.GetFreeInt()).IsEqualTo(id);
        }

        #endregion
    }
}
