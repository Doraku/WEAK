using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using NSubstitute;
using WEAK.Input;

namespace WEAK.Test.Input
{
    [TestClass]
    public class UnDoManagerTest
    {
        #region Methods

        [TestMethod]
        public void Version_Should_incremente_When_a_command_is_done()
        {
            IUnDoManager manager = new UnDoManager();

            int oldVersion = manager.Version;

            manager.Do(Substitute.For<IUnDo>());

            Check.That(manager.Version).IsGreaterThan(oldVersion);
        }

        [TestMethod]
        public void Version_Should_return_old_value_When_a_command_is_undone()
        {
            IUnDoManager manager = new UnDoManager();

            int oldVersion = manager.Version;

            manager.Do(Substitute.For<IUnDo>());
            manager.Undo();

            Check.That(manager.Version).IsEqualTo(oldVersion);
        }

        [TestMethod]
        public void Version_Should_return_last_value_When_a_command_is_redone()
        {
            IUnDoManager manager = new UnDoManager();

            manager.Do(Substitute.For<IUnDo>());

            int lastVersion = manager.Version;

            manager.Undo();
            manager.Redo();

            Check.That(manager.Version).IsEqualTo(lastVersion);
        }

        [TestMethod]
        public void CanUndo_Should_return_false_When_no_command_has_been_done()
        {
            IUnDoManager manager = new UnDoManager();

            Check.That(manager.CanUndo).IsFalse();
        }

        [TestMethod]
        public void CanUndo_Should_return_true_When_a_command_has_been_done()
        {
            IUnDoManager manager = new UnDoManager();

            manager.Do(Substitute.For<IUnDo>());

            Check.That(manager.CanUndo).IsTrue();
        }

        [TestMethod]
        public void CanUndo_Should_return_false_When_all_commands_have_been_undone()
        {
            IUnDoManager manager = new UnDoManager();

            manager.Do(Substitute.For<IUnDo>());
            manager.UndoAll();

            Check.That(manager.CanUndo).IsFalse();
        }

        [TestMethod]
        public void CanRedo_Should_return_false_When_no_command_has_been_done()
        {
            IUnDoManager manager = new UnDoManager();

            Check.That(manager.CanRedo).IsFalse();
        }

        [TestMethod]
        public void CanRedo_Should_return_true_When_a_command_has_been_undone()
        {
            IUnDoManager manager = new UnDoManager();

            manager.Do(Substitute.For<IUnDo>());
            manager.Undo();

            Check.That(manager.CanRedo).IsTrue();
        }

        [TestMethod]
        public void CanRedo_Should_return_false_When_all_commands_have_been_redone()
        {
            IUnDoManager manager = new UnDoManager();

            manager.Do(Substitute.For<IUnDo>());
            manager.UndoAll();
            manager.RedoAll();

            Check.That(manager.CanRedo).IsFalse();
        }

        [TestMethod]
        public void Clear_Should_clear_undone_and_done_history()
        {
            IUnDoManager manager = new UnDoManager();

            manager.Do(Substitute.For<IUnDo>());
            manager.Do(Substitute.For<IUnDo>());
            manager.Undo();

            Check.That(manager.CanUndo).IsTrue();
            Check.That(manager.CanRedo).IsTrue();

            manager.Clear();

            Check.That(manager.CanUndo).IsFalse();
            Check.That(manager.CanRedo).IsFalse();
        }

        [TestMethod]
        public void Do_Should_throw_ArgumentNullException_When_command_is_null()
        {
            IUnDoManager manager = new UnDoManager();

            Check
                .ThatCode(() => manager.Do(null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "command");
        }

        [TestMethod]
        public void Do_Should_Do()
        {
            IUnDoManager manager = new UnDoManager();
            IUnDo undo = Substitute.For<IUnDo>();

            bool done = false;
            undo.When(u => u.Do()).Do(_ => done = true);

            manager.Do(undo);

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void Do_Should_not_add_command_in_history_when_a_group_is_going_on()
        {
            IUnDoManager manager = new UnDoManager();
            IUnDo undo = Substitute.For<IUnDo>();
            int version = manager.Version;

            using (manager.BeginGroup())
            {
                manager.Do(undo);

                Check.That(manager.Version).IsEqualTo(version);
            }
        }

        [TestMethod]
        public void BeginGroup_Should_return_an_IDisposable()
        {
            IUnDoManager manager = new UnDoManager();

            Check.That(manager.BeginGroup()).IsNotNull();
        }

        [TestMethod]
        public void BeginGroup_Should_add_commands_as_one_operation_in_history_once_disposed()
        {
            IUnDoManager manager = new UnDoManager();
            IUnDo undo = Substitute.For<IUnDo>();
            int version = manager.Version;

            using (manager.BeginGroup())
            {
                manager.Do(undo);
                manager.Do(undo);
            }

            Check.That(manager.Version).IsGreaterThan(version);

            manager.Undo();

            Check.That(manager.Version).IsEqualTo(version);
        }

        [TestMethod]
        public void Do_Should_clear_undone_history()
        {
            IUnDoManager manager = new UnDoManager();

            manager.Do(Substitute.For<IUnDo>());
            manager.Do(Substitute.For<IUnDo>());
            manager.Undo();

            Check.That(manager.CanRedo).IsTrue();

            manager.Do(Substitute.For<IUnDo>());

            Check.That(manager.CanRedo).IsFalse();
        }

        [TestMethod]
        public void Undo_Should_throw_InvalidOperationException_When_CanUndo_is_false()
        {
            IUnDoManager manager = new UnDoManager();

            Check
                .ThatCode(() => manager.Undo())
                .Throws<InvalidOperationException>()
                .WithMessage("There is no action to undo.");
        }

        [TestMethod]
        public void Undo_Should_throw_InvalidOperationException_When_a_group_operation_is_going_on()
        {
            IUnDoManager manager = new UnDoManager();

            using (manager.BeginGroup())
            {
                Check
                    .ThatCode(() => manager.Undo())
                    .Throws<InvalidOperationException>()
                    .WithMessage("Cannot perform Undo while a group operation is going on.");
            }
        }

        [TestMethod]
        public void Undo_Should_Undo()
        {
            IUnDoManager manager = new UnDoManager();
            IUnDo undo = Substitute.For<IUnDo>();

            bool done = false;
            undo.When(u => u.Undo()).Do(_ => done = true);

            manager.Do(undo);
            manager.Undo();

            Check.That(done).IsTrue();
        }

        [TestMethod]
        public void Redo_Should_throw_InvalidOperationException_When_CanRedo_is_false()
        {
            IUnDoManager manager = new UnDoManager();

            Check
                .ThatCode(() => manager.Redo())
                .Throws<InvalidOperationException>()
                .WithMessage("There is no action to redo.");
        }

        [TestMethod]
        public void Redo_Should_throw_InvalidOperationException_When_a_group_operation_is_going_on()
        {
            IUnDoManager manager = new UnDoManager();

            using (manager.BeginGroup())
            {
                Check
                    .ThatCode(() => manager.Redo())
                    .Throws<InvalidOperationException>()
                    .WithMessage("Cannot perform Redo while a group operation is going on.");
            }
        }

        [TestMethod]
        public void Redo_Should_Redo()
        {
            IUnDoManager manager = new UnDoManager();
            IUnDo undo = Substitute.For<IUnDo>();

            manager.Do(undo);
            manager.Undo();

            bool done = false;
            undo.When(u => u.Do()).Do(_ => done = true);

            manager.Redo();

            Check.That(done).IsTrue();
        }

        #endregion
    }
}
