using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
    [AddComponentMenu("Better UI/TextMeshPro/Better TextMeshPro - Dropdown", 30)]
    public class BetterTextMeshProDropdown : TMP_Dropdown, IBetterTransitionUiElement
    {
        public List<Transitions> BetterTransitions { get { return betterTransitions; } }
        public List<Transitions> ShowHideTransitions { get { return showHideTransitions; } }

        [SerializeField]
        List<Transitions> betterTransitions = new List<Transitions>();

        [SerializeField]
        List<Transitions> showHideTransitions = new List<Transitions>();

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

        protected override GameObject CreateDropdownList(GameObject template)
        {
            foreach (var tr in showHideTransitions)
            {
                tr.SetState("Show", false);
            }

            return base.CreateDropdownList(template);
        }

        protected override void DestroyDropdownList(GameObject dropdownList)
        {
            foreach (var tr in showHideTransitions)
            {
                tr.SetState("Hide", false);
            }

            base.DestroyDropdownList(dropdownList);
        }
    }
}