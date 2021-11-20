using System;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;

namespace Rx.Unity.Triggers {
    public static class ObservableTriggerExtensions {
        /// <summary>Update is called every frame, if the MonoBehaviour is enabled.</summary>
        public static IObservable<Unit> UpdateAsObservable(this GameObject gameObject) {
            if (gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableUpdateTrigger>(gameObject).UpdateAsObservable();
        }

        /// <summary>Update is called every frame, if the MonoBehaviour is enabled.</summary>
        public static IObservable<Unit> UpdateAsObservable(this Component component) {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableUpdateTrigger>(component.gameObject).UpdateAsObservable();
        }

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        public static IObservable<Unit> OnDestroyAsObservable(this GameObject gameObject) {
            if (gameObject == null) return Observable.Return(Unit.Default); // send destroy message
            return GetOrAddComponent<ObservableDestroyTrigger>(gameObject).OnDestroyAsObservable();
        }

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        public static IObservable<Unit> OnDestroyAsObservable(this Component component) {
            if (component == null || component.gameObject == null) return Observable.Return(Unit.Default); // send destroy message
            return GetOrAddComponent<ObservableDestroyTrigger>(component.gameObject).OnDestroyAsObservable();
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }
    }
}
