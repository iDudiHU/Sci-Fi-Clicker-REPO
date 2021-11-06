using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheraBytes.BetterUi
{
    public enum AspectRatio
    {
        Portrait21To9,
        Portrait16To9,
        Portrait5To3,
        Portrait16To10,
        Portrait3To2,
        Portrait4To3,
        Portrait5To4,

        _,

        Landscape5To4,
        Landscape4To3,
        Landscape3To2,
        Landscape16To10,
        Landscape5To3,
        Landscape16To9,
        Landscape21To9,
    }

    public static class AspectRatioExtensions
    {
        const float LANDSCAPE5TO4 = 5f / 4f;
        const float LANDSCAPE4TO3 = 4f / 3f;
        const float LANDSCAPE3TO2 = 3f / 2f;
        const float LANDSCAPE16TO10 = 8f / 5f;
        const float LANDSCAPE5TO3 = 5f / 3f;
        const float LANDSCAPE16TO9 = 16f / 9f;
        const float LANDSCAPE21TO9 = 64f / 27f; //actual ratio

        const float PORTRAIT21TO9 = 27 / 64f; //actual ratio
        const float PORTRAIT16TO9 = 9f / 16f;
        const float PORTRAIT5TO3 = 3f / 5f;
        const float PORTRAIT16TO10 = 5f / 8f;
        const float PORTRAIT3TO2 = 2f / 3f;
        const float PORTRAIT4TO3 = 3f / 4f;
        const float PORTRAIT5TO4 = 4f / 5f;

        public static float GetRatioValue(this AspectRatio ratio)
        {
            switch (ratio)
            {

                case AspectRatio.Landscape5To4: return LANDSCAPE5TO4;
                case AspectRatio.Landscape4To3: return LANDSCAPE4TO3;
                case AspectRatio.Landscape3To2: return LANDSCAPE3TO2;
                case AspectRatio.Landscape16To10: return LANDSCAPE16TO10;
                case AspectRatio.Landscape5To3: return LANDSCAPE5TO3;
                case AspectRatio.Landscape16To9: return LANDSCAPE16TO9;
                case AspectRatio.Landscape21To9: return LANDSCAPE21TO9;

                case AspectRatio._: return 1;

                case AspectRatio.Portrait21To9: return PORTRAIT21TO9;
                case AspectRatio.Portrait16To9: return PORTRAIT16TO9;
                case AspectRatio.Portrait5To3: return PORTRAIT5TO3;
                case AspectRatio.Portrait16To10: return PORTRAIT16TO10;
                case AspectRatio.Portrait3To2: return PORTRAIT3TO2;
                case AspectRatio.Portrait4To3: return PORTRAIT4TO3;
                case AspectRatio.Portrait5To4: return PORTRAIT5TO4;

                default: throw new ArgumentException();
            }
        }

        public static string ToShortDetailedString(this AspectRatio ratio)
        {
            if (ratio == AspectRatio._)
                return "↑ Portrait    ↓ Landscape";

            string part1 = ratio.ToShortString();
            float part2 = ratio.GetRatioValue();

            return string.Format("{0}\t= {1:0.00}", part1, part2);
        }

        public static string ToShortString(this AspectRatio ratio)
        {
            switch (ratio)
            {

                case AspectRatio.Landscape5To4: return "5:4";
                case AspectRatio.Landscape4To3: return "4:3";
                case AspectRatio.Landscape3To2: return "3:2";
                case AspectRatio.Landscape16To10: return "16:10 (8:5)";
                case AspectRatio.Landscape5To3: return "5:3";
                case AspectRatio.Landscape16To9: return "16:9";
                case AspectRatio.Landscape21To9: return "21:9 (64:27)";

                case AspectRatio._: return "1:1";

                case AspectRatio.Portrait5To4: return "4:5";
                case AspectRatio.Portrait4To3: return "3:4";
                case AspectRatio.Portrait3To2: return "2:3";
                case AspectRatio.Portrait16To10: return "10:16 (5:8)";
                case AspectRatio.Portrait5To3: return "3:5";
                case AspectRatio.Portrait16To9: return "9:16";
                case AspectRatio.Portrait21To9: return "9:21 (27:64)";

                default: throw new ArgumentException();
            }
        }
    }
}
