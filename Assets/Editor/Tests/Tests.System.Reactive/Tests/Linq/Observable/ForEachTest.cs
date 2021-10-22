﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace ReactiveTests.Tests
{
    public class ForEachTest : ReactiveTest
    {

        [Test]
        public void ForEach_ArgumentChecking()
        {
            var someObservable = Observable.Empty<int>();

            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.ForEach(default(IObservable<int>), x => { }));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.ForEach(someObservable, default(Action<int>)));

            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.ForEach(default(IObservable<int>), (x, i) => { }));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.ForEach(someObservable, default(Action<int, int>)));
        }

        [Test]
        public void ForEach_Empty()
        {
            var lst = new List<int>();
            Observable.Empty<int>().ForEach(x => lst.Add(x));
            Assert.True(lst.SequenceEqual(Enumerable.Empty<int>()));
        }

        [Test]
        public void ForEach_Index_Empty()
        {
            var lstX = new List<int>();
            Observable.Empty<int>().ForEach((x, i) => lstX.Add(x));
            Assert.True(lstX.SequenceEqual(Enumerable.Empty<int>()));
        }

        [Test]
        public void ForEach_Return()
        {
            var lst = new List<int>();
            Observable.Return(42).ForEach(x => lst.Add(x));
            Assert.True(lst.SequenceEqual(new[] { 42 }));
        }

        [Test]
        public void ForEach_Index_Return()
        {
            var lstX = new List<int>();
            var lstI = new List<int>();
            Observable.Return(42).ForEach((x, i) => { lstX.Add(x); lstI.Add(i); });
            Assert.True(lstX.SequenceEqual(new[] { 42 }));
            Assert.True(lstI.SequenceEqual(new[] { 0 }));
        }

        [Test]
        public void ForEach_Throw()
        {
            var ex = new Exception();

            var xs = Observable.Throw<int>(ex);

            ReactiveAssert.Throws(ex, () => xs.ForEach(x => { Assert.True(false); }));
        }

        [Test]
        public void ForEach_Index_Throw()
        {
            var ex = new Exception();

            var xs = Observable.Throw<int>(ex);

            ReactiveAssert.Throws(ex, () => xs.ForEach((x, i) => { Assert.True(false); }));
        }

        [Test]
        public void ForEach_SomeData()
        {
            var lstX = new List<int>();
            Observable.Range(10, 10).ForEach(x => lstX.Add(x));
            Assert.True(lstX.SequenceEqual(Enumerable.Range(10, 10)));
        }

        [Test]
        public void ForEach_Index_SomeData()
        {
            var lstX = new List<int>();
            var lstI = new List<int>();
            Observable.Range(10, 10).ForEach((x, i) => { lstX.Add(x); lstI.Add(i); });
            Assert.True(lstX.SequenceEqual(Enumerable.Range(10, 10)));
            Assert.True(lstI.SequenceEqual(Enumerable.Range(0, 10)));
        }

        [Test]
        public void ForEach_OnNextThrows()
        {
            var ex = new Exception();

            var xs = Observable.Range(0, 10);

            ReactiveAssert.Throws(ex, () => xs.ForEach(x => { throw ex; }));
        }

        [Test]
        public void ForEach_Index_OnNextThrows()
        {
            var ex = new Exception();

            var xs = Observable.Range(0, 10);

            ReactiveAssert.Throws(ex, () => xs.ForEach((x, i) => { throw ex; }));
        }

    }
}
