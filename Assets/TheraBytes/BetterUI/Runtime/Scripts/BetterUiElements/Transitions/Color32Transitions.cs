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
    public class Color32Transitions : TransitionStateCollection<Color32>
    {
        [Serializable]
        public class Color32TransitionState : TransitionState
        {
            public Color32TransitionState(string name, Color32 stateObject)
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
        List<Color32TransitionState> states = new List<Color32TransitionState>();


        public Color32Transitions(params string[] stateNames)
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

            this.target.CrossFadeColor(state.StateObject, (instant) ? 0f : this.fadeDuration, true, true);

        }

        internal override void AddStateObject(string stateName)
        {
            var obj = new Color32TransitionState(stateName, new Color32(255, 255, 255, 255));
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
