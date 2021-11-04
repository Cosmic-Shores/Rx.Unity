using System;
using System.Collections;
using System.Collections.Generic;
using Rx.Extendibility.Observables;

namespace Rx.Unity.Linq.Observables {
    internal sealed class BatchFrame<T> : ObservableProducer<IList<T>, BatchFrame<T>._> {
        private readonly IObservable<T> _source;
        private readonly int _frameCount;
        private readonly FrameCountType _frameCountType;

        public BatchFrame(IObservable<T> source, int frameCount, FrameCountType frameCountType) {
            _source = source;
            _frameCount = frameCount;
            _frameCountType = frameCountType;
        }

        protected override _ CreateSink(IObserver<IList<T>> observer) => new _(observer, _frameCount, _frameCountType);

        protected override void Run(_ sink) => sink.Run(_source);

        internal sealed class _ : OperatorSink<T, IList<T>> {
            private readonly object _gate = new object();
            private readonly IEnumerator _timer;
            private readonly int _frameCount;
            private readonly FrameCountType _frameCountType;
            private bool _isRunning;
            private bool _isCompleted;
            private bool _isDisposed;
            private List<T> _list = new List<T>();

            public _(IObserver<IList<T>> observer, int frameCount, FrameCountType frameCountType) : base(observer) {
                _frameCount = frameCount;
                _frameCountType = frameCountType;
                _timer = new ReusableEnumerator(this);
            }

            public override void OnNext(T value) {
                lock (_gate) {
                    if (_isCompleted) return;
                    _list.Add(value);
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

            public override void OnCompleted() {
                List<T> currentList;
                lock (_gate) {
                    _isCompleted = true;
                    currentList = _list;
                }
                if (currentList.Count != 0)
                    ForwardOnNext(currentList);
                ForwardOnCompleted();
            }

            protected override void Dispose(bool disposing) {
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
                    List<T> currentList;
                    lock (_parent._gate) {
                        if (_currentFrame++ == _parent._frameCount) {
                            if (_parent._isCompleted) return false;

                            currentList = _parent._list;
                            _parent._list = new List<T>();
                            _parent._isRunning = false;

                            // exit lock 
                        }
                        else {
                            return true;
                        }
                    }

                    _parent.ForwardOnNext(currentList);
                    return false;
                }

                public void Reset() => _currentFrame = 0;
            }
        }
    }
}
