using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class CustomTransitions : TransitionStateCollection<UnityEvent>
    {
        [Serializable]
        public class CustomTransitionState : TransitionState
        {
            public CustomTransitionState(string name)
                : base(name, new UnityEvent())
            { }
        }

        [SerializeField]
        List<CustomTransitionState> states = new List<CustomTransitionState>();

        public override UnityEngine.Object Target { get { return null; } }

        public CustomTransitions(params string[] stateNames)
            : base(stateNames)
        {
        }

        protected override void ApplyState(TransitionState state, bool instant)
        {
            state.StateObject.Invoke();
        }

        internal override void AddStateObject(string stateName)
        {
            var obj = new CustomTransitionState(stateName);
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
