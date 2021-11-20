using System;
using System.Collections;
using System.Collections.Generic;
using Rx.Extendibility.Observables;

namespace Rx.Unity.Linq.Observables {
    internal sealed class DelayFrame<T> : ObservableProducer<T, DelayFrame<T>._> {
        private readonly IObservable<T> _source;
        private readonly int _frameCount;
        private readonly FrameCountType _frameCountType;

        public DelayFrame(IObservable<T> source, int frameCount, FrameCountType frameCountType) {
            _source = source;
            _frameCount = frameCount;
            _frameCountType = frameCountType;
        }

        protected override _ CreateSink(IObserver<T> observer) => new _(observer, _frameCount, _frameCountType);

        protected override void Run(_ sink) => sink.Run(_source);

        internal sealed class _ : OperatorSink<T, T> {
            private readonly object _gate = new object();
            private readonly QueuePool _pool = new QueuePool();
            private readonly int _frameCount;
            private readonly FrameCountType _frameCountType;
            private int _runningEnumeratorCount;
            private bool _readyDrainEnumerator;
            private bool _running;
            private Queue<T> _currentQueueReference;
            private bool _calledCompleted;
            private bool _hasError;
            private Exception _error;
            private bool _isDisposed;

            public _(IObserver<T> observer, int frameCount, FrameCountType frameCountType) : base(observer) {
                _frameCount = frameCount;
                _frameCountType = frameCountType;
            }

            public override void OnNext(T value) {
                if (_isDisposed) return;

                Queue<T> targetQueue = null;
                lock (_gate) {
                    if (!_readyDrainEnumerator) {
                        _readyDrainEnumerator = true;
                        _runningEnumeratorCount++;
                        targetQueue = _currentQueueReference = _pool.Get();
                        targetQueue.Enqueue(value);
                    }
                    else {
                        if (_currentQueueReference != null) // null - if doesn't start OnNext and start OnCompleted
                            _currentQueueReference.Enqueue(value);
                        return;
                    }
                }
                RunDrainQueue(targetQueue);
            }

            public override void OnError(Exception error) {
                if (_isDisposed) return;

                lock (_gate) {
                    if (_running) {
                        _hasError = true;
                        _error = error;
                        return;
                    }
                }

                _isDisposed = true;
                ForwardOnError(error);
            }

            public override void OnCompleted() {
                if (_isDisposed) return;

                lock (_gate) {
                    _calledCompleted = true;
                    if (_readyDrainEnumerator)
                        return;

                    _readyDrainEnumerator = true;
                    _runningEnumeratorCount++;
                }

                RunDrainQueue(null);
            }

            protected override void Dispose(bool disposing) {
                base.Dispose(disposing);
                _isDisposed = true;
            }

            private void RunDrainQueue(Queue<T> targetQueue) {
                switch (_frameCountType) {
                    case FrameCountType.Update:
                        MainThreadDispatcher.StartUpdateMicroCoroutine(DrainQueue(targetQueue, _frameCount));
                        break;
                    case FrameCountType.FixedUpdate:
                        MainThreadDispatcher.StartFixedUpdateMicroCoroutine(DrainQueue(targetQueue, _frameCount));
                        break;
                    case FrameCountType.EndOfFrame:
                        MainThreadDispatcher.StartEndOfFrameMicroCoroutine(DrainQueue(targetQueue, _frameCount));
                        break;
                    default:
                        throw new ArgumentException($"Invalid FrameCountType: {_frameCountType}", nameof(_frameCountType));
                }
            }

            IEnumerator DrainQueue(Queue<T> q, int frameCount) {
                lock (_gate) {
                    _readyDrainEnumerator = false; // use next queue.
                    _running = false;
                }

                while (!_isDisposed && frameCount-- != 0)
                    yield return null;

                try {
                    if (q != null) {
                        while (q.Count > 0 && !_hasError) {
                            if (_isDisposed) break;

                            lock (_gate) _running = true;

                            var value = q.Dequeue();
                            ForwardOnNext(value);

                            lock (_gate) _running = false;
                        }

                        if (q.Count == 0)
                            _pool.Return(q);
                    }

                    if (_hasError) {
                        if (!_isDisposed) {
                            _isDisposed = true;
                            ForwardOnError(_error);
                        }
                    }
                    else if (_calledCompleted) {
                        lock (_gate) {
                            // not self only
                            if (_runningEnumeratorCount != 1) yield break;
                        }

                        if (!_isDisposed) {
                            _isDisposed = true;
                            OnCompleted();
                        }
                    }
                }
                finally {
                    lock (_gate) _runningEnumeratorCount--;
                }
            }
        }

        internal sealed class QueuePool {
            private readonly object _gate = new object();
            private readonly Queue<Queue<T>> _pool = new Queue<Queue<T>>(2);

            public Queue<T> Get() {
                lock (_gate) {
                    if (_pool.Count == 0)
                        return new Queue<T>(2);
                    else
                        return _pool.Dequeue();
                }
            }

            public void Return(Queue<T> q) {
                lock (_gate)
                    _pool.Enqueue(q);
            }
        }
    }
}
