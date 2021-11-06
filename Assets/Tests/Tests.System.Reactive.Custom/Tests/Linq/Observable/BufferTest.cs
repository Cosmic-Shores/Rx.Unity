// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System;
using Microsoft.Reactive.Testing;
using System.Reactive.Linq.ObservableImpl;
using System.Reactive.Subjects;

namespace ReactiveTests.Tests {
    public partial class BufferTest : ReactiveTest {
        private static void BufferWithCount_Disposed_Aot() {
            _ = new TestScheduler().ScheduleAbsolute<((Buffer<int>.Ferry._, int), Action<(Buffer<int>.Ferry._, int)>)>(((null, default), null), 0L, null);
            _ = nameof(BufferWithCount_Disposed_Aot);
        }

        private static void BufferWithTime_TickWhileOnCompleted_Aot() {
            _ = new TestScheduler().ScheduleAbsolute<((Window<int>.Ferry._, Subject<int>), Action<(Window<int>.Ferry._, Subject<int>)>)>(((null, null), null), 0L, null);
            _ = nameof(BufferWithTime_TickWhileOnCompleted_Aot);
        }
    }
}
