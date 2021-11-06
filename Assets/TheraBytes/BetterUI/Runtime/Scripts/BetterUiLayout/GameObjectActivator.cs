using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    [HelpURL("https://documentation.therabytes.de/better-ui/GameObjectActivator.html")]
    [AddComponentMenu("Better UI/Helpers/Game Object Activator", 30)]
    public class GameObjectActivator : UIBehaviour, IResolutionDependency
    {
        [Serializable]
        public class Settings : IScreenConfigConnection
        {
            public List<GameObject> ActiveObjects = new List<GameObject>();
            public List<GameObject> InactiveObjects = new List<GameObject>();

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

        public void Apply()
        {
#if UNITY_EDITOR
            if (!(EditorPreview) && !(UnityEditor.EditorApplication.isPlaying))
                return;
#endif
            foreach (GameObject go in CurrentSettings.ActiveObjects)
            {
                if (go != null)
                {
                    go.SetActive(true);
                }
            }

            foreach (GameObject go in CurrentSettings.InactiveObjects)
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
        }

#if UNITY_EDITOR
        public bool EditorPreview { get; set; }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (EditorPreview)
            {
                Apply();
            }
        }
#endif
    }
    
}
