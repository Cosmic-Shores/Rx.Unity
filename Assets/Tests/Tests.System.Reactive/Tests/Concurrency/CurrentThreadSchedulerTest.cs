﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Threading;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ReactiveTests.Tests
{

    public class CurrentThreadSchedulerTest
    {
        [Test]
        public void CurrentThread_ArgumentChecking()
        {
            ReactiveAssert.Throws<ArgumentNullException>(() => Scheduler.CurrentThread.Schedule(42, default));
            ReactiveAssert.Throws<ArgumentNullException>(() => Scheduler.CurrentThread.Schedule(42, default(TimeSpan), default));
            ReactiveAssert.Throws<ArgumentNullException>(() => Scheduler.CurrentThread.Schedule(42, default(DateTimeOffset), default));
        }

        [Test]
        public void CurrentThread_Now()
        {
            var res = Scheduler.CurrentThread.Now - DateTime.Now;
            Assert.True(res.Seconds < 1);
        }

#if !NO_THREAD
        [Test]
        public void CurrentThread_ScheduleAction()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            var ran = false;
            Scheduler.CurrentThread.Schedule(() => { Assert.AreEqual(id, Thread.CurrentThread.ManagedThreadId); ran = true; });
            Assert.True(ran);
        }
#endif

        [Test]
        public void CurrentThread_ScheduleActionError()
        {
            var ex = new Exception();

            try
            {
                Scheduler.CurrentThread.Schedule(() => { throw ex; });
                Assert.True(false);
            }
            catch (Exception e)
            {
                Assert.AreSame(e, ex);
            }
        }
#if !NO_THREAD
        [Test]
        public void CurrentThread_ScheduleActionNested()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            var ran = false;
            Scheduler.CurrentThread.Schedule(() =>
            {
                Assert.AreEqual(id, Thread.CurrentThread.ManagedThreadId);
                Scheduler.CurrentThread.Schedule(() => { ran = true; });
            });
            Assert.True(ran);
        }

        [Test]
        public void CurrentThread_ScheduleActionNested_TimeSpan()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            var ran = false;
            Scheduler.CurrentThread.Schedule(() =>
            {
                Assert.AreEqual(id, Thread.CurrentThread.ManagedThreadId);
                Scheduler.CurrentThread.Schedule(TimeSpan.FromSeconds(1), () => { ran = true; });
            });
            Assert.True(ran);
        }

        [Test]
        public void CurrentThread_ScheduleActionDue()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            var ran = false;
            Scheduler.CurrentThread.Schedule(TimeSpan.FromSeconds(0.2), () => { Assert.AreEqual(id, Thread.CurrentThread.ManagedThreadId); ran = true; });
            Assert.True(ran, "ran");
        }

        [Test]
        public void CurrentThread_ScheduleActionDueNested()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            var ran = false;
            Scheduler.CurrentThread.Schedule(TimeSpan.FromSeconds(0.2), () =>
            {
                Assert.AreEqual(id, Thread.CurrentThread.ManagedThreadId);

                Scheduler.CurrentThread.Schedule(TimeSpan.FromSeconds(0.2), () =>
                {
                    Assert.AreEqual(id, Thread.CurrentThread.ManagedThreadId);
                    ran = true;
                });
            });
            Assert.True(ran, "ran");
        }
#endif
        [Test]
        public void CurrentThread_EnsureTrampoline()
        {
            var ran1 = false;
            var ran2 = false;
            Scheduler.CurrentThread.EnsureTrampoline(() =>
            {
                Scheduler.CurrentThread.Schedule(() => { ran1 = true; });
                Scheduler.CurrentThread.Schedule(() => { ran2 = true; });
            });
            Assert.True(ran1);
            Assert.True(ran2);
        }

        [Test]
        public void CurrentThread_EnsureTrampoline_Nested()
        {
            var ran1 = false;
            var ran2 = false;
            Scheduler.CurrentThread.EnsureTrampoline(() =>
            {
                Scheduler.CurrentThread.EnsureTrampoline(() => { ran1 = true; });
                Scheduler.CurrentThread.EnsureTrampoline(() => { ran2 = true; });
            });
            Assert.True(ran1);
            Assert.True(ran2);
        }

        [Test]
        public void CurrentThread_EnsureTrampolineAndCancel()
        {
            var ran1 = false;
            var ran2 = false;
            Scheduler.CurrentThread.EnsureTrampoline(() =>
            {
                Scheduler.CurrentThread.Schedule(() =>
                {
                    ran1 = true;
                    var d = Scheduler.CurrentThread.Schedule(() => { ran2 = true; });
                    d.Dispose();
                });
            });
            Assert.True(ran1);
            Assert.False(ran2);
        }

        [Test]
        public void CurrentThread_EnsureTrampolineAndCancelTimed()
        {
            var ran1 = false;
            var ran2 = false;
            Scheduler.CurrentThread.EnsureTrampoline(() =>
            {
                Scheduler.CurrentThread.Schedule(() =>
                {
                    ran1 = true;
                    var d = Scheduler.CurrentThread.Schedule(TimeSpan.FromSeconds(1), () => { ran2 = true; });
                    d.Dispose();
                });
            });
            Assert.True(ran1);
            Assert.False(ran2);
        }
    }
}
