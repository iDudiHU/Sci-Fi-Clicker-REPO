using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    [HelpURL("https://documentation.therabytes.de/better-ui/BetterLayoutElement.html")]
    [AddComponentMenu("Better UI/Layout/Better Layout Element", 30)]
    public class BetterLayoutElement : LayoutElement, IResolutionDependency
    {
        [Serializable]
        public class Settings : IScreenConfigConnection
        {
            public bool IgnoreLayout;
            public bool MinWidthEnabled, MinHeightEnabled;
            public bool PreferredWidthEnabled, PreferredHeightEnabled;
            public bool FlexibleWidthEnabled, FlexibleHeightEnabled;
            public float FlexibleWidth = 1;
            public float FlexibleHeight = 1;

            [SerializeField]
            string screenConfigName;
            public string ScreenConfigName { get { return screenConfigName; } set { screenConfigName = value; } }
        }

        [Serializable]
        public class SettingsConfigCollection : SizeConfigCollection<Settings> { }

        public Settings CurrentSettings { get { return customSettings.GetCurrentItem(settingsFallback); } }

        [SerializeField]
        Settings settingsFallback = new Settings();

        [SerializeField]
        SettingsConfigCollection customSettings = new SettingsConfigCollection();

        public FloatSizeModifier MinWidthSizer { get { return customMinWidthSizers.GetCurrentItem(minWidthSizerFallback); } }
        public FloatSizeModifier MinHeightSizer { get { return customMinHeightSizers.GetCurrentItem(minHeightSizerFallback); } }
        public FloatSizeModifier PreferredWidthSizer { get { return customPreferredWidthSizers.GetCurrentItem(preferredWidthSizerFallback); } }
        public FloatSizeModifier PreferredHeightSizer { get { return customPreferredHeightSizers.GetCurrentItem(preferredHeightSizerFallback); } }

        [SerializeField]
        FloatSizeModifier minWidthSizerFallback = new FloatSizeModifier(0, 0, 5000);
        [SerializeField]
        FloatSizeConfigCollection customMinWidthSizers = new FloatSizeConfigCollection();

        [SerializeField]
        FloatSizeModifier minHeightSizerFallback = new FloatSizeModifier(0, 0, 5000);
        [SerializeField]
        FloatSizeConfigCollection customMinHeightSizers = new FloatSizeConfigCollection();

        [SerializeField]
        FloatSizeModifier preferredWidthSizerFallback = new FloatSizeModifier(100, 0, 5000);
        [SerializeField]
        FloatSizeConfigCollection customPreferredWidthSizers = new FloatSizeConfigCollection();

        [SerializeField]
        FloatSizeModifier preferredHeightSizerFallback = new FloatSizeModifier(100, 0, 5000);
        [SerializeField]
        FloatSizeConfigCollection customPreferredHeightSizers = new FloatSizeConfigCollection();
        

        protected override void OnEnable()
        {
            base.OnEnable();
            Apply();
        }

        public void OnResolutionChanged()
        {
            Apply();
        }

        void Apply()
        {
            Settings s = CurrentSettings;

            base.ignoreLayout = s.IgnoreLayout;

            base.minWidth =         (s.MinWidthEnabled)         ? MinWidthSizer.CalculateSize(this)         : -1;
            base.minHeight =        (s.MinHeightEnabled)        ? MinHeightSizer.CalculateSize(this)        : -1;
            base.preferredWidth =   (s.PreferredWidthEnabled)   ? PreferredWidthSizer.CalculateSize(this)   : -1;
            base.preferredHeight =  (s.PreferredHeightEnabled)  ? PreferredHeightSizer.CalculateSize(this)  : -1;
            base.flexibleWidth =    (s.FlexibleWidthEnabled)    ? s.FlexibleWidth                           : -1;
            base.flexibleHeight =   (s.FlexibleHeightEnabled)   ? s.FlexibleHeight                          : -1;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Apply();
        }
#endif
    }
}
