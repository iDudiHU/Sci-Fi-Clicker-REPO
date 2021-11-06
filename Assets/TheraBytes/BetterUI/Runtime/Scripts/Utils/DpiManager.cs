using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class DpiManager
    {

#if UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern double GetDPI();

#endif

        [Serializable]
        public class DpiOverride
        {
            [SerializeField] float dpi = 96;
            [SerializeField] string deviceModel;

            public float Dpi { get { return dpi; } }
            public string DeviceModel { get { return deviceModel; } }

            public DpiOverride(string deviceModel, float dpi)
            {
                this.deviceModel = deviceModel;
                this.dpi = dpi;
            }
        }

        [SerializeField]
        List<DpiOverride> overrides = new List<DpiOverride>();

        public float GetDpi()
        {
            DpiOverride ov = overrides.FirstOrDefault(o => o.DeviceModel == SystemInfo.deviceModel);

            if (ov != null)
                return ov.Dpi;

#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // Fix Web GL Dpi bug (Unity thinks it is always 96 DPI)
                return (float)(GetDPI() * 96.0f);
            }
            catch
            {
                Debug.LogError("Could not retrieve real DPI. Is the WebGL-DPI-Plugin installed in the project?");
            }
#endif
            return Screen.dpi;
        }
    }
}
