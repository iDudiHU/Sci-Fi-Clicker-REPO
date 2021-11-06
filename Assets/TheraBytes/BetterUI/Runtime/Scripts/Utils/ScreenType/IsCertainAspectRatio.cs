using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class IsCertainAspectRatio : IScreenTypeCheck
    {

        [SerializeField]
        float minAspect = 0.66f;

        [SerializeField]
        float maxAspect = 1.5f;

        [SerializeField]
        bool inverse;

        public float MinAspect { get { return minAspect; } set { minAspect = value; } }
        public float MaxAspect { get { return maxAspect; } set { maxAspect = value; } }
        public bool Inverse { get { return inverse; } set { inverse = value; } }

        [SerializeField]
        bool isActive;
        public bool IsActive { get { return isActive; } set { isActive = value; } }

        public bool IsScreenType()
        {
            float realAspect = (float)Screen.width / Screen.height;

            return (!(inverse) 
                    && realAspect >= minAspect
                    && realAspect <= maxAspect)
                || ((inverse)
                    && realAspect < minAspect
                    && realAspect > maxAspect);
        }

       
    }
}
