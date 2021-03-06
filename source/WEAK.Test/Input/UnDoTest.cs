﻿using System;
using NFluent;
using NSubstitute;
using WEAK.Input;
using Xunit;

namespace WEAK.Test.Input
{
    public class UnDoTest
    {
        #region Methods

        [Fact]
        public void UnDo_Should_throw_ArgumentNullException_When_doAction_is_null()
        {
            Check
                .ThatCode(() => new UnDo(null, null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "doAction");
        }

        [Fact]
        public void UnDo_Should_throw_ArgumentNullException_When_undoAction_is_null()
        {
            Check
                .ThatCode(() => new UnDo(Substitute.For<Action>(), null))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "undoAction");
        }

        [Fact]
        public void Do_Should_do_doAction()
        {
            bool done = false;
            IUnDo undo = new UnDo(() => done = true, Substitute.For<Action>());

            undo.Do();

            Check.That(done).IsTrue();
        }

        [Fact]
        public void Undo_Should_do_undoAction()
        {
            bool done = false;
            IUnDo undo = new UnDo(Substitute.For<Action>(), () => done = true);

            undo.Undo();

            Check.That(done).IsTrue();
        }

        #endregion
    }
}
