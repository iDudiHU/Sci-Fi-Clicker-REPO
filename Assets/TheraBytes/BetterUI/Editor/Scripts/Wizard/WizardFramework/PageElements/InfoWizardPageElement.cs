using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public enum InfoType
    {
        Text,
        Header,
        Box,
        InfoBox,
        WarningBox,
        ErrorBox,
    }

    public class InfoWizardPageElement : WizardPageElementBase
    {
        GUIContent content;
        InfoType infoType;

        public InfoWizardPageElement(GUIContent content)
        {
            this.content = content;
            this.infoType = InfoType.Text;
            markCompleteImmediately = true;
        }

        public InfoWizardPageElement(string content, InfoType infoType = InfoType.Text)
        {
            this.content = new GUIContent(content);
            this.infoType = infoType;
            markCompleteImmediately = true;
        }

        public override void DrawGui()
        {
            switch (infoType)
            {
                case InfoType.Text:
                    EditorGUILayout.LabelField(content, EditorStyles.wordWrappedLabel);
                    break;

                case InfoType.Header:
                    EditorGUILayout.LabelField(content, EditorStyles.boldLabel);
                    break;

                case InfoType.Box:
                    EditorGUILayout.HelpBox(content.text, MessageType.None);
                    break;

                case InfoType.InfoBox:
                    EditorGUILayout.HelpBox(content.text, MessageType.Info);
                    break;

                case InfoType.WarningBox:
                    EditorGUILayout.HelpBox(content.text, MessageType.Warning);
                    break;

                case InfoType.ErrorBox:
                    EditorGUILayout.HelpBox(content.text, MessageType.Error);
                    break;

                default: throw new NotImplementedException();
            }
        }

    }
}
