using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using NSubstitute;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class IDictionaryExtensionTest
    {
        #region Methods

        [TestMethod]
        public void ToUnDo_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IDictionary<object, object> source = null;

            Check
                .ThatCode(() => source.ToUnDo(null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void ToUnDo_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();

            Check
                .ThatCode(() => source.ToUnDo(null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void ToUnDo_Should_return_an_IDictionary()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();

            Check.That(source.ToUnDo(manager)).IsNotNull();
        }

        [TestMethod]
        public void UnDoDictionary_Add_key_Should_Add_key()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object key = new object();
            object value = new object();

            bool done = false;

            source.When(s => s.Add(key, value)).Do(_ => done = true);
            manager.Do(Arg.Do<IUnDo>(i => i.Do()));

            IDictionary<object, object> unDoDictionary = source.ToUnDo(manager);

            unDoDictionary.Add(key, value);

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void UnDoDictionary_ContainsKey_Should_return_ContainsKey()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object key = new object();

            source.ContainsKey(key).Returns(true);

            IDictionary<object, object> unDoDictionary = source.ToUnDo(manager);

            Check.That(unDoDictionary.ContainsKey(key)).IsEqualTo(source.ContainsKey(key));
        }

        [TestMethod]
        public void UnDoDictionary_Remove_key_Should_return_Remove_key()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object key = new object();
            object value = new object();

            source.Remove(key).Returns(true);
            source.TryGetValue(key, out value).ReturnsForAnyArgs(true);
            manager.Do(Arg.Do<IUnDo>(i => i.Do()));

            IDictionary<object, object> unDoDictionary = source.ToUnDo(manager);

            Check.That(unDoDictionary.Remove(key)).IsEqualTo(source.Remove(key));
        }

        [TestMethod]
        public void UnDoDictionary_TryGetValue_Should_return_TryGetValue()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object key = new object();
            object value = new object();

            object value1, value2;
            source.TryGetValue(key, out value2).ReturnsForAnyArgs(c => { c[1] = value; return true; });

            IDictionary<object, object> unDoDictionary = source.ToUnDo(manager);

            Check.That(unDoDictionary.TryGetValue(key, out value1)).IsEqualTo(source.TryGetValue(key, out value2));
            Check.That(value1).IsEqualTo(value);
            Check.That(value1).IsEqualTo(value2);
        }

        [TestMethod]
        public void UnDoDictionary_this_key_get_Should_return_this_key_get()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object key = new object();
            object value = new object();

            source[key].Returns(value);

            IDictionary<object, object> unDoDictionary = source.ToUnDo(manager);

            Check.That(unDoDictionary[key]).IsEqualTo(source[key]);
        }

        [TestMethod]
        public void UnDoDictionary_this_key_set_Should_set_this_key_When_TryGetValue_is_true()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object key = new object();
            object value = new object();

            bool done = false;

            source.TryGetValue(key, out value).ReturnsForAnyArgs(true);
            source.When(s => s[key] = value).Do(_ => done = true);
            manager.Do(Arg.Do<IUnDo>(i => i.Do()));

            IDictionary<object, object> unDoDictionary = source.ToUnDo(manager);

            unDoDictionary[key] = value;

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void UnDoDictionary_this_key_set_Should_set_this_key_When_TryGetValue_is_false()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object key = new object();
            object value = new object();

            bool done = false;

            source.TryGetValue(key, out value).ReturnsForAnyArgs(false);
            source.When(s => s[key] = value).Do(_ => done = true);
            manager.Do(Arg.Do<IUnDo>(i => i.Do()));

            IDictionary<object, object> unDoDictionary = source.ToUnDo(manager);

            unDoDictionary[key] = value;

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void UnDoDictionary_Keys_Should_return_Keys()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            ICollection<object> keys = Substitute.For<ICollection<object>>();

            source.Keys.Returns(keys);

            IDictionary<object, object> unDoDictionary = source.ToUnDo(manager);

            Check.That(unDoDictionary.Keys).IsEqualTo(source.Keys);
        }

        [TestMethod]
        public void UnDoDictionary_Values_Should_return_Values()
        {
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            ICollection<object> values = Substitute.For<ICollection<object>>();

            source.Values.Returns(values);

            IDictionary<object, object> unDoDictionary = source.ToUnDo(manager);

            Check.That(unDoDictionary.Values).IsEqualTo(source.Values);
        }

        #endregion
    }
}
