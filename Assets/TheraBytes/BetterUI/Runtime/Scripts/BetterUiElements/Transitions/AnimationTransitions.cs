using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class AnimationTransitions : TransitionStateCollection<string>
    {
        [Serializable]
        public class AnimationTransitionState : TransitionState
        {
            public AnimationTransitionState(string name, string stateObject)
                : base(name, stateObject)
            { }
        }


        public override UnityEngine.Object Target { get { return target; } }

        [SerializeField]
        Animator target;

        [SerializeField]
        List<AnimationTransitionState> states = new List<AnimationTransitionState>();


        public AnimationTransitions(params string[] stateNames)
            : base(stateNames)
        {
        }

        protected override void ApplyState(TransitionState state, bool instant)
        {
            if (this.Target == null
                || !(this.target.isActiveAndEnabled)
                || this.target.runtimeAnimatorController == null
                || string.IsNullOrEmpty(state.StateObject))
            {
                return;
            }

            foreach (var s in states)
            {
                this.target.ResetTrigger(s.StateObject);
            }

            this.target.SetTrigger(state.StateObject);
        }

        internal override void AddStateObject(string stateName)
        {
            var obj = new AnimationTransitionState(stateName, null);
            this.states.Add(obj);
        }

        protected override IEnumerable<TransitionState> GetTransitionStates()
        {
            foreach (var s in states)
                yield return s;
        }

        internal override void SortStates(string[] sortedOrder)
        {
            base.SortStatesLogic(states, sortedOrder);
        }
    }
}
