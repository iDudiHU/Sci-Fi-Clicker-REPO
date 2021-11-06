using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheraBytes.BetterUi
{
    [HelpURL("https://documentation.therabytes.de/better-ui/IngameResolutionMonitor.html")]
    [AddComponentMenu("Better UI/In-Game Resolution Monitor", 30)]
    public class IngameResolutionMonitor : MonoBehaviour
    {
        static IngameResolutionMonitor instance;

        [SerializeField] bool onlyPresentInThisScene= false;

        public static GameObject Create()
        {
            GameObject go = new GameObject("IngameResolutionMonitor");
            go.AddComponent<IngameResolutionMonitor>();

            return go;
        }

        private void OnEnable()
        {
            if(instance != null)
            {
                Debug.LogWarning("There already is an Ingame Resolution Monitor. One is enough. Destroying the previous one now...");
                GameObject.Destroy(instance.gameObject);
            }

            instance = this;

            if (!onlyPresentInThisScene)
            {
                GameObject.DontDestroyOnLoad(this.gameObject);
            }

            SceneManager.sceneLoaded += SceneLoaded;
        }

        private void OnDisable()
        {
            instance = null;
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResolutionMonitor.MarkDirty();
            ResolutionMonitor.Update();
        }

#if !(UNITY_EDITOR)
        void Update()
        {
            ResolutionMonitor.Update();
        }
#endif
    }
}
