using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    [AddComponentMenu("Better UI/TextMeshPro/Better TextMeshPro - Input Field", 30)]
    public class BetterTextMeshProInputField : TMP_InputField, IBetterTransitionUiElement, IResolutionDependency
    {
        public List<Transitions> BetterTransitions { get { return betterTransitions; } }
        public List<Graphic> AdditionalPlaceholders { get { return additionalPlaceholders; } }
        public FloatSizeModifier PointSizeScaler { get { return pointSizeScaler; } }
        public bool OverridePointSizeSettings
        {
            get { return overridePointSize; }
            set { overridePointSize = value; }
        }

        [SerializeField]
        List<Transitions> betterTransitions = new List<Transitions>();

        [SerializeField]
        List<Graphic> additionalPlaceholders = new List<Graphic>();

        [SerializeField]
        FloatSizeModifier pointSizeScaler = new FloatSizeModifier(36, 10, 500);

        [SerializeField]
        bool overridePointSize;
        
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

        protected override void OnEnable()
        {
            CalculateSize();
            base.OnEnable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            CalculateSize();
        }

        public void OnResolutionChanged()
        {
            CalculateSize();
        }

        public void CalculateSize()
        {
            if (overridePointSize)
                base.pointSize = pointSizeScaler.CalculateSize(this);

            OverrideBetterTextMeshSize(base.m_Placeholder as BetterTextMeshProUGUI, pointSize);
            OverrideBetterTextMeshSize(base.m_TextComponent as BetterTextMeshProUGUI, pointSize);

            foreach(var p in additionalPlaceholders)
            {
                OverrideBetterTextMeshSize(p as BetterTextMeshProUGUI, pointSize);
            }
        }

        void OverrideBetterTextMeshSize(BetterTextMeshProUGUI better, float size)
        {
            if (better == null)
                return;

            better.IgnoreFontSizerOptions = overridePointSize;

            if (overridePointSize)
            {
                better.FontSizer.OverrideLastCalculatedSize(size);
                better.fontSize = size;
            }
            else
            {
                better.FontSizer.CalculateSize(this);
            }

        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            CalculateSize();
            base.OnValidate();
        }
#endif

    }
}