using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class ValueUnDoTest
    {
        #region Methods

        [TestMethod]
        public void ValueUnDo_Should_throw_ArgumentNullException_When_setter_is_null()
        {
            Check
                .ThatCode(() => new ValueUnDo<object>(null, null, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "setter");
        }

        [TestMethod]
        public void Do_Should_set_newValue()
        {
            object value = null;
            object oldValue = new object();
            object newValue = new object();
            IUnDo undo = new ValueUnDo<object>(v => value = v, oldValue, newValue);

            undo.Do();

            Check.That(value).IsEqualTo(newValue);
        }

        [TestMethod]
        public void Undo_Should_set_oldValue()
        {
            object value = null;
            object oldValue = new object();
            object newValue = new object();
            IUnDo undo = new ValueUnDo<object>(v => value = v, oldValue, newValue);

            undo.Undo();

            Check.That(value).IsEqualTo(oldValue);
        }

        #endregion
    }
}
