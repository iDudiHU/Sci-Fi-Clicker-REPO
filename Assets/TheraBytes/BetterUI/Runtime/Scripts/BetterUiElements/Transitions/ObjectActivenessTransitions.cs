using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class ObjectActivenessTransitions : TransitionStateCollection<bool>
    {
        [Serializable]
        public class ActiveTransitionState : TransitionState
        {
            public ActiveTransitionState(string name, bool stateObject)
                : base(name, stateObject)
            { }
        }


        public override UnityEngine.Object Target { get { return target; } }

        [SerializeField]
        GameObject target;

        [SerializeField]
        List<ActiveTransitionState> states = new List<ActiveTransitionState>();


        public ObjectActivenessTransitions(params string[] stateNames)
            : base(stateNames)
        {
        }

        protected override void ApplyState(TransitionState state, bool instant)
        {
            if (this.Target == null)
                return;

            if (Application.isPlaying)
            {
                this.target.SetActive(state.StateObject);
            }
            //else
            //{
            //    Debug.LogWarning("Active State Transitions cannot be previewed outside play mode.");
            //}

        }

        internal override void AddStateObject(string stateName)
        {
            var obj = new ActiveTransitionState(stateName, true);
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
