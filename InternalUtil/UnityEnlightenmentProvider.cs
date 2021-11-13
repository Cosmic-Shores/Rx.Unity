using System.Reactive.Concurrency;
using System.Reactive.PlatformServices;
using Rx.Unity.Concurrency;

namespace Rx.Unity.InternalUtil {
    sealed class UnityEnlightenmentProvider : CurrentPlatformEnlightenmentProvider {
        public override T GetService<T>(object[] args) {
            if (typeof(T) == typeof(IScheduler) && args != null && ((string)args[0]) == "ThreadPool")
                return (T)(object)ThreadPoolOnlyScheduler.Instance;
            return base.GetService<T>(args);
        }
    }
}
