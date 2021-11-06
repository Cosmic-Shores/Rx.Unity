// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using Microsoft.Reactive.Testing;
using System.Reactive.Linq.ObservableImpl;
using System;

namespace ReactiveTests.Tests {
    public partial class WindowTest : ReactiveTest {
        private static void WindowWithTimeOrCount_Disposed_Aot() {
            _ = new TestScheduler().ScheduleAbsolute<((Buffer<int>.TimeSliding._, bool, bool), Action<(Buffer<int>.TimeSliding._, bool, bool)>)>(((null, default, default), null), 0L, null);
            _ = nameof(WindowWithTimeOrCount_Disposed_Aot);
        }
    }
}
