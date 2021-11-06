using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class PointSelectorPopupWindow : PopupWindowContent
    {
        const float BUTTON_SIZE = 50;

        public event Action<Vector2> SelctedCallback;

        Margin margin = new Margin(10, 5, 10, 5);
        Vector2 spacing = new Vector2(5, 5);

        public override Vector2 GetWindowSize()
        {
            float btnSize = 3 * BUTTON_SIZE;

            return new Vector2(
                margin.Horizontal + 3 * spacing.x + btnSize,
                margin.Vertical + 3 * spacing.y + btnSize);
        }

        public override void OnGUI(Rect rect)
        {
            float w = BUTTON_SIZE; 
            float h = BUTTON_SIZE;

            Vector2 size = new Vector2(w, h);

            for(int x = 0; x < 3; x++)
            {
                for(int y = 0; y < 3; y++)
                {

                    Vector2 pos = new Vector2(
                        margin.Left + x * (w + spacing.x),
                        margin.Top  + y * (h + spacing.y));

                    if(GUI.Button(new Rect(pos, size), GetContent(x, y), GetStyle(x, y)))
                    {
                        if(this.SelctedCallback != null)
                        {
                            Vector2 point = new Vector2(x / 2f, 1 - (y / 2f));
                            SelctedCallback(point);
                        }
                    }

                }
            }

            GetStyle(1, 1); // reset to centered text
        }

        GUIContent GetContent(int x, int y)
        {
            return new GUIContent("♠");
        }

        GUIStyle GetStyle(int x, int y)
        {
            var style = GUI.skin.button;
            
            if(y == 0)
            {
                style.alignment = (x == 0)
                    ? TextAnchor.UpperLeft
                    : ((x == 1) ? TextAnchor.UpperCenter : TextAnchor.UpperRight);
            }
            else if(y == 1)
            {
                style.alignment = (x == 0)
                    ? TextAnchor.MiddleLeft
                    : ((x == 1) ? TextAnchor.MiddleCenter : TextAnchor.MiddleRight);
            }
            else if(y == 2)
            {
                style.alignment = (x == 0)
                    ? TextAnchor.LowerLeft
                    : ((x == 1) ? TextAnchor.LowerCenter : TextAnchor.LowerRight);
            }

            return style;
        }
    }
}
