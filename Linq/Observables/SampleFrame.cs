using System;
using Rx.Extendibility.Observables;

namespace Rx.Unity.Linq.Observables {
    internal sealed class SampleFrame<T> : ObservableProducer<T, SampleFrame<T>._> {
        private readonly IObservable<T> _source;
        private readonly int _frameCount;
        private readonly FrameCountType _frameCountType;

        public SampleFrame(IObservable<T> source, int frameCount, FrameCountType frameCountType) {
            _source = source;
            _frameCount = frameCount;
            _frameCountType = frameCountType;
        }

        protected override _ CreateSink(IObserver<T> observer) => new _(observer, _frameCount, _frameCountType);

        protected override void Run(_ sink) => sink.Run(_source);

        internal sealed class _ : ByFrame<T, T>._ {
            private T _latestValue;

            public _(IObserver<T> observer, int frameCount, FrameCountType frameCountType) : base(observer, frameCount, frameCountType) {}

            protected override void OnHandleNext(T value) => _latestValue = value;
            protected override T GetTarget() => _latestValue;
            protected override void ResetTarget() { }
        }
    }
}
