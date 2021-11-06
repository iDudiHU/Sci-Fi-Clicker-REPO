using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class UnserializedButtonPageElement : WizardPageElementBase
    {
        GUIContent buttonContent;
        Action buttonCallback;

        public UnserializedButtonPageElement(string buttonContent, Action buttonCallback, bool completeImmediately = true)
            : this(new GUIContent(buttonContent), buttonCallback, completeImmediately)
        { }

        public UnserializedButtonPageElement(GUIContent buttonContent, Action buttonCallback, bool completeImmediately = true)
        {
            this.buttonContent = buttonContent;
            this.buttonCallback = buttonCallback;
            base.markCompleteImmediately = completeImmediately;
        }

        public override void DrawGui()
        {
            if(GUILayout.Button(buttonContent) && buttonCallback != null)
            {
                buttonCallback();
            }
        }
    }
}
