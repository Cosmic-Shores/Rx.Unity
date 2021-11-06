// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq.ObservableImpl;
using System.Reactive;

namespace ReactiveTests.Tests {
    public partial class EventLoopSchedulerTest {
        private static void EventLoop_ScheduleActionDue_Aot() {
            _ = CurrentThreadScheduler.Instance.Schedule<(Func<(BasicProducer<int>, SingleAssignmentDisposable, IObserver<int>), IDisposable>, (BasicProducer<int>, SingleAssignmentDisposable, IObserver<int>))>(
                (null, (null, null, null)), TimeSpan.Zero, null);
            _ = nameof(EventLoop_ScheduleActionDue_Aot);
        }

        private static void EventLoop_ScheduleActionDueNested_Aot() {
            _ = CurrentThreadScheduler.Instance.Schedule<(Action<(Producer<int, Where<int>.Predicate._>, Where<int>.Predicate._)>, (Producer<int, Where<int>.Predicate._>, Where<int>.Predicate._))>(
                (null, (null, null)), TimeSpan.Zero, null);
            _ = nameof(EventLoop_ScheduleActionDueNested_Aot);
        }
    }
}
