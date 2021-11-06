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
    [CustomEditor(typeof(BetterToggle)), CanEditMultipleObjects]
    public class BetterToggleEditor : ToggleEditor
    {
        BetterElementHelper<Toggle, BetterToggle> helper =
            new BetterElementHelper<Toggle, BetterToggle>();

        int toggleTransCount = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BetterToggle tgl = target as BetterToggle;
            helper.DrawGui(serializedObject, tgl);
            
            // Toggle Transitions
            var tgProp = serializedObject.FindProperty("betterToggleTransitions");
            EditorGuiUtils.DrawTransitions("Better Toggle Transitions (On / Off)", tgl.BetterToggleTransitions, tgProp, 
                ref toggleTransCount, Transitions.OnOffStateNames);

            serializedObject.ApplyModifiedProperties();
        }
        
        [MenuItem("CONTEXT/Toggle/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            Toggle tgl = command.context as Toggle;
            Betterizer.MakeBetter<Toggle, BetterToggle>(tgl);
        }
    }
}
