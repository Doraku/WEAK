using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using NSubstitute;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class IUnDoManagerExtensionTest
    {
        #region Methods

        [TestMethod]
        public void DoAdd_ICollection_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            ICollection<object> source = null;


            Check
                .ThatCode(() => manager.DoAdd(source, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void DoAdd_ICollection_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            ICollection<object> source = null;


            Check
                .ThatCode(() => manager.DoAdd(source, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void DoClear_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            ICollection<object> source = null;


            Check
                .ThatCode(() => manager.DoClear(source))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void DoClear_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            ICollection<object> source = null;


            Check
                .ThatCode(() => manager.DoClear(source))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void DoClear_Should_add_old_elements_When_undone()
        {
            ICollection<object> source = new List<object>
            {
                new object(),
                new object(),
                new object()
            };
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IUnDo undo = null;
            List<object> sourceCopy = source.ToList();

            manager.Do(Arg.Do<IUnDo>(i => undo = i));

            manager.DoClear(source);

            Check.That(undo).IsNotNull();

            undo.Do();

            Check.That(source.Count).IsEqualTo(0);

            undo.Undo();

            Check.That(source).ContainsExactly(sourceCopy);
        }

        [TestMethod]
        public void DoRemove_ICollection_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            ICollection<object> source = null;


            Check
                .ThatCode(() => manager.DoRemove(source, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void DoRemove_ICollection_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            ICollection<object> source = null;


            Check
                .ThatCode(() => manager.DoRemove(source, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void DoAdd_IDictionary_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            IDictionary<object, object> source = null;
            object key = null;


            Check
                .ThatCode(() => manager.DoAdd(source, key, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void DoAdd_IDictionary_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IDictionary<object, object> source = null;
            object key = null;


            Check
                .ThatCode(() => manager.DoAdd(source, key, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void DoAdd_IDictionary_Should_throw_ArgumentNullException_When_key_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            object key = null;


            Check
                .ThatCode(() => manager.DoAdd(source, key, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "key");
        }

        [TestMethod]
        public void DoRemove_IDictionary_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            IDictionary<object, object> source = null;
            object key = null;

            Check
                .ThatCode(() => manager.DoRemove(source, key))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void DoRemove_IDictionary_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IDictionary<object, object> source = null;
            object key = null;

            Check
                .ThatCode(() => manager.DoRemove(source, key))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void DoRemove_IDictionary_Should_throw_ArgumentNullException_When_key_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            object key = null;

            Check
                .ThatCode(() => manager.DoRemove(source, key))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "key");
        }

        [TestMethod]
        public void Do_IDictionary_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            IDictionary<object, object> source = null;
            object key = null;

            Check
                .ThatCode(() => manager.Do(source, key, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void Do_IDictionary_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IDictionary<object, object> source = null;
            object key = null;

            Check
                .ThatCode(() => manager.Do(source, key, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void Do_IDictionary_Should_throw_ArgumentNullException_When_key_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IDictionary<object, object> source = Substitute.For<IDictionary<object, object>>();
            object key = null;

            Check
                .ThatCode(() => manager.Do(source, key, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "key");
        }

        [TestMethod]
        public void Do_IDictionary_Should_remove_element_When_undone()
        {
            object key = new object();
            IDictionary<object, object> source = new Dictionary<object, object>();
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IUnDo undo = null;

            manager.Do(Arg.Do<IUnDo>(i => undo = i));

            manager.Do(source, key, null);

            Check.That(undo).IsNotNull();

            undo.Do();

            Check.That(source.ContainsKey(key)).IsTrue();

            undo.Undo();

            Check.That(source.ContainsKey(key)).IsFalse();
        }

        [TestMethod]
        public void DoInsert_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            IList<object> source = null;


            Check
                .ThatCode(() => manager.DoInsert(source, 0, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void DoInsert_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IList<object> source = null;


            Check
                .ThatCode(() => manager.DoInsert(source, 0, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void DoRemoveAt_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            IList<object> source = null;


            Check
                .ThatCode(() => manager.DoRemoveAt(source, 0))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void DoRemoveAt_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IList<object> source = null;


            Check
                .ThatCode(() => manager.DoRemoveAt(source, 0))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void Do_IList_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            IList<object> source = null;


            Check
                .ThatCode(() => manager.Do(source, 0, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void Do_IList_Should_throw_ArgumentNullException_When_source_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            IList<object> source = null;


            Check
                .ThatCode(() => manager.Do(source, 0, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "source");
        }

        [TestMethod]
        public void Do_UnDo_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            Action doAction = null;
            Action undoAction = null;

            Check
                .ThatCode(() => manager.Do(doAction, undoAction))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void Do_UnDo_Should_throw_ArgumentNullException_When_doAction_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            Action doAction = null;
            Action undoAction = null;

            Check
                .ThatCode(() => manager.Do(doAction, undoAction))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "doAction");
        }

        [TestMethod]
        public void Do_UnDo_Should_throw_ArgumentNullException_When_undoAction_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            Action doAction = Substitute.For<Action>();
            Action undoAction = null;

            Check
                .ThatCode(() => manager.Do(doAction, undoAction))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "undoAction");
        }

        [TestMethod]
        public void Do_ValueUnDo_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;
            Action<object> setter = null;

            Check
                .ThatCode(() => manager.Do(setter, null, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void Do_ValueUnDo_Should_throw_ArgumentNullException_When_setter_is_null()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();
            Action<object> setter = null;

            Check
                .ThatCode(() => manager.Do(setter, null, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "setter");
        }

        [TestMethod]
        public void UndoAll_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;

            Check
                .ThatCode(() => manager.UndoAll())
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void UndoAll_Should_Undo_while_CanUndo()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();

            int count = 3;
            int canUndoCount = count;
            int undoCount = 0;

            manager.CanUndo.Returns(_ => canUndoCount-- > 0);
            manager.When(m => m.Undo()).Do(_ => ++undoCount);

            manager.UndoAll();

            Check.That(undoCount).IsEqualTo(count);
        }

        [TestMethod]
        public void RedoAll_Should_throw_ArgumentNullException_When_manager_is_null()
        {
            IUnDoManager manager = null;

            Check
                .ThatCode(() => manager.RedoAll())
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "manager");
        }

        [TestMethod]
        public void RedoAll_Should_Redo_while_CanRedo()
        {
            IUnDoManager manager = Substitute.For<IUnDoManager>();

            int count = 3;
            int canRedoCount = count;
            int redoCount = 0;

            manager.CanRedo.Returns(_ => canRedoCount-- > 0);
            manager.When(m => m.Redo()).Do(_ => ++redoCount);

            manager.RedoAll();

            Check.That(redoCount).IsEqualTo(count);
        }

        #endregion
    }
}
