using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class MaterialPropertyTransition : TransitionStateCollection<float>
    {
        static Dictionary<MaterialPropertyTransition, Coroutine> activeCoroutines = new Dictionary<MaterialPropertyTransition, Coroutine>();
        static List<MaterialPropertyTransition> keysToRemove = new List<MaterialPropertyTransition>();

        [Serializable]
        public class MaterialPropertyTransitionState : TransitionState
        {
            public MaterialPropertyTransitionState(string name, float stateObject)
                : base(name, stateObject)
            { }
        }

        public override UnityEngine.Object Target { get { return target; } }
        public float FadeDurtaion { get { return fadeDuration; } set { fadeDuration = value; } }

        public int PropertyIndex { get { return propertyIndex; } set { propertyIndex = value; } }

        [SerializeField]
        BetterImage target;

        [SerializeField]
        float fadeDuration = 0.1f;

        [SerializeField]
        List<MaterialPropertyTransitionState> states = new List<MaterialPropertyTransitionState>();

        [SerializeField]
        int propertyIndex;


        public MaterialPropertyTransition(params string[] stateNames)
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

            float start = target.GetMaterialPropertyValue(propertyIndex);
            CrossFadeProperty(start, state.StateObject, (instant) ? 0 : fadeDuration);
        }

        internal override void AddStateObject(string stateName)
        {
            var obj = new MaterialPropertyTransitionState(stateName, 1f);
            this.states.Add(obj);
        }

        protected override IEnumerable<TransitionState> GetTransitionStates()
        {
            foreach (var s in states)
                yield return s;
        }

        void CrossFadeProperty(float startValue, float targetValue, float duration)
        {

            // Stop clashing coroutines
            foreach (var key in activeCoroutines.Keys)
            {
                if (key.target == this.target && key.propertyIndex == this.propertyIndex)
                {
                    if(key.target != null)
                        key.target.StopCoroutine(activeCoroutines[key]);

                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                activeCoroutines.Remove(key);
            }

            keysToRemove.Clear();

            // trigger value changes
            if (duration == 0)
            {
                target.SetMaterialProperty(propertyIndex, targetValue);
            }
            else
            {
                Coroutine coroutine = target.StartCoroutine(CoCrossFadeProperty(startValue, targetValue, duration));
                activeCoroutines.Add(this, coroutine);
            }
        }

        private IEnumerator CoCrossFadeProperty(float startValue, float targetValue, float duration)
        {

            // animate
            float startTime = Time.unscaledTime;
            float endTime = startTime + duration;

            while(Time.unscaledTime < endTime)
            {
                float amount = (Time.unscaledTime - startTime) / duration;
                float value = Mathf.Lerp(startValue, targetValue, amount);
                target.SetMaterialProperty(propertyIndex, value);
                yield return null;
            }

            target.SetMaterialProperty(propertyIndex, targetValue);
        }

        internal override void SortStates(string[] sortedOrder)
        {
            base.SortStatesLogic(states, sortedOrder);
        }
    }
}
