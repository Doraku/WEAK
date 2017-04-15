using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using NSubstitute;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class ICollectionExtensionTest
    {
        #region Methods

        [TestMethod]
        public void ToUnDo_Should_throw_ArgumentNullException_When_source_is_null()
        {
            ICollection<int> source = null;

            Check
                .ThatCode(() => source.ToUnDo(null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void ToUnDo_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            ICollection<int> source = Substitute.For<ICollection<int>>();

            Check
                .ThatCode(() => source.ToUnDo(null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void ToUnDo_Should_return_an_ICollection()
        {
            ICollection<int> source = Substitute.For<ICollection<int>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();

            Check.That(source.ToUnDo(manager)).IsNotNull();
        }

        [TestMethod]
        public void UnDoCollection_Add_Should_Add()
        {
            ICollection<object> source = Substitute.For<ICollection<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object value = new object();

            bool done = false;

            source.When(s => s.Add(value)).Do(_ => done = true);
            manager.Do(Arg.Do<IUnDo>(i => i.Do()));

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            unDoCollection.Add(value);

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void UnDoCollection_Clear_Should_Clear()
        {
            ICollection<object> source = Substitute.For<ICollection<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();

            bool done = false;

            source.When(s => s.Clear()).Do(_ => done = true);
            manager.Do(Arg.Do<IUnDo>(i => i.Do()));

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            unDoCollection.Clear();

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void UnDoCollection_Contains_Should_return_Contains()
        {
            ICollection<object> source = Substitute.For<ICollection<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object value = new object();

            source.Contains(value).Returns(true);

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            Check.That(unDoCollection.Contains(value)).IsEqualTo(source.Contains(value));
        }

        [TestMethod]
        public void UnDoCollection_CopyTo_Should_CopyTo()
        {
            ICollection<object> source = Substitute.For<ICollection<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object[] values = new object[0];
            int index = 42;

            bool done = false;

            source.When(s => s.CopyTo(values, index)).Do(_ => done = true);

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            unDoCollection.CopyTo(values, index);

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void UnDoCollection_Count_Should_return_Count()
        {
            ICollection<object> source = Substitute.For<ICollection<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();

            source.Count.Returns(1337);

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            Check.That(unDoCollection.Count).IsEqualTo(source.Count);
        }

        [TestMethod]
        public void UnDoCollection_IsReadOnly_Should_return_IsReadOnly()
        {
            ICollection<object> source = Substitute.For<ICollection<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();

            source.IsReadOnly.Returns(true);

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            Check.That(unDoCollection.IsReadOnly).IsEqualTo(source.IsReadOnly);
        }

        [TestMethod]
        public void UnDoCollection_Remove_Should_return_Remove()
        {
            ICollection<object> source = Substitute.For<ICollection<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            object value = new object();

            source.Remove(value).Returns(true);
            source.Contains(value).Returns(true);
            manager.Do(Arg.Do<IUnDo>(i => i.Do()));

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            Check.That(unDoCollection.Remove(value)).IsEqualTo(source.Remove(value));
        }

        [TestMethod]
        public void UnDoCollection_GetEnumerator_T_Should_return_GetEnumerator_T()
        {
            ICollection<object> source = Substitute.For<ICollection<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IEnumerator<object> enumerator = Substitute.For<IEnumerator<object>>();

            source.GetEnumerator().Returns(enumerator);

            ICollection<object> unDoCollection = source.ToUnDo(manager);


            Check.That(unDoCollection.GetEnumerator()).IsEqualTo(source.GetEnumerator());
        }

        [TestMethod]
        public void UnDoCollection_GetEnumerator_Should_return_GetEnumerator()
        {
            ICollection<object> source = Substitute.For<ICollection<object>>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IEnumerator enumerator = Substitute.For<IEnumerator>();

            (source as IEnumerable).GetEnumerator().Returns(enumerator);

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            Check.That((unDoCollection as IEnumerable).GetEnumerator()).IsEqualTo((source as IEnumerable).GetEnumerator());
        }

        [TestMethod]
        public void UnDoCollection_CollectionChanged_add_Should_add_CollectionChanged()
        {
            ICollection<object> source = Substitute.For<ICollection<object>, INotifyCollectionChanged>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IEnumerator enumerator = Substitute.For<IEnumerator>();
            NotifyCollectionChangedEventHandler handler = Substitute.For<NotifyCollectionChangedEventHandler>();

            bool done = false;

            (source as INotifyCollectionChanged).When(s => s.CollectionChanged += handler).Do(_ => done = true);

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            (unDoCollection as INotifyCollectionChanged).CollectionChanged += handler;

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void UnDoCollection_CollectionChanged_remove_Should_remove_CollectionChanged()
        {
            ICollection<object> source = Substitute.For<ICollection<object>, INotifyCollectionChanged>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IEnumerator enumerator = Substitute.For<IEnumerator>();
            NotifyCollectionChangedEventHandler handler = Substitute.For<NotifyCollectionChangedEventHandler>();

            bool done = false;

            (source as INotifyCollectionChanged).When(s => s.CollectionChanged -= handler).Do(_ => done = true);

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            (unDoCollection as INotifyCollectionChanged).CollectionChanged -= handler;

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void UnDoCollection_PropertyChanged_add_Should_add_PropertyChanged()
        {
            ICollection<object> source = Substitute.For<ICollection<object>, INotifyPropertyChanged>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IEnumerator enumerator = Substitute.For<IEnumerator>();
            PropertyChangedEventHandler handler = Substitute.For<PropertyChangedEventHandler>();

            bool done = false;

            (source as INotifyPropertyChanged).When(s => s.PropertyChanged += handler).Do(_ => done = true);

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            (unDoCollection as INotifyPropertyChanged).PropertyChanged += handler;

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void UnDoCollection_PropertyChanged_remove_Should_remove_PropertyChanged()
        {
            ICollection<object> source = Substitute.For<ICollection<object>, INotifyPropertyChanged>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IEnumerator enumerator = Substitute.For<IEnumerator>();
            PropertyChangedEventHandler handler = Substitute.For<PropertyChangedEventHandler>();

            bool done = false;

            (source as INotifyPropertyChanged).When(s => s.PropertyChanged -= handler).Do(_ => done = true);

            ICollection<object> unDoCollection = source.ToUnDo(manager);

            (unDoCollection as INotifyPropertyChanged).PropertyChanged -= handler;

            Check.That(done).IsTrue();
        }

        #endregion
    }
}
