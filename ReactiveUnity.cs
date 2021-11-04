using Rx.Unity.InternalUtil;
using UnityEngine;
using Rx.Unity.Concurrency;
using System.Reactive.PlatformServices;
using Rx.Data;
using System.Reactive.Concurrency;

namespace Rx.Unity {
    public static class ReactiveUnity {
        public static void SetupPatches() {
            InitSchedulerDefaults();
            InitCurrentPlatformEnlightenmentProvider();
            AddUnityEqualityComparer<Vector2>();
            AddUnityEqualityComparer<Vector3>();
            AddUnityEqualityComparer<Vector4>();
            AddUnityEqualityComparer<Color>();
            AddUnityEqualityComparer<Color32>();
            AddUnityEqualityComparer<Rect>();
            AddUnityEqualityComparer<Bounds>();
            AddUnityEqualityComparer<Quaternion>();
            AddUnityEqualityComparer<Vector2Int>();
            AddUnityEqualityComparer<Vector3Int>();
            AddUnityEqualityComparer<RangeInt>();
            AddUnityEqualityComparer<RectInt>();
            AddUnityEqualityComparer<BoundsInt>();
        }

        private static void InitSchedulerDefaults() {
            SchedulerDefaults.TimeBasedOperations = UnityMainThreadScheduler.Instance;
#if WEB_GL
            SchedulerDefaults.AsyncConversions = UnityMainThreadScheduler.Instance;
#else
            SchedulerDefaults.AsyncConversions = DefaultScheduler.Instance;
            SchedulerDefaults.AsyncConversions = ThreadPoolOnlyScheduler.Instance;
#endif
        }

        private static void InitCurrentPlatformEnlightenmentProvider() {
            PlatformEnlightenmentProvider.Current = new UnityEnlightenmentProvider();
        }

        private static void AddUnityEqualityComparer<T>() {
            var comparer = UnityEqualityComparer.GetDefault<T>();
            ReactiveProperty<T>.defaultEqualityComparer = comparer;
            ReadOnlyReactiveProperty<T>.defaultEqualityComparer = comparer;
        }
    }
}
