using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using NSubstitute;
using WEAK.Communication;

namespace WEAK.Test.Communication
{
    [TestClass]
    public class IPublisherExtensionTest
    {
        #region Types

        private class InvalidNumberOfParameter
        {
            [Subscribe(ExecutionMode.Direct)]
            public static void Method(object arg1, object arg2) { }
        }

        private class InvalidReturnType
        {
            [Subscribe(ExecutionMode.Direct)]
            public static object Method(object arg1) { return null; }
        }

        private class StaticMethod
        {
            [Subscribe(ExecutionMode.Direct)]
            public static void Method(object arg) { }
        }

        private class InstanceMethod
        {
            [Subscribe(ExecutionMode.Direct)]
            public void Method(object arg) { }
        }

        private class DerivedClass : InstanceMethod
        { }

        #endregion

        #region Methods

        [TestMethod]
        public void Subscribe_Should_thow_ArgumentNullException_When_publisher_is_null()
        {
            IPublisher publisher = null;

            Check
                .ThatCode(() => publisher.Subscribe<object>())
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "publisher");
        }

        [TestMethod]
        public void Subscribe_Should_thow_NotSupportedException_When_method_has_invalid_numbers_of_parameter()
        {
            IPublisher publisher = Substitute.For<IPublisher>();

            InvalidNumberOfParameter.Method(null, null);

            Check
                .ThatCode(() => publisher.Subscribe<InvalidNumberOfParameter>())
                .Throws<NotSupportedException>();
        }

        [TestMethod]
        public void Subscribe_Should_thow_NotSupportedException_When_method_has_a_non_void_return_type()
        {
            IPublisher publisher = Substitute.For<IPublisher>();

            InvalidReturnType.Method(null);

            Check
                .ThatCode(() => publisher.Subscribe<InvalidReturnType>())
                .Throws<NotSupportedException>();
        }

        [TestMethod]
        public void Subscribe_Should_call_publisher_Subscribe_on_decorated_static_method()
        {
            bool done = false;
            IPublisher publisher = Substitute.For<IPublisher>();
            publisher.When(p => p.Subscribe<object>(StaticMethod.Method, ExecutionMode.Direct)).Do(_ => done = true);

            publisher.Subscribe<StaticMethod>();

            StaticMethod.Method(null);

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void Subscribe_target_Should_thow_ArgumentNullException_When_publisher_is_null()
        {
            IPublisher publisher = null;
            object target = null;

            Check
                .ThatCode(() => publisher.Subscribe(target))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "publisher");
        }

        [TestMethod]
        public void Subscribe_target_Should_thow_ArgumentNullException_When_target_is_null()
        {
            IPublisher publisher = Substitute.For<IPublisher>();
            object target = null;

            Check
                .ThatCode(() => publisher.Subscribe(target))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "target");
        }

        [TestMethod]
        public void Subscribe_target_Should_call_publisher_Subscribe_on_decorated_instance_method()
        {
            bool done = false;
            IPublisher publisher = Substitute.For<IPublisher>();
            InstanceMethod target = new InstanceMethod();
            publisher.When(p => p.Subscribe<object>(target.Method, ExecutionMode.Direct)).Do(_ => done = true);

            publisher.Subscribe(target);

            target.Method(null);

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void Subscribe_target_Should_call_publisher_Subscribe_on_decorated_method_from_base_class()
        {
            bool done = false;
            IPublisher publisher = Substitute.For<IPublisher>();
            DerivedClass target = new DerivedClass();
            publisher.When(p => p.Subscribe<object>(target.Method, ExecutionMode.Direct)).Do(_ => done = true);

            publisher.Subscribe(target);

            target.Method(null);

            Check.That(done).IsTrue();
        }

        #endregion
    }
}
