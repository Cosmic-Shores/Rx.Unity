// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using Microsoft.Reactive.Testing;
using System.Reactive.Linq.ObservableImpl;
using System;

namespace ReactiveTests.Tests {
    public partial class ThrottleTest : ReactiveTest {
        private static void Throttle_Simple_Aot() {
            _ = new TestScheduler().ScheduleAbsolute<((Throttle<int>._, ulong), Action<(Throttle<int>._, ulong)>)>(((null, default), null), 0L, null);
            _ = nameof(Throttle_Simple_Aot);
        }
    }
}
