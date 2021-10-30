﻿using System.Reactive.Extendibility.Observables;

namespace System.Reactive.Data.Linq.Observables {
    internal sealed class Pairwise<T> : ObservableProducer<Pair<T>, Pairwise<T>._> {
        private readonly IObservable<T> _source;

        public Pairwise(IObservable<T> source) => _source = source;

        protected override _ CreateSink(IObserver<Pair<T>> observer) => new _(observer);

        protected override void Run(_ sink) => sink.Run(_source);

        internal sealed class _ : Sink<T, Pair<T>> {
            private T _prev;
            private bool _isFirst = true;

            public _(IObserver<Pair<T>> observer) : base(observer) { }

            public override void OnNext(T value) {
                if (_isFirst) {
                    _isFirst = false;
                    _prev = value;
                    return;
                }

                var pair = new Pair<T>(_prev, value);
                _prev = value;
                ForwardOnNext(pair);
            }
        }
    }
}
