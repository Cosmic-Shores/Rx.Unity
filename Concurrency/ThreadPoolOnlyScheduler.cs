using System;
using System.Reactive.Concurrency;

namespace Rx.Unity.Concurrency {
    /// <summary>
    /// Represents an object that schedules units of work on the CLR thread pool. (This one will never fall back to the NewThreadScheduler unlike the ThreadPoolScheduler)
    /// </summary>
    /// <seealso cref="Instance">Singleton instance of this type exposed through this static property.</seealso>
    public sealed class ThreadPoolOnlyScheduler : LocalScheduler, ISchedulerPeriodic {
        private static readonly Lazy<ThreadPoolOnlyScheduler> _lazyDefault = new Lazy<ThreadPoolOnlyScheduler>(static () => new ThreadPoolOnlyScheduler());
        private readonly ThreadPoolScheduler _innerScheduler;

        public ThreadPoolOnlyScheduler() {
            _innerScheduler = ThreadPoolScheduler.Instance;
        }

        /// <summary>
        /// Gets the singleton instance of the thread pool only scheduler. (This one will never fall back to the NewThreadScheduler unlike the ThreadPoolScheduler)
        /// </summary>
        public static ThreadPoolOnlyScheduler Instance => _lazyDefault.Value;

        /// <summary>
        /// Schedules an action to be executed.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
        /// <param name="state">State passed to the action to be executed.</param>
        /// <param name="action">Action to be executed.</param>
        /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action) {
            return _innerScheduler.Schedule(state, action);
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
            return _innerScheduler.Schedule(state, dueTime, action);
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is less than zero.</exception>
        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action) {
            return _innerScheduler.SchedulePeriodic(state, period, action);
        }

        public override IStopwatch StartStopwatch() => _innerScheduler.StartStopwatch();
    }
}
