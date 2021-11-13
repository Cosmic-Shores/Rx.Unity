using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace Microsoft.Reactive.Testing {
    public partial class ReactiveTest {
        [SetUp]
        public void BeforeLogging() {
            ThreadPool.GetAvailableThreads(out var workerThreads1, out var completionPortThreads1);
            ThreadPool.GetMaxThreads(out var workerThreads2, out var completionPortThreads2);
            var memUsage = GC.GetTotalMemory(false) / 1000000d;
            UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Memory usage: {0:N2} MB; Thread Worker: {1} / {2} ; IoCompletion: {3} / {4}", memUsage, workerThreads1, workerThreads2, completionPortThreads1, completionPortThreads2);
        }
    }
}
