using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using WEAK.Helper;

namespace WEAK.Test.Helper
{
    [TestClass]
    public class DelegateExtensionTest
    {
        #region Types

        private class DelegateTest
        {
            #region Fields

            private readonly Action _action;
            private readonly Action<bool> _action1Param;
            private readonly Func<bool, bool> _func1Param;

            #endregion

            #region Initialisation

            public DelegateTest()
            { }

            public DelegateTest(Action action)
            {
                _action = action;
            }

            public DelegateTest(Action<bool> action1Param)
            {
                _action1Param = action1Param;
            }

            public DelegateTest(Func<bool, bool> func1Param)
            {
                _func1Param = func1Param;
            }

            #endregion

            #region Methods

            public static void StaticAction()
            { }

            public void InstanceAction()
            {
                _action?.Invoke();
            }

            public void InstanceAction(bool param)
            {
                _action1Param?.Invoke(param);
            }

            public bool InstanceFunc(bool param)
            {
                return _func1Param?.Invoke(param) ?? false;
            }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public void ToWeak_Should_return_delegateAction_When_delegateAction_is_not_a_delegate()
        {
            object delegateAction = new object();

            Check.That(delegateAction.ToWeak()).IsEqualTo(delegateAction);
        }

        [TestMethod]
        public void ToWeak_Should_return_null_When_delegateAction_is_a_null_delegate()
        {
            Action delegateAction = null;

            Check.That<Action>(delegateAction.ToWeak()).IsNull();
        }

        [TestMethod]
        public void ToWeak_Should_return_delegateAction_When_delegateAction_is_a_static_method()
        {
            DelegateTest.StaticAction();
            Action delegateAction = DelegateTest.StaticAction;

            Check.That<Action>(delegateAction.ToWeak()).IsEqualTo(delegateAction);
        }

        [TestMethod]
        public void ToWeak_Should_return_delegateAction_When_delegateAction_is_a_value_type_method()
        {
            int target = 42;
            Func<string> delegateAction = target.ToString;

            Check.That<Func<string>>(delegateAction.ToWeak()).IsEqualTo(delegateAction);
        }

        [TestMethod]
        public void ToWeak_Should_return_a_weak_delegate_When_delegateAction_is_an_instance_method()
        {
            bool done = false;
            DelegateTest test = new DelegateTest(() => done = true);
            Action delegateAction = new Action(test.InstanceAction);

            Check.That<Action>(delegateAction.ToWeak()).IsNotEqualTo(delegateAction);

            delegateAction = delegateAction.ToWeak();

            delegateAction();

            Check.That(done).IsTrue();

            done = false;

            test = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            delegateAction();

            Check.That(done).IsFalse();
        }

        [TestMethod]
        public void ToWeak_Should_return_delegateAction_When_delegateAction_is_a_weak_delegate()
        {
            DelegateTest test = new DelegateTest();
            Action delegateAction = new Action(test.InstanceAction).ToWeak();

            Check.That<Action>(delegateAction.ToWeak()).IsEqualTo(delegateAction);
        }

        [TestMethod]
        public void ToWeak_Should_return_same_delegate_When_delegate_is_the_same_instance_method()
        {
            DelegateTest test = new DelegateTest();
            Action delegateAction = new Action(test.InstanceAction);

            Check.That<Action>(delegateAction.ToWeak()).IsEqualTo(delegateAction.ToWeak());
        }

        [TestMethod]
        public void WeakDelegate_Should_behave_like_expected_Action()
        {
            bool done = false;
            DelegateTest test = new DelegateTest(b => { done = b; });
            Action<bool> delegateAction = new Action<bool>(test.InstanceAction).ToWeak();

            delegateAction(true);

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void WeakDelegate_Should_behave_like_expected_Func()
        {
            DelegateTest test = new DelegateTest(b => b);
            Func<bool, bool> delegateAction = new Func<bool, bool>(test.InstanceFunc).ToWeak();

            Check.That(delegateAction(true)).IsTrue();
        }

        [TestMethod]
        public void WeakDelegate_Should_()
        {
            bool done = false;
            DelegateTest test = new DelegateTest(b => { done = b; });
            Action<bool> delegateAction = test.InstanceAction;
            delegateAction += test.InstanceAction;
            
            Check.That(delegateAction.ToWeak()).IsEqualTo(delegateAction);
        }

        [TestMethod, TestCategory("Performance")]
        public void WeakDelegate_Performance()
        {
            bool temp = false;
            DelegateTest test = new DelegateTest(value => temp != value);
            Func<bool, bool> delegateAction = new Func<bool, bool>(test.InstanceFunc);
            Func<bool, bool> weakDelegateAction = delegateAction.ToWeak();

            Stopwatch wInstance = new Stopwatch();
            Stopwatch wDelegate = new Stopwatch();
            Stopwatch wWeak = new Stopwatch();

            for (int i = 0; i < 1000000; ++i)
            {
                wInstance.Start();
                test.InstanceFunc(true);
                wInstance.Stop();

                wDelegate.Start();
                delegateAction(true);
                wDelegate.Stop();

                wWeak.Start();
                weakDelegateAction(true);
                wWeak.Stop();
            }

            Console.WriteLine($"weak to delegate ratio: {(double)wWeak.ElapsedTicks / wDelegate.ElapsedTicks}");
            Console.WriteLine($"weak to instance ratio: {(double)wWeak.ElapsedTicks / wInstance.ElapsedTicks}");
        }

        #endregion
    }
}
