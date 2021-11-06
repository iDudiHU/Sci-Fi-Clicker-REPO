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
    [CustomEditor(typeof(BetterInputField)), CanEditMultipleObjects]
    public class BetterInputFieldEditor : InputFieldEditor
    {
        BetterElementHelper<InputField, BetterInputField> helper =
            new BetterElementHelper<InputField, BetterInputField>();

        int phCount = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BetterInputField obj = target as BetterInputField;
            helper.DrawGui(serializedObject, obj);

            var prop = serializedObject.FindProperty("additionalPlaceholders");
            EditorGuiUtils.DrawLayoutList("Additional Placeholders",
                obj.AdditionalPlaceholders, prop, ref phCount,
                createCallback: null,
                drawItemCallback: (item, p) =>
                {
                    EditorGUILayout.PropertyField(p);
                    serializedObject.ApplyModifiedProperties();
                }
            );

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/InputField/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            InputField obj = command.context as InputField;
            Betterizer.MakeBetter<InputField, BetterInputField>(obj);
        }
    }
}
