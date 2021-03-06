﻿using System;
using System.Collections.Generic;
using NFluent;
using NSubstitute;
using WEAK.Helper;
using Xunit;

namespace WEAK.Test.Helper
{
    public class IDisposableExtensionTest
    {
        #region Methods

        [Fact]
        public void Merge_Should_not_throw_When_disposables_is_null()
        {
            IEnumerable<IDisposable> disposables = null;

            Check
                .ThatCode(() => disposables.Merge())
                .Not
                .Throws<Exception>();
        }

        [Fact]
        public void Merge_Should_not_throw_When_disposables_param_is_null()
        {
            IDisposable disposable = null;

            Check
                .ThatCode(() => disposable.Merge(null))
                .Not
                .Throws<Exception>();
        }

        [Fact]
        public void Merge_Should_return_an_IDisposable()
        {
            IDisposable disposable = Substitute.For<IDisposable>();

            Check.That(disposable.Merge()).IsNotNull();
        }

        [Fact]
        public void DisposableGroup_Dispose_Should_dispose_children_in_reverse_order()
        {
            List<IDisposable> disposed = new List<IDisposable>();

            IDisposable disposable1 = Substitute.For<IDisposable>();
            disposable1.When(i => i.Dispose()).Do(_ => disposed.Add(disposable1));

            IDisposable disposable2 = Substitute.For<IDisposable>();
            disposable2.When(i => i.Dispose()).Do(_ => disposed.Add(disposable2));

            IDisposable disposable3 = Substitute.For<IDisposable>();
            disposable3.When(i => i.Dispose()).Do(_ => disposed.Add(disposable3));

            IDisposable group = disposable1.Merge(disposable2, disposable3);

            group.Dispose();

            Check
                .That(disposed)
                .ContainsExactly(disposable3, disposable2, disposable1);
        }

        [Fact]
        public void DisposableGroup_Dispose_Should_dispose_only_once()
        {
            int disposedCount = 0;

            IDisposable disposable = Substitute.For<IDisposable>();
            disposable.When(i => i.Dispose()).Do(_ => ++disposedCount);

            IDisposable group = disposable.Merge();

            group.Dispose();
            group.Dispose();

            Check
                .That(disposedCount)
                .IsEqualTo(1);
        }

        [Fact]
        public void Merge_Should_merge_already_merged_IDisposable()
        {
            List<IDisposable> disposed = new List<IDisposable>();

            IDisposable disposable1 = Substitute.For<IDisposable>();
            disposable1.When(i => i.Dispose()).Do(_ => disposed.Add(disposable1));

            IDisposable disposable2 = Substitute.For<IDisposable>();
            disposable2.When(i => i.Dispose()).Do(_ => disposed.Add(disposable2));

            IDisposable group1 = disposable1.Merge();
            IDisposable group2 = disposable2.Merge();

            IDisposable group = group1.Merge(group2);

            group.Dispose();

            Check
                .That(disposed)
                .ContainsExactly(disposable2, disposable1);
        }

        #endregion
    }
}
