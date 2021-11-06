using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

#pragma warning disable 0649 // never assigned warning

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif

    [HelpURL("https://documentation.therabytes.de/better-ui/OverrideScreenProperties.html")]
    [AddComponentMenu("Better UI/Layout/Override Screen Properties", 30)]
    public class OverrideScreenProperties : UIBehaviour, IResolutionDependency
    {
        public enum ScreenProperty
        {
            Width,
            Height,
            Dpi,
        }

        public enum OverrideMode
        {
            Override,
            Inherit,
            ActualScreenProperty,
        }

        [Serializable]
        public class Settings : IScreenConfigConnection
        {
            [Serializable]
            public class OverrideProperty
            {
                [SerializeField]
                OverrideMode mode;

                [SerializeField]
                float value;

                public OverrideMode Mode { get { return mode; } }
                public float Value { get { return value; } }
            }

            public OverrideProperty OptimizedWidthOverride;
            public OverrideProperty OptimizedHeightOverride;
            public OverrideProperty OptimizedDpiOverride;

            public IEnumerable<OverrideProperty> PropertyIterator()
            {
                yield return OptimizedWidthOverride;
                yield return OptimizedHeightOverride;
                yield return OptimizedDpiOverride;
            }

            public OverrideProperty this[ScreenProperty property]
            {
                get
                {
                    switch (property)
                    {
                        case ScreenProperty.Width: return OptimizedWidthOverride;
                        case ScreenProperty.Height: return OptimizedHeightOverride;
                        case ScreenProperty.Dpi: return OptimizedDpiOverride;
                        default: throw new ArgumentException();
                    }
                }
            }

            [SerializeField]
            string screenConfigName;
            public string ScreenConfigName { get { return screenConfigName; } set { screenConfigName = value; } }
        }

        [Serializable]
        public class SettingsConfigCollection : SizeConfigCollection<Settings> { }

        
        [SerializeField]
        Settings settingsFallback = new Settings();

        [SerializeField]
        SettingsConfigCollection customSettings = new SettingsConfigCollection();

        public Settings CurrentSettings { get { return customSettings.GetCurrentItem(settingsFallback); } }

#if UNITY_EDITOR
        public SettingsConfigCollection SettingsList { get { return customSettings; } }
        public Settings FallbackSettings { get { return settingsFallback; } }
#endif

        ScreenInfo optimizedOverride = new ScreenInfo();
        ScreenInfo currentOverride = new ScreenInfo();

        public ScreenInfo OptimizedOverride { get { return optimizedOverride; } }
        public ScreenInfo CurrentSize { get { return currentOverride; } }

        protected override void OnEnable()
        {
            base.OnEnable();
            OnResolutionChanged();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            OnResolutionChanged();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            OnResolutionChanged();
        }

        public void OnResolutionChanged()
        {
            // unfortunately the dimension is updated delayed and the UI-interfaces do not work (at least not for all supported unity versions)
            // so we simply wait a frame, before updating the sizes.

            StopAllCoroutines();
            StartCoroutine(RecalculateRoutine());
        }

        IEnumerator RecalculateRoutine()
        {
            yield return null;

            var settings = customSettings.GetCurrentItem(settingsFallback);
            Recalculate(settings);

            // let all children recalculate now
            InformChildren();
        }

        private void Recalculate(Settings settings)
        {
            OverrideScreenProperties parent = (settings.PropertyIterator().Any(o => o.Mode == OverrideMode.Inherit)) 
                ? this.GetComponentInParent<OverrideScreenProperties>()
                : null;

            float optimizedWidth  = CalculateOptimizedValue(settings, ScreenProperty.Width, parent);
            float optimizedHeight = CalculateOptimizedValue(settings, ScreenProperty.Height, parent);
            float optimizedDpi    = CalculateOptimizedValue(settings, ScreenProperty.Dpi, parent);

            optimizedOverride.Resolution = new Vector2(optimizedWidth, optimizedHeight);
            optimizedOverride.Dpi = optimizedDpi;

            Rect rect = new Rect();
            if (settings.PropertyIterator().Any(o => o.Mode == OverrideMode.Override))
            {
                rect = (this.transform as RectTransform).rect;
            }; 


            float currentWidth = CalculateCurrentValue(settings, ScreenProperty.Width, parent, rect);
            float currentHeight = CalculateCurrentValue(settings, ScreenProperty.Height, parent, rect);
            float currentDpi = CalculateCurrentValue(settings, ScreenProperty.Dpi, parent, rect);

            currentOverride.Resolution = new Vector2(currentWidth, currentHeight);
            currentOverride.Dpi = currentDpi;
        }

        public float CalculateOptimizedValue(Settings settings, ScreenProperty property, OverrideScreenProperties parent)
        {
            switch(settings[property].Mode)
            {
                case OverrideMode.Override:
                    return settings[property].Value;

                case OverrideMode.Inherit:
                    if (parent != null)
                    {
                        switch (parent.CurrentSettings[property].Mode)
                        {
                            case OverrideMode.Override:
                                return parent.CurrentSettings[property].Value;

                            case OverrideMode.Inherit:
                                OverrideScreenProperties parentParent = parent.GetComponentsInParent<OverrideScreenProperties>().FirstOrDefault(o => o.gameObject != this.gameObject);
                                return parent.CalculateOptimizedValue(parent.CurrentSettings, property, parentParent);

                            case OverrideMode.ActualScreenProperty: break;
                        }
                    }

                    // If parent is null or parent uses actual screen property: 
                    // Fall through!
                    goto case OverrideMode.ActualScreenProperty;
                    
                case OverrideMode.ActualScreenProperty:
                    ScreenInfo info = ResolutionMonitor.GetOpimizedScreenInfo(settings.ScreenConfigName);
                    switch (property)
                    {
                        case ScreenProperty.Width: return info.Resolution.x;
                        case ScreenProperty.Height: return info.Resolution.y;
                        case ScreenProperty.Dpi: return info.Dpi;
                        default: throw new ArgumentException(); ;
                    }
            }

            throw new ArgumentException();
        }
        private float CalculateCurrentValue(Settings settings, ScreenProperty property, OverrideScreenProperties parent, Rect rect)
        {
            switch (settings[property].Mode)
            {
                case OverrideMode.Override:
                    switch (property)
                    {
                        case ScreenProperty.Width: return rect.width;
                        case ScreenProperty.Height: return rect.height;
                        case ScreenProperty.Dpi: break; 
                    };

                    // DPI case: Fall through!
                    goto case OverrideMode.ActualScreenProperty;

                case OverrideMode.Inherit:
                    if (parent != null)
                    {
                        switch (parent.CurrentSettings[property].Mode)
                        {
                            case OverrideMode.Override:
                                Rect parentRect = (parent.transform as RectTransform).rect;
                                return parent.CalculateCurrentValue(parent.CurrentSettings, property, null, parentRect);

                            case OverrideMode.Inherit:
                                OverrideScreenProperties parentParent = parent.GetComponentsInParent<OverrideScreenProperties>().FirstOrDefault(o => o.gameObject != this.gameObject);
                                return parent.CalculateCurrentValue(parent.CurrentSettings, property, parentParent, new Rect());

                            case OverrideMode.ActualScreenProperty: break;
                        }
                    }

                    // If parent is null or parent uses actual screen property: 
                    // Fall through!
                    goto case OverrideMode.ActualScreenProperty;

                case OverrideMode.ActualScreenProperty:
                    switch (property)
                    {
                        case ScreenProperty.Width: return ResolutionMonitor.CurrentResolution.x;
                        case ScreenProperty.Height: return ResolutionMonitor.CurrentResolution.y;
                        case ScreenProperty.Dpi: return ResolutionMonitor.CurrentDpi;
                        default: throw new ArgumentException(); ;
                    }
            }

            throw new ArgumentException();
        }


        public void InformChildren()
        {
            var resDeps = this.GetComponentsInChildren<Component>().OfType<IResolutionDependency>();
            foreach (IResolutionDependency comp in resDeps)
            {
                if (comp.Equals(this))
                    continue;

                comp.OnResolutionChanged();
            }

        }

    }
}

#pragma warning restore 0649
