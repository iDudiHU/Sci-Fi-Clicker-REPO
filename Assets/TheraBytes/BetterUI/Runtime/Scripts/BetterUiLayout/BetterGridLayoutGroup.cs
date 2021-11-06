using System;
using System.Collections;
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
    [HelpURL("https://documentation.therabytes.de/better-ui/BetterGridLayoutGroup.html")]
    [AddComponentMenu("Better UI/Layout/Better Grid Layout Group", 30)]
    public class BetterGridLayoutGroup : GridLayoutGroup, IResolutionDependency
    {
        [Serializable]
        public class Settings : IScreenConfigConnection
        {
            public Constraint Constraint;
            public int ConstraintCount;
            public TextAnchor ChildAlignment;
            public Axis StartAxis;
            public Corner StartCorner;
            public bool Fit;

            [SerializeField]
            string screenConfigName;
            public string ScreenConfigName { get { return screenConfigName; } set { screenConfigName = value; } }

            public Settings(BetterGridLayoutGroup grid)
            {
                this.Constraint = grid.m_Constraint;
                this.ConstraintCount = grid.m_ConstraintCount;
                this.ChildAlignment = grid.childAlignment;
                this.StartAxis = grid.m_StartAxis;
                this.StartCorner = grid.m_StartCorner;
                this.Fit = grid.fit;
            }

        }

        [Serializable]
        public class SettingsConfigCollection : SizeConfigCollection<Settings> { }


        public MarginSizeModifier PaddingSizer { get { return customPaddingSizers.GetCurrentItem(paddingSizerFallback); } }
        public Vector2SizeModifier CellSizer { get { return customCellSizers.GetCurrentItem(cellSizerFallback); } }
        public Vector2SizeModifier SpacingSizer { get { return customSpacingSizers.GetCurrentItem(spacingSizerFallback); } }
        public Settings CurrentSettings { get { return customSettings.GetCurrentItem(settingsFallback); } }

        public bool Fit
        {
            get { return fit; }
            set
            {
                if (fit == value)
                    return;

                fit = value;
                CalculateCellSize();
            }
        }

        [FormerlySerializedAs("paddingSizer")]
        [SerializeField]
        MarginSizeModifier paddingSizerFallback =
            new MarginSizeModifier(new Margin(), new Margin(), new Margin(1000, 1000, 1000, 1000));

        [SerializeField]
        MarginSizeConfigCollection customPaddingSizers = new MarginSizeConfigCollection();

        [FormerlySerializedAs("cellSizer")]
        [SerializeField]
        Vector2SizeModifier cellSizerFallback =
            new Vector2SizeModifier(new Vector2(100, 100), new Vector2(10, 10), new Vector2(300, 300));

        [SerializeField]
        Vector2SizeConfigCollection customCellSizers = new Vector2SizeConfigCollection();

        [FormerlySerializedAs("spacingSizer")]
        [SerializeField]
        Vector2SizeModifier spacingSizerFallback =
            new Vector2SizeModifier(Vector2.zero, Vector2.zero, new Vector2(300, 300));

        [SerializeField]
        Vector2SizeConfigCollection customSpacingSizers = new Vector2SizeConfigCollection();

        [SerializeField]
        Settings settingsFallback;

        [SerializeField]
        SettingsConfigCollection customSettings = new SettingsConfigCollection();

        [SerializeField]
        bool fit = false;

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            CalculateCellSize();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (settingsFallback == null || string.IsNullOrEmpty(settingsFallback.ScreenConfigName))
            {
                StartCoroutine(InitDelayed());
            }
            else
            {
                CalculateCellSize();
            }
        }

        IEnumerator InitDelayed()
        {
            yield return null;

            settingsFallback = new Settings(this)
            {
                ScreenConfigName = "Fallback",
            };

            CalculateCellSize();
        }
        
        public void OnResolutionChanged()
        {
            CalculateCellSize();

            // for fit mode we need to calculate it again because of unity internal stuff...
            if (fit)
            {
                base.SetDirty();
                CalculateCellSize();
            }
        }
        
        public void CalculateCellSize()
        {

            Rect r = this.rectTransform.rect;
            if (r.width == float.NaN || r.height == float.NaN)
                return;

            this.ApplySettings(CurrentSettings);
            
            base.m_Spacing = SpacingSizer.CalculateSize(this);

            Margin pad = PaddingSizer.CalculateSize(this);
            pad.CopyValuesTo(base.m_Padding);

            // cell size
            CellSizer.CalculateSize(this);

            if(fit)
            {
                Vector2 size = CellSizer.LastCalculatedSize;

                switch (base.constraint)
                {
                    case Constraint.FixedColumnCount:
                        
                        size.x = GetCellWidth();
                        break;

                    case Constraint.FixedRowCount:

                        size.y = GetCellHeight();
                        break;
                }

                CellSizer.OverrideLastCalculatedSize(size);
            }
            
            base.m_CellSize = CellSizer.LastCalculatedSize;
        }
        
        
        public float GetCellWidth()
        {
            float space = this.rectTransform.rect.width 
                - base.padding.horizontal 
                - base.constraintCount * base.spacing.x;

            return space / constraintCount;
        }

        public float GetCellHeight()
        {
            float space = this.rectTransform.rect.height
                - base.padding.vertical
                - base.constraintCount * base.spacing.y;

            return space / constraintCount;
        }


        void ApplySettings(Settings settings)
        {
            if (settingsFallback == null)
                return;

            this.m_Constraint = settings.Constraint;
            this.m_ConstraintCount = settings.ConstraintCount;
            this.m_ChildAlignment = settings.ChildAlignment;
            this.m_StartAxis = settings.StartAxis;
            this.m_StartCorner = settings.StartCorner;
            this.fit = settings.Fit;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            CalculateCellSize();
            base.OnValidate();
        }
#endif
    }
}
