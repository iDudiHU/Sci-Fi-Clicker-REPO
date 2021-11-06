using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterButton)), CanEditMultipleObjects]
    public class BetterButtonEditor : ButtonEditor
    {
        BetterElementHelper<Button, BetterButton> helper =
            new BetterElementHelper<Button, BetterButton>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BetterButton btn = target as BetterButton;
            helper.DrawGui(serializedObject, btn);

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/Button/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            Button btn = command.context as Button;
            Betterizer.MakeBetter<Button, BetterButton>(btn);
        }
    }
}
