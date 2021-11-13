using System;
using System.Collections;
using System.Reactive.Disposables;
using Rx.Extendibility.Observables;

namespace Rx.Unity.Linq.Observables {
    internal sealed class UnityInterval : ObservableProducer<long, UnityInterval._> {
        private readonly FrameCountType _frameCountType;

        public UnityInterval(FrameCountType frameCountType) => _frameCountType = frameCountType;

        protected override _ CreateSink(IObserver<long> observer) => new _(observer, _frameCountType);

        protected override void Run(_ sink) => sink.Run();

        internal sealed class _ : OperatorSink<long, long> {
            private readonly FrameCountType _frameCountType;

            public _(IObserver<long> observer, FrameCountType frameCountType) : base(observer) {
                _frameCountType = frameCountType;
            }

            public void Run() {
                var disposable = new BooleanDisposable();
                switch (_frameCountType) {
                    case FrameCountType.Update:
                        MainThreadDispatcher.StartUpdateMicroCoroutine(EveryCycle(disposable));
                        break;
                    case FrameCountType.FixedUpdate:
                        MainThreadDispatcher.StartFixedUpdateMicroCoroutine(EveryCycle(disposable));
                        break;
                    case FrameCountType.EndOfFrame:
                        MainThreadDispatcher.StartEndOfFrameMicroCoroutine(EveryCycle(disposable));
                        break;
                    default:
                        throw new ArgumentException($"Invalid FrameCountType: {_frameCountType}", nameof(_frameCountType));
                }

                SetUpstream(disposable);
            }

            internal IEnumerator EveryCycle(BooleanDisposable disposable) {
                if (disposable.IsDisposed) yield break;
                var count = 0L;
                while (true) {
                    yield return null;
                    if (disposable.IsDisposed) yield break;

                    ForwardOnNext(unchecked(count++));
                }
            }

            public override void OnNext(long value) {}
        }
    }
}
