using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using Rx.Unity.Concurrency;
using Rx.Unity.Linq.Observables;
using Rx.Unity.Triggers;
using System.Threading;
using UnityEngine;
using System;

namespace Rx.Unity.Linq {
    public static class UnityObservable {
        public static IObservable<long> EveryUpdate() => new UnityInterval(FrameCountType.Update);

        public static IObservable<long> EveryFixedUpdate() => new UnityInterval(FrameCountType.FixedUpdate);

        public static IObservable<long> EveryEndOfFrame() => new UnityInterval(FrameCountType.EndOfFrame);

        internal static IEnumerator EveryCycleCore(IObserver<long> observer, CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested) yield break;
            var count = 0L;
            while (true) {
                yield return null;
                if (cancellationToken.IsCancellationRequested) yield break;

                observer.OnNext(count++);
            }
        }

        public static IObservable<T> ObserveOnMainThread<T>(this IObservable<T> source) {
            return source.ObserveOn(UnityMainThreadScheduler.Instance);
        }

        public static IObservable<T> SubscribeOnMainThread<T>(this IObservable<T> source) {
            return source.SubscribeOn(UnityMainThreadScheduler.Instance);
        }

        /// <summary>
        /// Buffer elements in during target frame counts.
        /// </summary>
        public static IObservable<IList<T>> BatchFrame<T>(this IObservable<T> source, int frameCount = 0, FrameCountType frameCountType = FrameCountType.EndOfFrame) {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (frameCount < 0) throw new ArgumentException("frameCount must be >= 0, frameCount:" + frameCount, nameof(frameCount));
            return new BatchFrame<T>(source, frameCount, frameCountType);
        }

        /// <summary>
        /// Emits the last value of each window if any. Each window is reset using the specified frameCount and frameCountType.
        /// It always emits on the main thread.
        /// (Same as `BatchFrame(frameCount, frameCountType).Select(x => x.Last())`; Default parameter for frameCountType differs!)
        /// </summary>
        public static IObservable<T> SampleFrame<T>(this IObservable<T> source, int frameCount = 0, FrameCountType frameCountType = FrameCountType.Update) {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (frameCount < 0) throw new ArgumentException("frameCount must be >= 0, frameCount:" + frameCount, nameof(frameCount));
            return new DelayFrame<T>(source, frameCount, frameCountType);
        }

        public static IObservable<T> DelayFrame<T>(this IObservable<T> source, int frameCount, FrameCountType frameCountType = FrameCountType.Update) {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (frameCount < 0) throw new ArgumentException("frameCount must be >= 0, frameCount:" + frameCount, nameof(frameCount));
            return new DelayFrame<T>(source, frameCount, frameCountType);
        }

        public static IObservable<T> TakeUntilDestroy<T>(this IObservable<T> source, Component target) {
            return source.TakeUntil(target.OnDestroyAsObservable());
        }

        public static IObservable<T> TakeUntilDestroy<T>(this IObservable<T> source, GameObject target) {
            return source.TakeUntil(target.OnDestroyAsObservable());
        }
    }
}
