using System;
using System.Collections;
using Rx.Extendibility.Observables;

namespace Rx.Unity.Linq.Observables {
    internal static class ByFrame<TSource, TTarget> {
        internal abstract class _ : OperatorSink<TSource, TTarget> {
            private readonly object _gate = new object();
            private readonly IEnumerator _timer;
            private readonly int _frameCount;
            private readonly FrameCountType _frameCountType;
            private bool _isRunning;
            private bool _isCompleted;
            private bool _isDisposed;

            public _(IObserver<TTarget> observer, int frameCount, FrameCountType frameCountType) : base(observer) {
                _frameCount = frameCount;
                _frameCountType = frameCountType;
                _timer = new ReusableEnumerator(this);
            }

            public sealed override void OnNext(TSource value) {
                lock (_gate) {
                    if (_isCompleted) return;
                    OnHandleNext(value);
                    if (_isRunning) return;
                    _isRunning = true;
                    _timer.Reset(); // reuse

                    switch (_frameCountType) {
                        case FrameCountType.Update:
                            MainThreadDispatcher.StartUpdateMicroCoroutine(_timer);
                            break;
                        case FrameCountType.FixedUpdate:
                            MainThreadDispatcher.StartFixedUpdateMicroCoroutine(_timer);
                            break;
                        case FrameCountType.EndOfFrame:
                            MainThreadDispatcher.StartEndOfFrameMicroCoroutine(_timer);
                            break;
                        default:
                            throw new ArgumentException($"Invalid FrameCountType: {_frameCountType}", nameof(_frameCountType));
                    }
                }
            }

            public sealed override void OnCompleted() {
                try {
                    TTarget target = default;
                    lock (_gate) {
                        _isCompleted = true;
                        if (!_isRunning)
                            return;
                        target = GetTarget();
                        _isRunning = false;
                    }
                    ForwardOnNext(target);
                }
                finally {
                    ForwardOnCompleted();
                }
            }

            protected abstract void OnHandleNext(TSource value);
            protected abstract TTarget GetTarget();
            protected abstract void ResetTarget();

            protected sealed override void Dispose(bool disposing) {
                base.Dispose(disposing);
                _isDisposed = true;
            }

            // reuse, no gc allocate
            internal sealed class ReusableEnumerator : IEnumerator {
                private readonly _ _parent;
                private int _currentFrame;

                public ReusableEnumerator(_ parent) => _parent = parent;

                public object Current => null;

                public bool MoveNext() {
                    if (_parent._isDisposed)
                        return false;
                    TTarget currentTarget;
                    lock (_parent._gate) {
                        if (_currentFrame++ == _parent._frameCount) {
                            if (_parent._isCompleted) return false;

                            currentTarget = _parent.GetTarget();
                            _parent.ResetTarget();
                            _parent._isRunning = false;
                        }
                        else {
                            return true;
                        }
                    }

                    _parent.ForwardOnNext(currentTarget);
                    return false;
                }

                public void Reset() => _currentFrame = 0;
            }
        }
    }
}
