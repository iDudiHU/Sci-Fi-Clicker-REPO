using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class ScreenTypeConditions
    {
        [SerializeField]
        string name = "Screen";

        public string Name
        {
            get { return name; }
            set { this.name = value; }
        }

        [SerializeField]
        IsCertainScreenOrientation checkOrientation;

        [SerializeField]
        IsScreenOfCertainSize checkScreenSize;

        [SerializeField]
        IsCertainAspectRatio checkAspectRatio;

        [SerializeField]
        IsScreenOfCertainDeviceInfo checkDeviceType;

        [SerializeField]
        IsScreenTagPresent checkScreenTag;

        [SerializeField]
        ScreenInfo optimizedScreenInfo;

        [SerializeField]
        List<string> fallbacks = new List<string>();

        public bool IsActive { get; private set; }

        public List<string> Fallbacks { get { return fallbacks; } }

        public Vector2 OptimizedResolution { get { return (optimizedScreenInfo != null) ? optimizedScreenInfo.Resolution : ResolutionMonitor.OptimizedResolutionFallback; } }

        public int OptimizedWidth { get { return (int)OptimizedResolution.x; } }
        public int OptimizedHeight { get { return (int)OptimizedResolution.y; } }

        public float OptimizedDpi { get { return (optimizedScreenInfo != null) ? optimizedScreenInfo.Dpi : ResolutionMonitor.OptimizedDpiFallback; } }

        public IsCertainScreenOrientation CheckOrientation
        {
            get { return checkOrientation; }
        }

        public IsScreenOfCertainSize CheckScreenSize
        {
            get { return checkScreenSize; }
        }

        public IsCertainAspectRatio CheckAspectRatio
        {
            get { return checkAspectRatio; }
        }

        public IsScreenOfCertainDeviceInfo CheckDeviceType
        {
            get { return checkDeviceType; }
        }
        public IsScreenTagPresent CheckScreenTag
        {
            get { return checkScreenTag; }
        }


        public ScreenInfo OptimizedScreenInfo
        {
            get { return optimizedScreenInfo; }
        }

        public ScreenTypeConditions(string displayName, params Type[] enabledByDefault)
        {
            this.name = displayName;
            this.optimizedScreenInfo = new ScreenInfo(new Vector2(1920, 1080), 96);
            EnsureScreenConditions(enabledByDefault);
        }

        private void EnsureScreenConditions(params Type[] enabledByDefault)
        {
            EnsureScreenCondition(ref checkOrientation, () => new IsCertainScreenOrientation(IsCertainScreenOrientation.Orientation.Landscape), enabledByDefault);
            EnsureScreenCondition(ref checkScreenSize, () => new IsScreenOfCertainSize(), enabledByDefault);
            EnsureScreenCondition(ref checkAspectRatio, () => new IsCertainAspectRatio(), enabledByDefault);
            EnsureScreenCondition(ref checkDeviceType, () => new IsScreenOfCertainDeviceInfo(), enabledByDefault);
            EnsureScreenCondition(ref checkScreenTag, () => new IsScreenTagPresent(), enabledByDefault);
        }

        private void EnsureScreenCondition<T>(ref T screenCondition, Func<T> instantiatoMethod, Type[] enabledTypes)
            where T : IIsActive
        {
            if (screenCondition != null)
                return;

            screenCondition = instantiatoMethod();
            screenCondition.IsActive = enabledTypes.Contains(typeof(T));
        }

        public bool IsScreenType()
        {
            EnsureScreenConditions();

            IsActive = (!(checkOrientation.IsActive)    || checkOrientation.IsScreenType())
                && (!(checkScreenSize.IsActive)         || checkScreenSize.IsScreenType())
                && (!(checkAspectRatio.IsActive)        || checkAspectRatio.IsScreenType())
                && (!(checkDeviceType.IsActive)         || checkDeviceType.IsScreenType())
                && (!(checkScreenTag.IsActive)          || checkScreenTag.IsScreenType());

            return IsActive;
        }

    }
}
