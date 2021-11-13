using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace Microsoft.Reactive.Testing {
    public partial class ReactiveTest {
        [SetUp]
        public void BeforeLogging() {
            ThreadPool.GetAvailableThreads(out var workerThreads1, out var completionPortThreads1);
            ThreadPool.GetMaxThreads(out var workerThreads2, out var completionPortThreads2);
            var memUsage = Process.GetCurrentProcess().PrivateMemorySize64;
            UnityEngine.Debug.LogFormat("Memory usage: {0} bytes; Thread Worker: {1} / {2} ; IoCompletion: {3} / {4}", memUsage, workerThreads1, workerThreads2, completionPortThreads1, completionPortThreads2);
        }
    }
}
