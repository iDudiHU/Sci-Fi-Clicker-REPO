using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class AlphaTransitions : TransitionStateCollection<float>
    {
        [Serializable]
        public class AlphaTransitionState : TransitionState
        {
            public AlphaTransitionState(string name, float stateObject)
                : base(name, stateObject)
            { }
        }

        public override UnityEngine.Object Target { get { return target; } }
        public float FadeDurtaion { get { return fadeDuration; } set { fadeDuration = value; } }

        [SerializeField]
        Graphic target;

        [SerializeField]
        float fadeDuration = 0.1f;

        [SerializeField]
        List<AlphaTransitionState> states = new List<AlphaTransitionState>();


        public AlphaTransitions(params string[] stateNames)
            : base(stateNames)
        {
        }

        protected override void ApplyState(TransitionState state, bool instant)
        {
            if (this.Target == null)
                return;

            if (!(Application.isPlaying))
            {
                instant = true;
            }

            this.target.CrossFadeAlpha(state.StateObject, (instant) ? 0f : this.fadeDuration, true);
        }

        internal override void AddStateObject(string stateName)
        {
            var obj = new AlphaTransitionState(stateName, 1f);
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
