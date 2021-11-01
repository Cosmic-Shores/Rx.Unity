// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Rx.Unity.Tests.Helper;

namespace ReactiveTests.Tests
{
    public partial class AwaitTest : ReactiveTest
    {
        [Test]
        public void Await_ArgumentChecking()
        {
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.GetAwaiter(default(IObservable<int>)));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.GetAwaiter<int>(default));

            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.GetAwaiter(Observable.Empty<int>()).OnCompleted(null));
        }

        [Test]
        public void Await()
        {
            SynchronizationContext.SetSynchronizationContext(null);

            var scheduler = new TestScheduler();

            var xs = scheduler.CreateHotObservable(
                OnNext(20, -1),
                OnNext(150, 0),
                OnNext(220, 1),
                OnNext(290, 2),
                OnNext(340, 3),
                OnCompleted<int>(410)
            );

            var awaiter = default(AsyncSubject<int>);
            var result = default(int);
            var t = long.MaxValue;

            scheduler.ScheduleAbsolute(100, () => awaiter = xs.GetAwaiter());
            scheduler.ScheduleAbsolute(200, () => awaiter.OnCompleted(() => { t = scheduler.Clock; result = awaiter.GetResult(); }));

            scheduler.Start();

            XunitAssert.Equal(410, t);
            XunitAssert.Equal(3, result);

            xs.Subscriptions.AssertEqual(
                Subscribe(100)
            );
        }

        [Test]
        public void Await_Connectable()
        {
            SynchronizationContext.SetSynchronizationContext(null);

            var scheduler = new TestScheduler();

            var s = default(long);

            var xs = Observable.Create<int>(observer =>
            {
                s = scheduler.Clock;

                return StableCompositeDisposable.Create(
                    scheduler.ScheduleAbsolute(250, () => { observer.OnNext(42); }),
                    scheduler.ScheduleAbsolute(260, () => { observer.OnCompleted(); })
                );
            });

            var ys = xs.Publish();

            var awaiter = default(AsyncSubject<int>);
            var result = default(int);
            var t = long.MaxValue;

            scheduler.ScheduleAbsolute(100, () => awaiter = ys.GetAwaiter());
            scheduler.ScheduleAbsolute(200, () => awaiter.OnCompleted(() => { t = scheduler.Clock; result = awaiter.GetResult(); }));

            scheduler.Start();

            XunitAssert.Equal(100, s);
            XunitAssert.Equal(260, t);
            XunitAssert.Equal(42, result);
        }

        [Test]
        public void Await_Error()
        {
            SynchronizationContext.SetSynchronizationContext(null);

            var scheduler = new TestScheduler();

            var ex = new Exception();

            var xs = scheduler.CreateHotObservable(
                OnNext(20, -1),
                OnNext(150, 0),
                OnNext(220, 1),
                OnNext(290, 2),
                OnNext(340, 3),
                OnError<int>(410, ex)
            );

            var awaiter = default(AsyncSubject<int>);
            var t = long.MaxValue;

            scheduler.ScheduleAbsolute(100, () => awaiter = xs.GetAwaiter());
            scheduler.ScheduleAbsolute(200, () => awaiter.OnCompleted(() => { t = scheduler.Clock; ReactiveAssert.Throws(ex, () => awaiter.GetResult()); }));

            scheduler.Start();

            XunitAssert.Equal(410, t);

            xs.Subscriptions.AssertEqual(
                Subscribe(100)
            );
        }

        [Test]
        public void Await_Never()
        {
            SynchronizationContext.SetSynchronizationContext(null);

            var scheduler = new TestScheduler();

            var xs = scheduler.CreateHotObservable(
                OnNext(20, -1),
                OnNext(150, 0),
                OnNext(220, 1),
                OnNext(290, 2),
                OnNext(340, 3)
            );

            var awaiter = default(AsyncSubject<int>);
            var hasValue = default(bool);
            var t = long.MaxValue;

            scheduler.ScheduleAbsolute(100, () => awaiter = xs.GetAwaiter());
            scheduler.ScheduleAbsolute(200, () => awaiter.OnCompleted(() => { t = scheduler.Clock; awaiter.GetResult(); hasValue = true; }));

            scheduler.Start();

            XunitAssert.Equal(long.MaxValue, t);
            Assert.False(hasValue);

            xs.Subscriptions.AssertEqual(
                Subscribe(100)
            );
        }

        [Test]
        public void Await_Empty()
        {
            SynchronizationContext.SetSynchronizationContext(null);

            var scheduler = new TestScheduler();

            var xs = scheduler.CreateHotObservable(
                OnCompleted<int>(300)
            );

            var awaiter = default(AsyncSubject<int>);
            var t = long.MaxValue;

            scheduler.ScheduleAbsolute(100, () => awaiter = xs.GetAwaiter());
            scheduler.ScheduleAbsolute(200, () => awaiter.OnCompleted(() => { t = scheduler.Clock; ReactiveAssert.Throws<InvalidOperationException>(() => awaiter.GetResult()); }));

            scheduler.Start();

            XunitAssert.Equal(300, t);

            xs.Subscriptions.AssertEqual(
                Subscribe(100)
            );
        }
    }
}
