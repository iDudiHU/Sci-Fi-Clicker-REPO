using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    [HelpURL("https://documentation.therabytes.de/better-ui/BetterText.html")]
    [AddComponentMenu("Better UI/Controls/Better Text", 30)]
    public class BetterText : Text, IResolutionDependency
    {
        public enum FittingMode
        {
            SizerOnly,
            StayInBounds,
            BestFit,
        }

        public FloatSizeModifier FontSizer { get { return customFontSizers.GetCurrentItem(fontSizerFallback); } }
        public FittingMode Fitting { get { return fitting; } set { fitting = value; CalculateSize(); } }
        

        [SerializeField]
        FittingMode fitting = FittingMode.StayInBounds;

        [FormerlySerializedAs("fontSizer")]
        [SerializeField]
        FloatSizeModifier fontSizerFallback = new FloatSizeModifier(40, 0, 500);

        [SerializeField]
        FloatSizeConfigCollection customFontSizers = new FloatSizeConfigCollection();

        bool isCalculatingSize;

        protected override void OnEnable()
        {
            base.OnEnable();
            CalculateSize();
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

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            CalculateSize();
        }

        void CalculateSize()
        {
            if (isCalculatingSize)
                return;

            isCalculatingSize = true;

            switch (fitting)
            {
                case FittingMode.SizerOnly:

                    base.resizeTextForBestFit = false;
                    base.fontSize = Mathf.RoundToInt(FontSizer.CalculateSize(this));
                    break;

                case FittingMode.StayInBounds:

                    base.resizeTextMinSize = Mathf.RoundToInt(FontSizer.MinSize);
                    base.resizeTextMaxSize = Mathf.RoundToInt(FontSizer.MaxSize);
                    base.resizeTextForBestFit = true;
                    int size = Mathf.RoundToInt(FontSizer.CalculateSize(this));
                    
                    base.fontSize = size;
                    base.Rebuild(CanvasUpdate.PreRender);

                    int bestFit = base.cachedTextGenerator.fontSizeUsedForBestFit;
                    base.resizeTextForBestFit = false;

                    fontSize = (bestFit < size) ? bestFit : size;
                    FontSizer.OverrideLastCalculatedSize(base.fontSize);

                    break;
                    
                case FittingMode.BestFit:

                    base.resizeTextMinSize = Mathf.RoundToInt(FontSizer.MinSize);
                    base.resizeTextMaxSize = Mathf.RoundToInt(FontSizer.MaxSize);
                    base.resizeTextForBestFit = true;

                    base.Rebuild(CanvasUpdate.PreRender);

                    FontSizer.OverrideLastCalculatedSize(base.cachedTextGenerator.fontSizeUsedForBestFit);
                    break;

                default:
                    break;
            }
            
            isCalculatingSize = false;
            
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
