﻿using System;
using System.Collections.Generic;
using NFluent;
using NSubstitute;
using WEAK.Input;
using Xunit;

namespace WEAK.Test.Input
{
    public class IListExtensionTest
    {
        #region Methods

        [Fact]
        public void ToUnDo_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IList<int> source = null;

            Check
                .ThatCode(() => source.ToUnDo(null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [Fact]
        public void ToUnDo_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IList<int> source = Substitute.For<IList<int>>();

            Check
                .ThatCode(() => source.ToUnDo(null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [Fact]
        public void UnDoList_IndexOf_Should_return_IndexOf()
        {
            IList<object> source = Substitute.For<IList<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object value = new object();

            source.IndexOf(value).Returns(42);

            IList<object> unDoList = source.ToUnDo(manager);

            Check.That(unDoList.IndexOf(value)).IsEqualTo(source.IndexOf(value));
        }

        [Fact]
        public void UnDoList_Insert_Should_Insert()
        {
            IList<object> source = Substitute.For<IList<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            int index = 42;
            object value = new object();

            bool done = false;

            source.When(s => s.Insert(index, value)).Do(_ => done = true);
            manager.Do(Arg.Do<IUnDo>(i => i.Do()));

            IList<object> unDoList = source.ToUnDo(manager);

            unDoList.Insert(index, value);

            Check.That(done).IsTrue();
        }

        [Fact]
        public void UnDoList_RemoveAt_Should_RemoveAt()
        {
            IList<object> source = Substitute.For<IList<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            int index = 42;

            bool done = false;

            source.When(s => s.RemoveAt(index)).Do(_ => done = true);
            manager.Do(Arg.Do<IUnDo>(i => i.Do()));

            IList<object> unDoList = source.ToUnDo(manager);

            unDoList.RemoveAt(index);

            Check.That(done).IsTrue();
        }

        [Fact]
        public void UnDoList_this_index_get_Should_return_this_index_get()
        {
            IList<object> source = Substitute.For<IList<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            int index = 42;
            object value = new object();

            source[index].Returns(value);

            IList<object> unDoList = source.ToUnDo(manager);

            Check.That(unDoList[index]).IsEqualTo(source[index]);
        }

        [Fact]
        public void UnDoList_this_index_set_Should_set_this_index()
        {
            IList<object> source = Substitute.For<IList<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            int index = 42;
            object value = new object();

            bool done = false;

            source.When(s => s[index] = value).Do(_ => done = true);
            manager.Do(Arg.Do<IUnDo>(i => i.Do()));

            IList<object> unDoList = source.ToUnDo(manager);

            unDoList[index] = value;

            Check.That(done).IsTrue();
        }

        #endregion
    }
}
