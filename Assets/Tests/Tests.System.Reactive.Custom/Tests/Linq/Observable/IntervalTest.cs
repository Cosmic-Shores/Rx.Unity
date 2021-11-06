// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using Microsoft.Reactive.Testing;
using NUnit.Framework;
using UniRx.Tests;
using Rx.Unity;

namespace ReactiveTests.Tests {
    public partial class IntervalTest : ReactiveTest {
        [SetUp]
        public void Init() {
            TestUtil.SetSchedulerForImport();
        }

        [TearDown]
        public void Dispose() {
            ReactiveUnity.SetupPatches();
        }
    }
}
