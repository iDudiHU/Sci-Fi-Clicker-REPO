using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
    [HelpURL("https://documentation.therabytes.de/better-ui/BetterToggle.html")]
    [AddComponentMenu("Better UI/Controls/Better Toggle", 30)]
    public class BetterToggle : Toggle, IBetterTransitionUiElement
    {
        [Serializable]
        public class ToggleGraphics
        {
            public ToggleTransition ToggleTransition = ToggleTransition.Fade;
            public Graphic Graphic;
            public float FadeDuration = 0.1f;
        }

        public List<Transitions> BetterTransitions { get { return betterTransitions; } }
        public List<Transitions> BetterToggleTransitions { get { return betterToggleTransitions; } }

        [SerializeField]
        List<Transitions> betterTransitions = new List<Transitions>();

        [SerializeField]
        List<Transitions> betterToggleTransitions = new List<Transitions>();
        

        protected override void OnEnable()
        {
            base.onValueChanged.AddListener(ValueChanged);
            base.OnEnable();
            ValueChanged(base.isOn, true);
            DoStateTransition(SelectionState.Normal, true);
        }

        protected override void OnDisable()
        {
            base.onValueChanged.RemoveListener(ValueChanged);
            base.OnDisable();
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            
            if (!(base.gameObject.activeInHierarchy))
                return;

            foreach (var info in betterTransitions)
            {
                if (state != SelectionState.Disabled && isOn)
                {
                    var tglTr = betterToggleTransitions.FirstOrDefault(
                        (o) => o.TransitionStates != null && info.TransitionStates != null
                            && o.TransitionStates.Target == info.TransitionStates.Target
                            && o.Mode == info.Mode);

                    if (tglTr != null)
                    {
                        continue;
                    }
                }

                info.SetState(state.ToString(), instant);
            }
        }

        private void ValueChanged(bool on)
        {
            ValueChanged(on, false);
        }

        private void ValueChanged(bool on, bool immediate)
        {
            foreach(var state in betterToggleTransitions)
            {
                state.SetState((on) ? "On" : "Off", immediate);
            }
        }
        
    }
}
