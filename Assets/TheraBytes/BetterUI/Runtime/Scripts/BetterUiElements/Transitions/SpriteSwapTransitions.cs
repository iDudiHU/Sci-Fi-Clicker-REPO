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
    public class SpriteSwapTransitions : TransitionStateCollection<Sprite>
    {
        [Serializable]
        public class SpriteSwapTransitionState : TransitionState
        {
            public SpriteSwapTransitionState(string name, Sprite stateObject)
                : base(name, stateObject)
            { }
        }


        public override UnityEngine.Object Target { get { return target; } }

        [SerializeField]
        Image target;

        [SerializeField]
        List<SpriteSwapTransitionState> states = new List<SpriteSwapTransitionState>();


        public SpriteSwapTransitions(params string[] stateNames)
            : base(stateNames)
        {
        }

        protected override void ApplyState(TransitionState state, bool instant)
        {
            if (this.Target == null)
                return;

            target.overrideSprite = state.StateObject;
        }

        internal override void AddStateObject(string stateName)
        {
            var obj = new SpriteSwapTransitionState(stateName, null);
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
