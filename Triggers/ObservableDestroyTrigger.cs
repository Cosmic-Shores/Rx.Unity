using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rx.Unity.Triggers {
    [DisallowMultipleComponent]
    public sealed class ObservableDestroyTrigger : MonoBehaviour {
        private bool _isDestroyed = false;
        private Subject<Unit> _onDestroy;

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        public IObservable<Unit> OnDestroyAsObservable() {
            if (this == null) return Observable.Return(Unit.Default);
            if (_isDestroyed) return Observable.Return(Unit.Default);
            return _onDestroy ??= new Subject<Unit>();
        }

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        void OnDestroy() {
            if (_isDestroyed)
                return;
            _isDestroyed = true;
            if (_onDestroy != null) {
                _onDestroy.OnNext(Unit.Default);
                _onDestroy.OnCompleted();
            }
        }
    }
}
