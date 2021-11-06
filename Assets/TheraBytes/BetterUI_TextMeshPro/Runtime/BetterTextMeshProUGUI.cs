using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    [AddComponentMenu("Better UI/TextMeshPro/Better TextMeshPro Text", 30)]
    public class BetterTextMeshProUGUI : TextMeshProUGUI, IResolutionDependency
    {
        public BetterText.FittingMode Fitting
        {
            get { return fitting; }
            set
            {
                if (fitting == value)
                    return;

                fitting = value;
                CalculateSize();
            }
        }

        public MarginSizeModifier MarginSizer { get { return customMarginSizers.GetCurrentItem(marginSizerFallback); } }

        public FloatSizeModifier FontSizer { get { return customFontSizers.GetCurrentItem(fontSizerFallback); } }
        public FloatSizeModifier MinFontSizer { get { return customMinFontSizers.GetCurrentItem(minFontSizerFallback); } }
        public FloatSizeModifier MaxFontSizer { get { return customMaxFontSizers.GetCurrentItem(maxFontSizerFallback); } }

        public bool IgnoreFontSizerOptions { get; set; }

        [SerializeField]
        BetterText.FittingMode fitting;

        [FormerlySerializedAs("marginSizer")]
        [SerializeField]
        MarginSizeModifier marginSizerFallback =
            new MarginSizeModifier(new Margin(), new Margin(), new Margin(1000, 1000, 1000, 1000));

        [SerializeField]
        MarginSizeConfigCollection customMarginSizers = new MarginSizeConfigCollection();


        [FormerlySerializedAs("fontSizer")]
        [SerializeField]
        FloatSizeModifier fontSizerFallback = new FloatSizeModifier(36, 10, 500);

        [SerializeField]
        FloatSizeConfigCollection customFontSizers = new FloatSizeConfigCollection();


        [FormerlySerializedAs("minFontSizer")]
        [SerializeField]
        FloatSizeModifier minFontSizerFallback = new FloatSizeModifier(10, 10, 500);

        [SerializeField]
        FloatSizeConfigCollection customMinFontSizers = new FloatSizeConfigCollection();


        [FormerlySerializedAs("maxFontSizer")]
        [SerializeField]
        FloatSizeModifier maxFontSizerFallback = new FloatSizeModifier(500, 500, 500);

        [SerializeField]
        FloatSizeConfigCollection customMaxFontSizers = new FloatSizeConfigCollection();

        protected override void OnEnable()
        {
            CalculateSize();
            base.OnEnable();
        }

        public void OnResolutionChanged()
        {
            CalculateSize();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            CalculateSize();
        }


        public void CalculateSize()
        {
            if (IgnoreFontSizerOptions)
            {
                base.enableAutoSizing = false;
            }
            else
            {
                switch (fitting)
                {
                    case BetterText.FittingMode.SizerOnly:

                        base.enableAutoSizing = false;
                        base.fontSize = FontSizer.CalculateSize(this);
                        break;

                    case BetterText.FittingMode.StayInBounds:

                        base.enableAutoSizing = true;
                        base.fontSizeMin = MinFontSizer.CalculateSize(this);
                        base.fontSizeMax = FontSizer.CalculateSize(this);
                        break;

                    case BetterText.FittingMode.BestFit:

                        base.enableAutoSizing = true;
                        base.fontSizeMin = MinFontSizer.CalculateSize(this);
                        base.fontSizeMax = MaxFontSizer.CalculateSize(this);
                        break;
                }
            }

            base.margin = MarginSizer.CalculateSize(this).ToVector4();
        }

        public void RegisterMaterials(Material[] materials)
        {
            base.GetMaterials(materials);
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

