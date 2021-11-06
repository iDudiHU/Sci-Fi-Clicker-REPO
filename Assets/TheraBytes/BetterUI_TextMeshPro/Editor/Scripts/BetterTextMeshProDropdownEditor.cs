using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TMPro.EditorUtilities;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterTextMeshProDropdown)), CanEditMultipleObjects]
    public class BetterTextMeshProDropDownEditor : TMPro.EditorUtilities.DropdownEditor
    {

        bool foldout = true;

        BetterElementHelper<TMP_Dropdown, BetterTextMeshProDropdown> helper =
            new BetterElementHelper<TMP_Dropdown, BetterTextMeshProDropdown>();

        int showHideTransCount = 0;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            var origFontStyle = EditorStyles.foldout.fontStyle;
            EditorStyles.foldout.fontStyle = FontStyle.Bold;

            foldout = EditorGUILayout.Foldout(foldout, new GUIContent("Better UI"));

            EditorStyles.foldout.fontStyle = origFontStyle;

            if (foldout)
            {
                EditorGUI.indentLevel++;

                BetterTextMeshProDropdown obj = target as BetterTextMeshProDropdown;
                helper.DrawGui(serializedObject, obj);

                // Show / Hide Transitions
                var prop = serializedObject.FindProperty("showHideTransitions");
                EditorGuiUtils.DrawTransitions("Show / Hide Transitions", obj.ShowHideTransitions, prop,
                    ref showHideTransCount, Transitions.ShowHideStateNames);

                serializedObject.ApplyModifiedProperties();

                EditorGUI.indentLevel--;
            }

            base.OnInspectorGUI();
        }

        [MenuItem("CONTEXT/TMP_Dropdown/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            var sel = command.context as TMP_Dropdown;
            Betterizer.MakeBetter<TMP_Dropdown, BetterTextMeshProDropdown>(sel);
        }
    }
}