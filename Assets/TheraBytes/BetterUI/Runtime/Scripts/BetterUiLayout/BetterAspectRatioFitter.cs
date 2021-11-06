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
    [HelpURL("https://documentation.therabytes.de/better-ui/BetterAspectRatioFitter.html")]
    [AddComponentMenu("Better UI/Layout/Better Aspect Ratio Fitter", 30)]
    public class BetterAspectRatioFitter : AspectRatioFitter, IResolutionDependency
    {
        [Serializable]
        public class Settings : IScreenConfigConnection
        {
            public AspectMode AspectMode;
            public float AspectRatio = 1;

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
            base.aspectMode = CurrentSettings.AspectMode;
            base.aspectRatio = CurrentSettings.AspectRatio;
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
