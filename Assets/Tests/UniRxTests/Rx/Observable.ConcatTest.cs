﻿using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Rx.Unity.Linq;
using Rx.Data.Linq;
using System.Reactive.Concurrency;

namespace UniRx.Tests
{
    
    public class ObservableConcatTest
    {
        [Test]
        public void Concat()
        {
            var a = Observable.Range(1, 5, Scheduler.ThreadPool);
            var b = Observable.Range(10, 3, Scheduler.ThreadPool);
            var c = Observable.Return(300, Scheduler.ThreadPool);

            Observable.Concat(a, b, c).ToArray().Wait().Is(1, 2, 3, 4, 5, 10, 11, 12, 300);
        }

        [Test]
        public void Zip()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            a.Zip(b, (x, y) => new { x, y }).Materialize().Subscribe(x => l.Add(x));

            a.OnNext(1000);
            b.OnNext(2000);

            a.OnCompleted();

            l.Count.Is(1); // OnNext

            a.OnNext(1001);
            l.Count.Is(1);

            b.OnNext(5);
            l.Count.Is(2); // Completed!

            l[1].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void Zip2()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            a.Zip(b, (x, y) => new { x, y }).Materialize().Subscribe(x => l.Add(x));

            a.OnNext(1000);
            b.OnNext(2000);

            a.OnCompleted();

            l.Count.Is(1); // OnNext

            b.OnCompleted(); // Completed!

            l.Count.Is(2); // Completed!
            l[1].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void ZipMulti()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            Observable.Zip(a, b).Select(xs => new { x = xs[0], y = xs[1] }).Materialize().Subscribe(x => l.Add(x));


            a.OnNext(1000);
            b.OnNext(2000);

            a.OnCompleted();

            l.Count.Is(1); // OnNext

            a.OnNext(1001);
            l.Count.Is(1);

            b.OnNext(5);
            l.Count.Is(2); // Completed!

            l[1].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void ZipMulti2()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            Observable.Zip(a, b).Select(xs => new { x = xs[0], y = xs[1] }).Materialize().Subscribe(x => l.Add(x));

            a.OnNext(1000);
            b.OnNext(2000);

            a.OnCompleted();

            l.Count.Is(1); // OnNext

            b.OnCompleted(); // Completed!

