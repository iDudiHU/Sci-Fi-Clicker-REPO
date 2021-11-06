using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
    [HelpURL("https://documentation.therabytes.de/better-ui/BetterSelectable.html")]
    [AddComponentMenu("Better UI/Controls/Better Selectable", 30)]
    public class BetterSelectable : Selectable, IBetterTransitionUiElement
    {
        public List<Transitions> BetterTransitions { get { return betterTransitions; } }

        [SerializeField]
        List<Transitions> betterTransitions = new List<Transitions>();

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
    }
}
