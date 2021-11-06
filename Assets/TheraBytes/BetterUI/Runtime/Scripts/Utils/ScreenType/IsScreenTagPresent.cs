using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class IsScreenTagPresent : IScreenTypeCheck
    {
        [SerializeField]
        string screenTag;
        public string ScreenTag { get { return screenTag; } set { screenTag = value; } }

        [SerializeField]
        bool isActive;

        public bool IsActive { get { return isActive; } set { isActive = value; } }

        public bool IsScreenType()
        {
            var curentTags = ResolutionMonitor.CurrentScreenTags as HashSet<string>;
            return curentTags.Contains(screenTag);
        }
    }
}
