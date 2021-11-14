# Rx.Unity

[![Build](https://github.com/Cosmic-Shores/Rx.Unity/actions/workflows/build.yml/badge.svg)](https://github.com/Cosmic-Shores/Rx.Unity/actions/workflows/build.yml)
[![openupm](https://img.shields.io/npm/v/com.rx.unity?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.rx.unity/)
[![Unity 2021.2+](https://img.shields.io/badge/unity-2021.2%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://badgen.net/github/license/Naereen/Strapdown.js)](https://github.com/Cosmic-Shores/Rx.Unity/blob/master/LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com)

A lightweight layer on top of System.Reactive to add support for Unity in a similar fashion as one using UniRx is used to. Thus one can enjoy the improvement made in System.Reactive over the years and consume libraries using it with ease.

Right now it requires some changes to System.Reactive in order to work but I'll try and create a PR to integrate required changes to the official System.Reactive repo soon.

This is still work in progress.
A few tests still need fixing and alot of AoT issues need to be fixed / added hacks to enforce generic type generation for.

Part of the goal is to drop some legacy code from the UniRx code and start using a newer C# Syntax and framework versions.
This also means that older versions of unity won't be supported (Versions >= 2021 should work - testing is currently happening in the unity beta version only)

Other than the basic scheduling these are the features I moved over from UniRx:

Assembly: Rx.Data
- .AsUnitObservable()
- .AsSingleUnitObservable()
- .Pairwise() (without overloads)
- ReactiveProperty
- ReactiveCollection
- ReactiveDictionary
(all of those incl. readonly variants)

Assembly: Rx.Unity
- ObservableStateMachineTrigger
- Observable.ReturnUnit()
- Observable.EveryUpdate()
- Observable.EveryFixedUpdate()
- .UpdateAsObservable()
- .OnDestroyAsObservable()
- .ObserveOnMainThread() (without overloads)
- .SubscribeOnMainThread() (without overloads)
- .TakeUntilDestroy()
- .DelayFrame()
- .BatchFrame()
- .SampleFrame() (since Version 1.0.1)

---
changes:
- removed serialization support ReactiveProperty
- removed serialization support ReactiveDictionary
- removed IOptimizedObservable stuff

### Errors in Subscribers:
There's a bit of a gotcha with the changes that have been made in _System.Reactive_ since _UniRx_ was forked:

If you have something like a subject and a direct subscriber to it throws an exception handleing an `OnNext()` everything works as expected.

However if you now introduce an operator between your subject and subscribe to that instead, an unhandled exception at the same spot as before, can lead to the source subject stopping emit future values (I haven't investigated the exact details on what happens to the subject beyond this description here). In UniRx this wasn't the case and that behaviour was ensured by the `DurabilityTest`-class.

After looking into it some _System.Reactive_ code and doing some research on it, it looks to me like this is sort of a known issue that is a tolerated side effect which can seldomly happen as additional checks to detect this between every operator would hurt performance.

---
Important: when using Rx.Unity it is crucial that
`Rx.Unity.ReactiveUnity.SetupPatches()`
is executed before any other reactive code to ensure the environment is properly configured for unity.

### UniRx Throttle-/ SampleFrame Fuuu:
(To help your sanity migrating to Rx.Unity)

#### ThrottleFrame
More like a debounce. Emits the last value of the previous frame the next frame unless another value was emited in the mean time. It always emits on the main thread.

#### ThrottleFirstFrame
A regular rx standard throttle, that emits in the source context (which might not be the main thread).

Emits the first value within each window. Each window is reset using the specified frameCount and frameCountType. Always emits the very first value right away.

#### SampleFrame
Does exactly what you expect it to do acording to rx standards.

Emits the last value of each window if any. Each window is reset using the specified frameCount and frameCountType. It always emits on the main thread. (Same as `BatchFrame(frameCount, frameCountType).Select(x => x.Last())`; Default parameter for frameCountType differs!)
