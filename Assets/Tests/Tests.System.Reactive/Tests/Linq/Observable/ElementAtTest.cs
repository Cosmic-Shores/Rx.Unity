﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using ReactiveTests.Dummies;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ReactiveTests.Tests
{
    public class ElementAtTest : ReactiveTest
    {

        [Test]
        public void ElementAt_ArgumentChecking()
        {
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.ElementAt(default(IObservable<int>), 2));
            ReactiveAssert.Throws<ArgumentOutOfRangeException>(() => Observable.ElementAt(DummyObservable<int>.Instance, -1));
        }

        [Test]
        public void ElementAt_First()
        {
            var scheduler = new TestScheduler();

            var xs = scheduler.CreateHotObservable(
                OnNext(280, 42),
                OnNext(360, 43),
                OnNext(470, 44),
                OnCompleted<int>(600)
            );

            var res = scheduler.Start(() =>
                xs.ElementAt(0)
            );

            res.Messages.AssertEqual(
                OnNext(280, 42),
                OnCompleted<int>(280)
            );

            xs.Subscriptions.AssertEqual(
                Subscribe(200, 280)
            );
        }

        [Test]
        public void ElementAt_Other()
        {
            var scheduler = new TestScheduler();

            var xs = scheduler.CreateHotObservable(
                OnNext(280, 42),
                OnNext(360, 43),
                OnNext(470, 44),
                OnCompleted<int>(600)
            );

            var res = scheduler.Start(() =>
                xs.ElementAt(2)
            );

            res.Messages.AssertEqual(
                OnNext(470, 44),
                OnCompleted<int>(470)
            );

            xs.Subscriptions.AssertEqual(
                Subscribe(200, 470)
            );
        }

        [Test]
        public void ElementAt_OutOfRange()
        {
            var scheduler = new TestScheduler();

            var xs = scheduler.CreateHotObservable(
                OnNext(280, 42),
                OnNext(360, 43),
                OnNext(470, 44),
                OnCompleted<int>(600)
            );

            var res = scheduler.Start(() =>
                xs.ElementAt(3)
            );

            res.Messages.AssertEqual(
                OnError<int>(600, e => e is ArgumentOutOfRangeException)
            );
        }

        [Test]
        public void ElementAt_Error()
        {
            var scheduler = new TestScheduler();

            var ex = new Exception();

            var xs = scheduler.CreateHotObservable(
                OnNext(280, 42),
                OnNext(360, 43),
                OnError<int>(420, ex)
            );

            var res = scheduler.Start(() =>
                xs.ElementAt(3)
            );

            res.Messages.AssertEqual(
                OnError<int>(420, ex)
            );

            xs.Subscriptions.AssertEqual(
                Subscribe(200, 420)
            );
        }

    }
}
