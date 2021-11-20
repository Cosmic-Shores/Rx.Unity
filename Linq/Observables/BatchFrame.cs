using System;
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

        internal sealed class _ : ByFrame<T, IList<T>>._ {
            private List<T> _list = new List<T>();

            public _(IObserver<IList<T>> observer, int frameCount, FrameCountType frameCountType) : base(observer, frameCount, frameCountType) { }

            protected override void OnHandleNext(T value) => _list.Add(value);
            protected override IList<T> GetTarget() => _list;
            protected override void ResetTarget() => _list = new List<T>();
        }
    }
}
