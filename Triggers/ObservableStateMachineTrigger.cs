using System; // require keep for Windows Universal App
using System.Reactive.Subjects;
using UnityEngine;

namespace Rx.Unity.Triggers {
    [DisallowMultipleComponent]
    public class ObservableStateMachineTrigger : StateMachineBehaviour {
        // OnStateExit

        private Subject<OnStateInfo> _onStateExit;

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _onStateExit?.OnNext(new OnStateInfo(animator, stateInfo, layerIndex));
        }

        public IObservable<OnStateInfo> OnStateExitAsObservable() => _onStateExit ??= new Subject<OnStateInfo>();

        // OnStateEnter

        private Subject<OnStateInfo> _onStateEnter;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _onStateEnter?.OnNext(new OnStateInfo(animator, stateInfo, layerIndex));
        }

        public IObservable<OnStateInfo> OnStateEnterAsObservable() => _onStateEnter ??= new Subject<OnStateInfo>();

        // OnStateIK

        private Subject<OnStateInfo> _onStateIK;

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _onStateIK?.OnNext(new OnStateInfo(animator, stateInfo, layerIndex));
        }

        public IObservable<OnStateInfo> OnStateIKAsObservable() => _onStateIK ??= new Subject<OnStateInfo>();

        // Does not implments OnStateMove.
        // ObservableStateMachine Trigger makes stop animating.
        // By defining OnAnimatorMove, you are signifying that you want to intercept the movement of the root object and apply it yourself.
        // http://fogbugz.unity3d.com/default.asp?700990_9jqaim4ev33i8e9h

        private Subject<OnStateInfo> _onStateUpdate;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _onStateUpdate?.OnNext(new OnStateInfo(animator, stateInfo, layerIndex));
        }

        public IObservable<OnStateInfo> OnStateUpdateAsObservable() => _onStateUpdate ??= new Subject<OnStateInfo>();

        // OnStateMachineEnter

        private Subject<OnStateMachineInfo> _onStateMachineEnter;

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            _onStateMachineEnter?.OnNext(new OnStateMachineInfo(animator, stateMachinePathHash));
        }

        public IObservable<OnStateMachineInfo> OnStateMachineEnterAsObservable() => _onStateMachineEnter ??= new Subject<OnStateMachineInfo>();

        // OnStateMachineExit

        private Subject<OnStateMachineInfo> _onStateMachineExit;

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            _onStateMachineExit?.OnNext(new OnStateMachineInfo(animator, stateMachinePathHash));
        }

        public IObservable<OnStateMachineInfo> OnStateMachineExitAsObservable() => _onStateMachineExit ??= new Subject<OnStateMachineInfo>();

        public class OnStateInfo {
            public Animator Animator { get; private set; }
            public AnimatorStateInfo StateInfo { get; private set; }
            public int LayerIndex { get; private set; }

            public OnStateInfo(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
                Animator = animator;
                StateInfo = stateInfo;
                LayerIndex = layerIndex;
            }
        }

        public class OnStateMachineInfo {
            public Animator Animator { get; private set; }
            public int StateMachinePathHash { get; private set; }

            public OnStateMachineInfo(Animator animator, int stateMachinePathHash) {
                Animator = animator;
                StateMachinePathHash = stateMachinePathHash;
            }
        }
    }
}
