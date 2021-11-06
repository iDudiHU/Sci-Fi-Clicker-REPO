using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterTextMeshProInputField)), CanEditMultipleObjects]
    public class BetterTextMeshProInputFieldEditor : TMP_InputFieldEditor
    {

        BetterElementHelper<TMP_InputField, BetterTextMeshProInputField> helper =
            new BetterElementHelper<TMP_InputField, BetterTextMeshProInputField>();

        int phCount = 0;

        bool foldout = true;

        SerializedProperty pointSizeScalerProp;
        SerializedProperty overrideSizeProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            pointSizeScalerProp = serializedObject.FindProperty("pointSizeScaler");
            overrideSizeProp = serializedObject.FindProperty("overridePointSize");
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

                var obj = target as BetterTextMeshProInputField;

                EditorGUILayout.PropertyField(overrideSizeProp);
                if(overrideSizeProp.boolValue)
                {
                    EditorGUILayout.PropertyField(pointSizeScalerProp);
                }

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

                EditorGUI.indentLevel--;
            }


            base.OnInspectorGUI();
        }

        [MenuItem("CONTEXT/TMP_InputField/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            TMP_InputField obj = command.context as TMP_InputField;
            Betterizer.MakeBetter<TMP_InputField, BetterTextMeshProInputField>(obj);
        }
    }
}
