﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information. 

using System;
using System.Collections;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace ReactiveTests.Tests
{
    public class ToEnumerableTest : ReactiveTest
    {

        [Test]
        public void ToEnumerable_ArgumentChecking()
        {
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.ToEnumerable(default(IObservable<int>)));
        }

        [Test]
        public void ToEnumerable_Generic()
        {
            Assert.True(Observable.Range(0, 10).ToEnumerable().SequenceEqual(Enumerable.Range(0, 10)));
        }

        [Test]
        public void ToEnumerable_NonGeneric()
        {
            Assert.True(((IEnumerable)Observable.Range(0, 10).ToEnumerable()).Cast<int>().SequenceEqual(Enumerable.Range(0, 10)));
        }

        [Test]
        public void ToEnumerable_ManualGeneric()
        {
            var res = Observable.Range(0, 10).ToEnumerable();
            var ieg = res.GetEnumerator();
            for (var i = 0; i < 10; i++)
            {
                Assert.True(ieg.MoveNext());
                Assert.AreEqual(i, ieg.Current);
            }
            Assert.False(ieg.MoveNext());
        }

        [Test]
        public void ToEnumerable_ManualNonGeneric()
        {
            var res = (IEnumerable)Observable.Range(0, 10).ToEnumerable();
            var ien = res.GetEnumerator();
            for (var i = 0; i < 10; i++)
            {
                Assert.True(ien.MoveNext());
                Assert.AreEqual(i, ien.Current);
            }
            Assert.False(ien.MoveNext());
        }

        [Test]
        public void ToEnumerable_ResetNotSupported()
        {
            ReactiveAssert.Throws<NotSupportedException>(() => Observable.Range(0, 10).ToEnumerable().GetEnumerator().Reset());
        }

    }
}
