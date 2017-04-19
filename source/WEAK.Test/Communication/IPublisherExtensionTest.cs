using System;
using NFluent;
using NSubstitute;
using WEAK.Communication;
using Xunit;

namespace WEAK.Test.Communication
{
    public class IPublisherExtensionTest
    {
        #region Types

        private class InvalidNumberOfParameter
        {
            [Subscribe(ExecutionOption.None)]
            public static void Method(object arg1, object arg2) { }
        }

        private class InvalidReturnType
        {
            [Subscribe(ExecutionOption.None)]
            public static object Method(object arg1) { return null; }
        }

        private class StaticMethod
        {
            [Subscribe(ExecutionOption.None)]
            public static void Method(object arg) { }
        }

        private class InstanceMethod
        {
            [Subscribe(ExecutionOption.None)]
            public void Method(object arg) { }
        }

        private class DerivedClass : InstanceMethod
        { }

        #endregion

        #region Methods

        [Fact]
        public void AutoSubscribe_Should_thow_ArgumentNullException_When_publisher_is_null()
        {
            IPublisher publisher = null;

            Check
                .ThatCode(() => publisher.AutoSubscribe<object>())
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "publisher");
        }

        [Fact]
        public void AutoSubscribe_Should_thow_NotSupportedException_When_method_has_invalid_numbers_of_parameter()
        {
            IPublisher publisher = Substitute.For<IPublisher>();

            InvalidNumberOfParameter.Method(null, null);

            Check
                .ThatCode(() => publisher.AutoSubscribe<InvalidNumberOfParameter>())
                .Throws<NotSupportedException>();
        }

        [Fact]
        public void AutoSubscribe_Should_thow_NotSupportedException_When_method_has_a_non_void_return_type()
        {
            IPublisher publisher = Substitute.For<IPublisher>();

            InvalidReturnType.Method(null);

            Check
                .ThatCode(() => publisher.AutoSubscribe<InvalidReturnType>())
                .Throws<NotSupportedException>();
        }

        [Fact]
        public void AutoSubscribe_Should_call_publisher_Subscribe_on_decorated_static_method()
        {
            bool done = false;
            IPublisher publisher = Substitute.For<IPublisher>();
            publisher.When(p => p.Subscribe<object>(StaticMethod.Method, ExecutionOption.None)).Do(_ => done = true);

            publisher.AutoSubscribe<StaticMethod>();

            StaticMethod.Method(null);

            Check.That(done).IsTrue();
        }

        [Fact]
        public void AutoSubscribe_target_Should_thow_ArgumentNullException_When_publisher_is_null()
        {
            IPublisher publisher = null;
            object target = null;

            Check
                .ThatCode(() => publisher.AutoSubscribe(target))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "publisher");
        }

        [Fact]
        public void AutoSubscribe_target_Should_thow_ArgumentNullException_When_target_is_null()
        {
            IPublisher publisher = Substitute.For<IPublisher>();
            object target = null;

            Check
                .ThatCode(() => publisher.AutoSubscribe(target))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "target");
        }

        [Fact]
        public void AutoSubscribe_target_Should_call_publisher_Subscribe_on_decorated_instance_method()
        {
            bool done = false;
            IPublisher publisher = Substitute.For<IPublisher>();
            InstanceMethod target = new InstanceMethod();
            publisher.When(p => p.Subscribe<object>(target.Method, ExecutionOption.None)).Do(_ => done = true);

            publisher.AutoSubscribe(target);

            target.Method(null);

            Check.That(done).IsTrue();
        }

        [Fact]
        public void AutoSubscribe_target_Should_call_publisher_Subscribe_on_decorated_method_from_base_class()
        {
            bool done = false;
            IPublisher publisher = Substitute.For<IPublisher>();
            DerivedClass target = new DerivedClass();
            publisher.When(p => p.Subscribe<object>(target.Method, ExecutionOption.None)).Do(_ => done = true);

            publisher.AutoSubscribe(target);

            target.Method(null);

            Check.That(done).IsTrue();
        }

        #endregion
    }
}