            l.Count.Is(2); // Completed!
            l[1].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void ZipNth()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();
            var c = new Subject<int>();

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0, z = 0 })).ToList();
            Observable.Zip(a, b, c, (x, y, z) => new { x, y, z }).Materialize().Subscribe(x => l.Add(x));

            a.OnNext(1000);
            b.OnNext(2000);
            c.OnNext(3000);

            l.Count.Is(1); // OnNext

            a.OnCompleted();

            b.OnNext(1001);
            l.Count.Is(1);

            b.OnCompleted();
            l.Count.Is(1);

            c.OnCompleted();
            l.Count.Is(2); // Completed!

            l[1].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void CombineLatest()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            a.CombineLatest(b, (x, y) => new { x, y }).Materialize().Subscribe(x => l.Add(x));

            a.OnNext(1000);
            b.OnNext(2000);
            l[0].Value.Is(new { x = 1000, y = 2000 });

            b.OnNext(3000);
            l[1].Value.Is(new { x = 1000, y = 3000 });

            a.OnNext(5000);
            l[2].Value.Is(new { x = 5000, y = 3000 });

            a.OnCompleted();
            l.Count.Is(3);

            a.OnNext(1001);
            l.Count.Is(3);

            b.OnNext(5);
            l[3].Value.Is(new { x = 5000, y = 5 });
            b.OnNext(500);
            l[4].Value.Is(new { x = 5000, y = 500 });

            b.OnCompleted();
            l[5].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void CombineLatest2()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            a.CombineLatest(b, (x, y) => new { x, y }).Materialize().Subscribe(x => l.Add(x));

            b.OnNext(2000);
            a.OnCompleted();

            l.Count.Is(0);

            b.OnNext(30);
            l.Count.Is(1);
            l[0].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void CombineLatest3()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            a.CombineLatest(b, (x, y) => new { x, y }).Materialize().Subscribe(x => l.Add(x));

            a.OnNext(2000);
            b.OnCompleted();

            l.Count.Is(0);

            a.OnNext(30);
            l.Count.Is(1);
            l[0].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void CombineLatest4()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            a.CombineLatest(b, (x, y) => new { x, y }).Materialize().Subscribe(x => l.Add(x));

            b.OnCompleted();
            l.Count.Is(0);

            a.OnNext(30);
            l.Count.Is(1);
            l[0].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void CombineLatestMulti()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            Observable.CombineLatest(a, b).Select((xs) => new { x = xs[0], y = xs[1] }).Materialize().Subscribe(x => l.Add(x));

            a.OnNext(1000);
            b.OnNext(2000);
            l[0].Value.Is(new { x = 1000, y = 2000 });

            b.OnNext(3000);
            l[1].Value.Is(new { x = 1000, y = 3000 });

            a.OnNext(5000);
            l[2].Value.Is(new { x = 5000, y = 3000 });

            a.OnCompleted();
            l.Count.Is(3);

            a.OnNext(1001);
            l.Count.Is(3);

            b.OnNext(5);
            l[3].Value.Is(new { x = 5000, y = 5 });
            b.OnNext(500);
            l[4].Value.Is(new { x = 5000, y = 500 });

            b.OnCompleted();
            l[5].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void CombineLatestMulti2()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            Observable.CombineLatest(a, b).Select((xs) => new { x = xs[0], y = xs[1] }).Materialize().Subscribe(x => l.Add(x));

            b.OnNext(2000);
            a.OnCompleted();

            l.Count.Is(0);

            b.OnNext(30);
            l.Count.Is(1);
            l[0].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void CombineLatestMulti3()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            Observable.CombineLatest(a, b).Select((xs) => new { x = xs[0], y = xs[1] }).Materialize().Subscribe(x => l.Add(x));

            a.OnNext(2000);
            b.OnCompleted();

            l.Count.Is(0);

            a.OnNext(30);
            l.Count.Is(1);
            l[0].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void CombineLatestMulti4()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            a.OnNext(10);
            b.OnNext(20);

            var l = Enumerable.Empty<Unit>().Select(_ => Notification.CreateOnNext(new { x = 0, y = 0 })).ToList();
            Observable.CombineLatest(a, b).Select((xs) => new { x = xs[0], y = xs[1] }).Materialize().Subscribe(x => l.Add(x));

            b.OnCompleted();
            l.Count.Is(0);

            a.OnNext(30);
            l.Count.Is(1);
            l[0].Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void WithLatestFrom()
        {
            var a = new Subject<int>();
            var b = new Subject<int>();

            var record = a.WithLatestFrom(b, (x, y) => new { x, y }).Record();

            b.OnNext(0); // 50
            b.OnNext(1); // 100
            a.OnNext(0); // 140
            b.OnNext(2); // 150
            b.OnNext(3); // 200
            b.OnNext(4); // 250
            a.OnNext(1); // 280
            b.OnNext(5); // 300
            b.OnNext(6); // 350
            b.OnNext(7); // 400
            a.OnNext(2); // 420
            b.OnNext(8); // 450
            b.OnNext(9); // 500
            b.OnNext(10); // 550
            a.OnNext(3); // 600

            record.Values.Is(
                new { x = 0, y = 1 },
                new { x = 1, y = 4 },
                new { x = 2, y = 7 },
                new { x = 3, y = 10 });

            a.OnCompleted();
            record.Notifications.Last().Kind.Is(NotificationKind.OnCompleted);
        }

        [Test]
        public void StartWith()
        {
            var seq = Observable.Range(1, 5);

            seq.StartWith(100).ToArray().Wait().Is(100, 1, 2, 3, 4, 5);
            seq.StartWith(100, 1000, 10000).ToArray().Wait().Is(100, 1000, 10000, 1, 2, 3, 4, 5);
            seq.StartWith(Enumerable.Range(100, 3)).ToArray().Wait().Is(100, 101, 102, 1, 2, 3, 4, 5);

            seq.StartWith(Scheduler.ThreadPool, 100).ToArray().Wait().Is(100, 1, 2, 3, 4, 5);
            seq.StartWith(Scheduler.ThreadPool, 100, 1000, 10000).ToArray().Wait().Is(100, 1000, 10000, 1, 2, 3, 4, 5);
            seq.StartWith(Scheduler.ThreadPool, Enumerable.Range(100, 3)).ToArray().Wait().Is(100, 101, 102, 1, 2, 3, 4, 5);
        }

        [Test]
        public void Merge()
        {
            var s1 = new Subject<int>();
            var s2 = new Subject<int>();
            var s3 = new Subject<int>();

            var list = new List<int>();
            var complete = false;
            Observable.Merge(s1, s2, s3).Subscribe(list.Add, () => complete = true);

            s1.OnNext(10);
            s1.OnNext(20);
            s3.OnNext(100);
            s2.OnNext(50);

            list.Is(10, 20, 100, 50);

            list.Clear();

            s2.OnCompleted();

            s3.OnNext(500);
            complete.IsFalse();
            s1.OnCompleted();
            s3.OnCompleted();
            complete.IsTrue();
        }


        [Test]
        public void MergeConcurrent()
        {
            var s1 = new Subject<int>();
            var s2 = new Subject<int>();
            var s3 = new Subject<int>();

            var list = new List<int>();
            var complete = false;
            Observable.Merge(new[] { s1, s2, s3 }, maxConcurrent: 2).Subscribe(list.Add, () => complete = true);

            s1.OnNext(10);
            s1.OnNext(20);
            s3.OnNext(100); // in queue
            s2.OnNext(50);

            list.Is(10, 20, 50);

            list.Clear();

            s2.OnCompleted();

            s3.OnNext(500); // dequeued
            list.Is(500);
            complete.IsFalse();

            s1.OnCompleted();
            s3.OnCompleted();
            complete.IsTrue();
        }

        [Test]
        public void Switch()
        {
            var source = new Subject<IObservable<int>>();

            var list = new List<int>();
            source.Switch().Subscribe(list.Add);

            var s1 = new Subject<int>();
            source.OnNext(s1);

            s1.OnNext(100);
            s1.OnNext(2000);

            var s2 = new Subject<int>();
            s1.HasObservers.IsTrue();
            source.OnNext(s2);
            s1.OnNext(3000); // do nothing
            s1.HasObservers.IsFalse();

            s2.OnNext(5000);

            source.OnCompleted();

            s2.OnNext(900000);

            list.Is(100, 2000, 5000, 900000);
            s2.OnCompleted();
        }
    }
}
