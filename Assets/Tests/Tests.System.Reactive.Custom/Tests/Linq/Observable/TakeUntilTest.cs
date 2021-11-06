// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using Microsoft.Reactive.Testing;
using System.Reactive.Linq.ObservableImpl;
using System;

namespace ReactiveTests.Tests {
    public partial class TakeUntilTest : ReactiveTest {
        private static void TakeUntil_Twice2_Aot() {
            _ = new TestScheduler().ScheduleAbsolute<(TakeUntil<int>._, Action<TakeUntil<int>._>)>((null, null), 0L, null);
            _ = nameof(TakeUntil_Twice2_Aot);
        }
    }
}
