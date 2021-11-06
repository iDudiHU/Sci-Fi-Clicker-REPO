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
    public class ColorTransitions : TransitionStateCollection<Color>
    {
        [Serializable]
        public class ColorTransitionState : TransitionState
        {
            public ColorTransitionState(string name, Color stateObject)
                : base(name, stateObject)
            { }
        }


        public override UnityEngine.Object Target { get { return target; } }
        public float FadeDurtaion { get { return fadeDuration; } set { fadeDuration = value; } }


        [SerializeField]
        Graphic target;

        [Range(1, 5)]
        [SerializeField]
        float colorMultiplier = 1;

        [SerializeField]
        float fadeDuration = 0.1f;

        [SerializeField]
        List<ColorTransitionState> states = new List<ColorTransitionState>();


        public ColorTransitions(params string[] stateNames)
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

            // Backwards compatibility: colorMultiplyer is a new field. 
            // It is 0 for upgrades from 1.x versions of Better UI.
            if (colorMultiplier <= float.Epsilon)
            {
                colorMultiplier = 1;
            }

            this.target.CrossFadeColor(state.StateObject * colorMultiplier, (instant) ? 0f : this.fadeDuration, true, true);

        }

        internal override void AddStateObject(string stateName)
        {
            var obj = new ColorTransitionState(stateName, Color.white);
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
