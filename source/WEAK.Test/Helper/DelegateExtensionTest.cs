using System;
using System.Diagnostics;
using NFluent;
using WEAK.Helper;
using Xunit;

namespace WEAK.Test.Helper
{
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

        [Fact]
        public void ToWeak_Should_throw_ArgumentException_When_delegateAction_is_not_a_delegate()
        {
            object delegateAction = new object();

            Check.ThatCode(() => delegateAction.ToWeak()).Throws<ArgumentException>();
        }

        [Fact]
        public void ToWeak_Should_return_null_When_delegateAction_is_a_null_delegate()
        {
            Action delegateAction = null;

            Check.That<Action>(delegateAction.ToWeak()).IsNull();
        }

        [Fact]
        public void ToWeak_Should_return_delegateAction_When_delegateAction_is_a_static_method()
        {
            DelegateTest.StaticAction();
            Action delegateAction = DelegateTest.StaticAction;

            Check.That<Action>(delegateAction.ToWeak()).IsEqualTo(delegateAction);
        }

        [Fact]
        public void ToWeak_Should_throw_ArgumentException_When_delegateAction_is_a_value_type_method()
        {
            int target = 42;
            Func<string> delegateAction = target.ToString;

            Check.ThatCode(() => delegateAction.ToWeak()).Throws<ArgumentException>();
        }

        [Fact]
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

        [Fact]
        public void ToWeak_Should_return_delegateAction_When_delegateAction_is_a_weak_delegate()
        {
            DelegateTest test = new DelegateTest();
            Action delegateAction = new Action(test.InstanceAction).ToWeak();

            Check.That<Action>(delegateAction.ToWeak()).IsEqualTo(delegateAction);
        }

        [Fact]
        public void WeakDelegate_Should_behave_like_expected_Action()
        {
            bool done = false;
            DelegateTest test = new DelegateTest(b => { done = b; });
            Action<bool> delegateAction = new Action<bool>(test.InstanceAction).ToWeak();

            delegateAction(true);

            Check.That(done).IsTrue();
        }

        [Fact]
        public void WeakDelegate_Should_behave_like_expected_Func()
        {
            DelegateTest test = new DelegateTest(b => b);
            Func<bool, bool> delegateAction = new Func<bool, bool>(test.InstanceFunc).ToWeak();

            Check.That(delegateAction(true)).IsTrue();
        }

        [Fact]
        public void WeakDelegate_Should_behave_not_hold_strong_reference()
        {
            bool done = false;
            DelegateTest test = new DelegateTest(b => { done = b; });

            WeakReference reference = new WeakReference(test);

            Action<bool> delegateAction = new Action<bool>(test.InstanceAction).ToWeak();

            delegateAction(true);

            Check.That(done).IsTrue();

            test = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            done = false;
            delegateAction(true);

            Check.That(done).IsFalse();
            Check.That(reference.IsAlive).IsFalse();
        }

        [Fact]
        public void WeakDelegate_Should_create_weak_of_all_invocation_list()
        {
            bool done = false;
            DelegateTest test = new DelegateTest(b => { done = b; });
            Action<bool> delegateAction = test.InstanceAction;
            test = new DelegateTest(b => { done = b; });
            delegateAction += test.InstanceAction;

            Check.That(delegateAction.ToWeak()).IsNotEqualTo(delegateAction);

            delegateAction = delegateAction.ToWeak();

            test = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            done = false;
            delegateAction(true);

            Check.That(done).IsFalse();
        }

        [Fact, Trait("Category", "Performance")]
        public void WeakDelegate_call_Performance()
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

        [Fact, Trait("Category", "Performance")]
        public void WeakDelegate_create_Performance()
        {
            DelegateTest test = new DelegateTest(value => { });

            test.InstanceAction();

            Action weakAction;
            Stopwatch watch = new Stopwatch();

            for (int i = 0; i < 10000; ++i)
            {
                watch.Start();
                weakAction = new Action(test.InstanceAction).ToWeak();
                watch.Stop();

                weakAction();
            }

            Console.WriteLine($"weak creation: { 10000 / watch.Elapsed.TotalSeconds}");
        }

        #endregion
    }
}
