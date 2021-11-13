using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace Microsoft.Reactive.Testing {
    public partial class ReactiveTest {
        [SetUp]
        public void BeforeLogging() {
            ThreadPool.GetAvailableThreads(out var workerThreads1, out var completionPortThreads1);
            ThreadPool.GetMaxThreads(out var workerThreads2, out var completionPortThreads2);
            var memUsageFromGc = GC.GetTotalMemory(false);
            var memUsage = Process.GetCurrentProcess().PrivateMemorySize64;

            UnityEngine.Debug.LogFormat("Memory usage from GC: {0} bytes; Memory usage: {1} bytes; Thread Worker: {2} / {3} ; IoCompletion: {4} / {5}", memUsageFromGc, memUsage, workerThreads1, workerThreads2, completionPortThreads1, completionPortThreads2);
        }
    }
}
