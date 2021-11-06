using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{   
    [HelpURL("https://documentation.therabytes.de/better-ui/BetterInputField.html")]
    [AddComponentMenu("Better UI/Controls/Better Input Field", 30)]
    public class BetterInputField : InputField, IBetterTransitionUiElement
    {
        public List<Transitions> BetterTransitions { get { return betterTransitions; } }
        public List<Graphic> AdditionalPlaceholders { get { return additionalPlaceholders; } }

        [SerializeField]
        List<Transitions> betterTransitions = new List<Transitions>();

        [SerializeField]
        List<Graphic> additionalPlaceholders = new List<Graphic>();
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (!(base.gameObject.activeInHierarchy))
                return;

            foreach (var info in betterTransitions)
            {
                info.SetState(state.ToString(), instant);
            }
        }
        
        public override void OnUpdateSelected(BaseEventData eventData)
        {
            base.OnUpdateSelected(eventData);
            DisplayPlaceholders(this.text);
        }

        void DisplayPlaceholders(string input)
        {
            bool show = string.IsNullOrEmpty(input);

            if (Application.isPlaying)
            {
                foreach (var ph in additionalPlaceholders)
                {
                    ph.enabled = show;
                }
            }
        }
    }
}
