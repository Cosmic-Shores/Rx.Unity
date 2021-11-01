// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Rx.Unity.Tests.Helper;

namespace ReactiveTests.Tests
{
    public partial class StartAsyncTest : ReactiveTest
    {

        private readonly Task<int> _doneTask;

        public StartAsyncTest()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(42);
            _doneTask = tcs.Task;
        }

        #region Func

        [Test]
        public void StartAsync_Func_ArgumentChecking()
        {
            var s = Scheduler.Immediate;

            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(default(Func<Task<int>>)));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(default(Func<CancellationToken, Task<int>>)));

            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(default(Func<Task<int>>), s));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(default(Func<CancellationToken, Task<int>>), s));

            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(() => _doneTask, default));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(ct => _doneTask, default));
        }

        [Test]
        public void StartAsync_Func_Success()
        {
            var n = 42;

            var i = 0;

            var xs = Observable.StartAsync(() =>
            {
                i++;
                return Task.Factory.StartNew(() => n);
            });

            XunitAssert.Equal(n, xs.Single());
            XunitAssert.Equal(1, i);

            XunitAssert.Equal(n, xs.Single());
            XunitAssert.Equal(1, i);
        }

        [Test]
        public void StartAsync_Func_Throw_Synchronous()
        {
            var ex = new Exception();

            var xs = Observable.StartAsync<int>(() =>
            {
                throw ex;
            });

            ReactiveAssert.Throws(ex, () => xs.Single());
        }

        [Test]
        public void StartAsync_Func_Throw_Asynchronous()
        {
            var ex = new Exception();

            var xs = Observable.StartAsync(() =>
                Task.Factory.StartNew<int>(() =>
                {
                    throw ex;
                })
            );

            ReactiveAssert.Throws(ex, () => xs.Single());
        }

        [Test]
        public void StartAsync_FuncWithCancel_Success()
        {
            var n = 42;

            var i = 0;
            var xs = Observable.StartAsync(ct =>
            {
                i++;
                return Task.Factory.StartNew(() => n);
            });

            XunitAssert.Equal(n, xs.Single());
            XunitAssert.Equal(1, i);

            XunitAssert.Equal(n, xs.Single());
            XunitAssert.Equal(1, i);
        }

        [Test]
        public void StartAsync_FuncWithCancel_Throw_Synchronous()
        {
            var ex = new Exception();

            var xs = Observable.StartAsync<int>(ct =>
            {
                throw ex;
            });

            ReactiveAssert.Throws(ex, () => xs.Single());
        }

        [Test]
        public void StartAsync_FuncWithCancel_Throw_Asynchronous()
        {
            var ex = new Exception();

            var xs = Observable.StartAsync(ct =>
                Task.Factory.StartNew<int>(() => { throw ex; })
            );

            ReactiveAssert.Throws(ex, () => xs.Single());
        }

        [Test]
        public void StartAsync_FuncWithCancel_Cancel()
        {
            var N = 10;

            for (var n = 0; n < N; n++)
            {
                var e = new ManualResetEvent(false);
                var f = new ManualResetEvent(false);

                var t = default(Task<int>);
                var xs = Observable.StartAsync(ct =>
                    t = Task.Factory.StartNew<int>(() =>
                    {
                        try
                        {
                            e.Set();
                            while (true)
                            {
                                ct.ThrowIfCancellationRequested();
                            }
                        }
                        finally
                        {
                            f.Set();
                        }
                    })
                );

                e.WaitOne();

                var d = xs.Subscribe(_ => { });
                d.Dispose();

                f.WaitOne();
                while (!t.IsCompleted)
                {
                    ;
                }

                ReactiveAssert.Throws<OperationCanceledException>(() => xs.Single());
            }
        }

#if DESKTOPCLR
        [Test]
        public void StartAsync_Func_Scheduler1()
        {
            var tcs = new TaskCompletionSource<int>();

            var e = new ManualResetEvent(false);
            var x = default(int);
            var t = default(int);

            var xs = Observable.StartAsync(() => tcs.Task, Scheduler.Immediate);
            xs.Subscribe(res =>
            {
                x = res;
                t = Thread.CurrentThread.ManagedThreadId;
                e.Set();
            });

            tcs.SetResult(42);

            e.WaitOne();

            XunitAssert.Equal(42, x);
            XunitAssert.Equal(Thread.CurrentThread.ManagedThreadId, t);
        }

        [Test]
        public void StartAsync_Func_Scheduler2()
        {
            var tcs = new TaskCompletionSource<int>();

            var e = new ManualResetEvent(false);
            var x = default(int);
            var t = default(int);

            var xs = Observable.StartAsync(ct => tcs.Task, Scheduler.Immediate);
            xs.Subscribe(res =>
            {
                x = res;
                t = Thread.CurrentThread.ManagedThreadId;
                e.Set();
            });

            tcs.SetResult(42);

            e.WaitOne();

            XunitAssert.Equal(42, x);
            XunitAssert.Equal(Thread.CurrentThread.ManagedThreadId, t);
        }
