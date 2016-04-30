using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using WEAK.Helper;

namespace WEAK.Test.Helper
{
    [TestClass]
    public class ObjectExtensionTest
    {
        #region Methods

        [TestMethod]
        public void CheckParameter_Should_throw_ArgumentNullException_When_param_is_null()
        {
            object param = null;

            Check
                .ThatCode(() => param.CheckParameter(nameof(param)))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", nameof(param));
        }

        [TestMethod]
        public void CheckParameter_Should_throw_ArgumentException_When_param_is_not_validated()
        {
            object param = new object();

            ArgumentException expectedException = new ArgumentException("message", nameof(param));

            Check
                .ThatCode(() => param.CheckParameter(nameof(param), p => false, "message"))
                .Throws<ArgumentException>()
                .WithProperty("ParamName", nameof(param))
                .And.WithMessage(expectedException.Message);
        }

        [TestMethod]
        public void CheckParameter_Should_not_throw_When_param_is_validated()
        {
            object param = new object();

            Check
                .ThatCode(() => param.CheckParameter(nameof(param), p => true, "message"))
                .Not.ThrowsAny();
        }

        [TestMethod]
        public void CheckParameter_Should_not_throw_When_validation_is_null()
        {
            object param = new object();

            Check
                .ThatCode(() => param.CheckParameter(nameof(param), null, "message"))
                .Not.ThrowsAny();
        }

        #endregion
    }
}
