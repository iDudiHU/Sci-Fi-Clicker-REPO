using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class InfoWithButtonWizardPageElement : WizardPageElementBase
    {
        public static float ButtonWidth = 100;

        public class ButtonInfo
        {
            public string ButtonText { get; private set; }
            public event Action ClickCallback;

            public ButtonInfo(string buttonText, Action clickCallback)
            {
                this.ButtonText = buttonText;
                this.ClickCallback = clickCallback;
            }

            public void Draw(float width)
            {
                if (GUILayout.Button(ButtonText, GUILayout.Width(width), GUILayout.ExpandHeight(true)))
                {
                    if (ClickCallback != null)
                    {
                        ClickCallback();
                    }
                }
            }
        }

        string text;
        ButtonInfo[] buttons;

        public InfoWithButtonWizardPageElement(string text, string buttonText, Action buttonClickCallback)
            : this(text, new ButtonInfo(buttonText, buttonClickCallback))
        {

        }
        public InfoWithButtonWizardPageElement(string text, params ButtonInfo[] buttons)
        {
            this.text = text;
            this.buttons = buttons;

            markCompleteImmediately = true;
        }

        public override void DrawGui()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField(text, EditorStyles.wordWrappedLabel);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();


            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();

            foreach (var btn in buttons)
            {
                btn.Draw(ButtonWidth);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

    }

}
