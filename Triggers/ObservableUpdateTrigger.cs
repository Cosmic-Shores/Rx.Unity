using System;
using System.Reactive;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rx.Unity.Triggers {
    [DisallowMultipleComponent]
    public sealed class ObservableUpdateTrigger : MonoBehaviour {
        private Subject<Unit> _update;

        /// <summary>Update is called every frame, if the MonoBehaviour is enabled.</summary>
        public IObservable<Unit> UpdateAsObservable() => _update ??= new Subject<Unit>();

        /// <summary>Update is called every frame, if the MonoBehaviour is enabled.</summary>
        void Update() => _update?.OnNext(Unit.Default);

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        void OnDestroy() => _update?.OnCompleted();
    }
}
