using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class IsScreenOfCertainDeviceInfo : IScreenTypeCheck
    {
        public enum DeviceInfo
        {
            Other,
            Handheld,
            Console,
            Desktop,
            TouchScreen,
            VirtualReality
        }

        [SerializeField]
        DeviceInfo expectedDeviceInfo;
        public DeviceInfo ExpectedDeviceInfo { get { return expectedDeviceInfo; } set { expectedDeviceInfo = value; } }

        [SerializeField]
        bool isActive;
        public bool IsActive { get { return isActive; } set { isActive = value; } }

        public bool IsScreenType()
        {
            switch (expectedDeviceInfo)
            {
                case DeviceInfo.Other:
                    return SystemInfo.deviceType == DeviceType.Unknown
#if XR
                        && !(UnityEngine.XR.XRDevice.isPresent)
#endif
                        && !(TouchScreenKeyboard.isSupported);

                case DeviceInfo.Handheld:
                    return SystemInfo.deviceType == DeviceType.Handheld;

                case DeviceInfo.Console:
                    return SystemInfo.deviceType == DeviceType.Console;

                case DeviceInfo.Desktop:
                    return SystemInfo.deviceType == DeviceType.Desktop;

                case DeviceInfo.TouchScreen:
                    return TouchScreenKeyboard.isSupported;

                case DeviceInfo.VirtualReality:
#if XR
                    return UnityEngine.XR.XRDevice.isPresent;
#else
                    return false;
#endif

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
