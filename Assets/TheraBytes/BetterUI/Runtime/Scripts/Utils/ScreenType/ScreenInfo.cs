using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class ScreenInfo
    {
        [SerializeField]
        Vector2 resolution = new Vector2(1980, 1020);

        [SerializeField]
        float dpi = 96;

        public Vector2 Resolution
        {
            get { return resolution; }
            set { resolution = value; }
        }

        public float Dpi
        {
            get { return dpi; }
            set { dpi = value; }
        }

        public ScreenInfo()
        {
        }

        public ScreenInfo(Vector2 resolution, float dpi)
        {
            this.resolution = resolution;
            this.dpi = dpi;
        }
    }
}
