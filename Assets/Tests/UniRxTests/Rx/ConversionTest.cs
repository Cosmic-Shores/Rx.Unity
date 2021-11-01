using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Rx.Data.Linq;
using NUnit.Framework;

namespace UniRx.Tests.Operators
{
    
    public class ConversionTest
    {
        [Test]
        public void AsObservable()
        {
            Observable.Range(1, 10).AsObservable().ToArrayWait().Is(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }

        [Test]
        public void ToObservable()
        {
            Enumerable.Range(1, 3).ToObservable(Scheduler.CurrentThread).ToArrayWait().Is(1, 2, 3);
            Enumerable.Range(1, 3).ToObservable(Scheduler.ThreadPool).ToArrayWait().Is(1, 2, 3);
            Enumerable.Range(1, 3).ToObservable(Scheduler.Immediate).ToArrayWait().Is(1, 2, 3);
        }

        [Test]
        public void OfType()
        {
            var subject = new Subject<object>();

            var list = new List<int>();
            subject.OfType<int>().Subscribe(x => list.Add(x));

            subject.OnNext(1);
            subject.OnNext(2);
            subject.OnNext("hogehoge");
            subject.OnNext(3);

            list.Is(1, 2, 3);
        }
    }
}