#endif

        #endregion

        #region Action

        [Test]
        public void StartAsync_Action_ArgumentChecking()
        {
            var s = Scheduler.Immediate;

            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(default(Func<Task>)));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(default(Func<CancellationToken, Task>)));

            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(default(Func<Task>), s));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(default(Func<CancellationToken, Task>), s));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(() => (Task)_doneTask, default));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.StartAsync(ct => (Task)_doneTask, default));
        }

        [Test]
        public void StartAsync_Action_Success()
        {
            var i = 0;
            var xs = Observable.StartAsync(() =>
            {
                i++;
                return Task.Factory.StartNew(() => { });
            });

            XunitAssert.Equal(Unit.Default, xs.Single());
            XunitAssert.Equal(1, i);

            XunitAssert.Equal(Unit.Default, xs.Single());
            XunitAssert.Equal(1, i);
        }

        [Test]
        public void StartAsync_Action_Throw_Synchronous()
        {
            var ex = new Exception();

            var xs = Observable.StartAsync(() =>
            {
                throw ex;
            });

            ReactiveAssert.Throws(ex, () => xs.Single());
        }

        [Test]
        public void StartAsync_Action_Throw_Asynchronous()
        {
            var ex = new Exception();

            var xs = Observable.StartAsync(() =>
                Task.Factory.StartNew(() => { throw ex; })
            );

            ReactiveAssert.Throws(ex, () => xs.Single());
        }

        [Test]
        public void StartAsync_ActionWithCancel_Success()
        {
            var i = 0;
            var xs = Observable.StartAsync(ct =>
            {
                i++;
                return Task.Factory.StartNew(() => { });
            });

            XunitAssert.Equal(Unit.Default, xs.Single());
            XunitAssert.Equal(1, i);

            XunitAssert.Equal(Unit.Default, xs.Single());
            XunitAssert.Equal(1, i);
        }

        [Test]
        public void StartAsync_ActionWithCancel_Throw_Synchronous()
        {
            var ex = new Exception();

            var xs = Observable.StartAsync(ct =>
            {
                throw ex;
            });

            ReactiveAssert.Throws(ex, () => xs.Single());
        }

        [Test]
        public void StartAsync_ActionWithCancel_Throw_Asynchronous()
        {
            var ex = new Exception();

            var xs = Observable.StartAsync(ct =>
                Task.Factory.StartNew(() => { throw ex; })
            );

            ReactiveAssert.Throws(ex, () => xs.Single());
        }

        [Test]
        public void StartAsync_ActionWithCancel_Cancel()
        {
            var N = 10;

            for (var n = 0; n < N; n++)
            {
                var e = new ManualResetEvent(false);
                var f = new ManualResetEvent(false);

                var t = default(Task);
                var xs = Observable.StartAsync(ct =>
                    t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            e.Set();
                            while (true)
                            {
                                ct.ThrowIfCancellationRequested();
                            }
                        }
                        finally
                        {
                            f.Set();
                        }
                    })
                );

                e.WaitOne();

                var d = xs.Subscribe(_ => { });
                d.Dispose();

                f.WaitOne();
                while (!t.IsCompleted)
                {
                    ;
                }

                ReactiveAssert.Throws<OperationCanceledException>(() => xs.Single());
            }
        }

#if DESKTOPCLR
        [Test]
        public void StartAsync_Action_Scheduler1()
        {
            var tcs = new TaskCompletionSource<int>();

            var e = new ManualResetEvent(false);
            var t = default(int);

            var xs = Observable.StartAsync(() => (Task)tcs.Task, Scheduler.Immediate);
            xs.Subscribe(res =>
            {
                t = Thread.CurrentThread.ManagedThreadId;
                e.Set();
            });

            tcs.SetResult(42);

            e.WaitOne();

            XunitAssert.Equal(Thread.CurrentThread.ManagedThreadId, t);
        }

        [Test]
        public void StartAsync_Action_Scheduler2()
        {
            var tcs = new TaskCompletionSource<int>();

            var e = new ManualResetEvent(false);
            var t = default(int);

            var xs = Observable.StartAsync(ct => (Task)tcs.Task, Scheduler.Immediate);
            xs.Subscribe(res =>
            {
                t = Thread.CurrentThread.ManagedThreadId;
                e.Set();
            });

            tcs.SetResult(42);

            e.WaitOne();

            XunitAssert.Equal(Thread.CurrentThread.ManagedThreadId, t);
        }
#endif

        #endregion

    }
}
