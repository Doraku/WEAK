using System;
using NFluent;
using WEAK.Helper;
using Xunit;

namespace WEAK.Test.Helper
{
    public class ObjectExtensionTest
    {
        #region Methods

        [Fact]
        public void CheckForArgumentNullException_Should_throw_ArgumentNullException_When_param_is_null()
        {
            object param = null;

            Check
                .ThatCode(() => param.CheckForArgumentNullException(nameof(param)))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", nameof(param));
        }

        [Fact]
        public void CheckForArgumentException_Should_throw_ArgumentNullException_When_validation_is_null()
        {
            object param = new object();

            Check
                .ThatCode(() => param.CheckForArgumentException(nameof(param), null, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "validation");
        }

        [Fact]
        public void CheckForArgumentException_Should_throw_ArgumentException_When_param_is_not_validated()
        {
            object param = new object();

            ArgumentException expectedException = new ArgumentException("message", nameof(param));

            Check
                .ThatCode(() => param.CheckForArgumentException(nameof(param), p => false, "message"))
                .Throws<ArgumentException>()
                .WithProperty("ParamName", nameof(param))
                .And.WithMessage(expectedException.Message);
        }

        [Fact]
        public void CheckForArgumentException_Should_return_param()
        {
            object param = new object();

            Check
                .That(param.CheckForArgumentException(nameof(param), p => true, "message"))
                .IsEqualTo(param);
        }

        [Fact]
        public void CheckForArgumentNullException_Should_return_param()
        {
            object param = new object();

            Check
                .That(param.CheckForArgumentNullException(nameof(param)))
                .IsEqualTo(param);
        }

        #endregion
    }
}
