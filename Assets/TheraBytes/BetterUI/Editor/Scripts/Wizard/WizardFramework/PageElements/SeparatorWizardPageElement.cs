using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class SeparatorWizardPageElement : WizardPageElementBase
    {
        public SeparatorWizardPageElement()
        {
            markCompleteImmediately = true;
        }

        public override void DrawGui()
        {
            EditorGUILayout.Separator();
        }

    }
}
