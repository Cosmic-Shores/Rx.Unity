using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Collections;
using UnityEngine;
using System;
using Rx.Extendibility.Disposables;

namespace Rx.Unity.Concurrency {
    /// <summary>
    /// Represents an object that schedules units of work on the Unity main thread.
    /// </summary>
    /// <seealso cref="Instance">Singleton instance of this type exposed through this static property.</seealso>
    public sealed class UnityMainThreadScheduler : LocalScheduler, ISchedulerPeriodic {
        private static readonly Lazy<UnityMainThreadScheduler> _lazyDefault = new Lazy<UnityMainThreadScheduler>(static () => new UnityMainThreadScheduler());
        private readonly Action<object> _scheduleAction;

        public UnityMainThreadScheduler() {
            MainThreadDispatcher.Initialize();
            _scheduleAction = new Action<object>(Schedule);
        }

        /// <summary>
        /// Gets the singleton instance of the Unity main thread scheduler.
        /// </summary>
        public static UnityMainThreadScheduler Instance => _lazyDefault.Value;

        /// <summary>
        /// Schedules an action to be executed.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
        /// <param name="state">State passed to the action to be executed.</param>
        /// <param name="action">Action to be executed.</param>
        /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var workItem = new WorkItem<TState>(this, state, action);
            MainThreadDispatcher.Post(_scheduleAction, (Action)workItem.Run);
            return workItem;
        }

        /// <summary>
        /// Schedules an action to be executed after dueTime.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
        /// <param name="state">State passed to the action to be executed.</param>
        /// <param name="action">Action to be executed.</param>
        /// <param name="dueTime">Relative time after which to execute the action.</param>
        /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dt = Scheduler.Normalize(dueTime);
            return dt.Ticks == 0
                ? Schedule(state, action)
                : ScheduleSlow(state, dt, action);
        }

        /// <summary>
        /// Schedules a periodic piece of work.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
        /// <param name="state">Initial state passed to the action upon the first iteration.</param>
        /// <param name="period">Period for running the work periodically.</param>
        /// <param name="action">Action to be executed, potentially updating the state.</param>
        /// <returns>The disposable object used to cancel the scheduled recurring action (best effort).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is less or equal <see cref="TimeSpan.Zero"/>.</exception>
        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action) {
            if (period <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(period));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var periodicallyScheduledWorkItem = new PeriodicallyScheduledWorkItem<TState>(state, action);
            MainThreadDispatcher.SendStartCoroutine(PeriodicAction(period, periodicallyScheduledWorkItem));
            return periodicallyScheduledWorkItem;
        }

        private IDisposable ScheduleSlow<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) {
            var workItem = new WorkItem<TState>(this, state, action);
            MainThreadDispatcher.SendStartCoroutine(DelayAction(dueTime, workItem));
            return workItem;
        }

        private static IEnumerator DelayAction(TimeSpan dueTime, IWorkItem workItem) {
            // zero == every frame
            if (dueTime == TimeSpan.Zero) {
                yield return null; // not immediately, run next frame
            }
            else {
                yield return new WaitForSeconds((float)dueTime.TotalSeconds);
            }

            if (workItem.IsDisposed) yield break;
            MainThreadDispatcher.UnsafeSend(workItem.Run);
        }

        private static IEnumerator PeriodicAction<TState>(TimeSpan period, PeriodicallyScheduledWorkItem<TState> periodicallyScheduledWorkItem) {
            if (period == TimeSpan.Zero) {
                while (true) {
                    yield return null; // not immediately, run next frame
                    if (periodicallyScheduledWorkItem.IsDisposed) yield break;

                    MainThreadDispatcher.UnsafeSend(periodicallyScheduledWorkItem.Run);
                }
            }
            else {
                var seconds = (float)(period.TotalMilliseconds / 1000d);
                var yieldInstruction = new WaitForSeconds(seconds); // cache single instruction object

                while (true) {
                    yield return yieldInstruction;
                    if (periodicallyScheduledWorkItem.IsDisposed) yield break;

                    MainThreadDispatcher.UnsafeSend(periodicallyScheduledWorkItem.Run);
                }
            }
        }

        private static void Schedule(object state) => ((Action)state)();

        internal interface IWorkItem : IDisposable {
            BooleanDisposable CancelQueueDisposable { get; }
            bool IsDisposed { get; }
            void Run();
        }

        internal sealed class WorkItem<TState> : IWorkItem {
            private SingleAssignmentDisposableValue _cancelRunDisposable;

            private readonly TState _state;
            private readonly IScheduler _scheduler;
            private readonly Func<IScheduler, TState, IDisposable> _action;
            public BooleanDisposable CancelQueueDisposable { get; } = new BooleanDisposable();
            public bool IsDisposed => CancelQueueDisposable.IsDisposed || _cancelRunDisposable.IsDisposed;

            public WorkItem(IScheduler scheduler, TState state, Func<IScheduler, TState, IDisposable> action) {
                _state = state;
                _action = action;
                _scheduler = scheduler;
            }

            public void Run() {
                if (!IsDisposed)
                    _cancelRunDisposable.Disposable = _action(_scheduler, _state);
            }

            public void Dispose() {
                CancelQueueDisposable.Dispose();
                _cancelRunDisposable.Dispose();
            }
        }

        internal sealed class PeriodicallyScheduledWorkItem<TState> : IWorkItem {
            private readonly Func<TState, TState> _action;

            private TState _state;

            public BooleanDisposable CancelQueueDisposable { get; } = new BooleanDisposable();

            public bool IsDisposed => CancelQueueDisposable.IsDisposed;

            public PeriodicallyScheduledWorkItem(TState state, Func<TState, TState> action) {
                _state = state;
                _action = action;
            }

            public void Run() {
                _state = _action(_state);
            }

            public void Dispose() => CancelQueueDisposable.Dispose();
        }
    }
}
