using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CanEditMultipleObjects]
    public abstract class BetterHorizontalOrVerticalLayoutGroupEditor<TSource, TBetter> : UnityEditor.Editor
        where TSource : HorizontalOrVerticalLayoutGroup
        where TBetter : TSource, IBetterHorizontalOrVerticalLayoutGroup
    {
        private SerializedProperty paddingFallback;
        private SerializedProperty paddingConfigs;
        private SerializedProperty spacingFallback;
        private SerializedProperty spacingConfigs;
        protected SerializedProperty settingsFallback;
        protected SerializedProperty settingsConfigs;

        bool hasReverseOption;
        bool hasChildScale;

        protected static TBetter MakeBetterLogic(MenuCommand command)
        {
            TSource lg = command.context as TSource;
            var pad = new Margin(lg.padding);
            var space = lg.spacing;

            var newLg = Betterizer.MakeBetter<TSource, TBetter>(lg, "m_Padding");

            var betterLg = newLg as TBetter;
            if (betterLg != null)
            {
                betterLg.PaddingSizer.SetSize(newLg, pad);
                betterLg.SpacingSizer.SetSize(newLg, space);
            }
            else if(newLg != null)
            {
                pad.CopyValuesTo(newLg.padding);
            }

            Betterizer.Validate(newLg);

            return newLg as TBetter;
        }

        protected virtual void OnEnable()
        {
            this.paddingFallback = base.serializedObject.FindProperty("paddingSizerFallback");
            this.paddingConfigs = base.serializedObject.FindProperty("customPaddingSizers");
            this.spacingFallback = base.serializedObject.FindProperty("spacingSizerFallback");
            this.spacingConfigs = base.serializedObject.FindProperty("customSpacingSizers");
            this.settingsFallback = base.serializedObject.FindProperty("settingsFallback");
            this.settingsConfigs = base.serializedObject.FindProperty("customSettings");

            this.hasReverseOption = base.serializedObject.FindProperty("m_ReverseArrangement") != null;
            this.hasChildScale = base.serializedObject.FindProperty("m_ChildScaleWidth") != null;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultPaddingAndSpacing();
            DrawSettings("", null);
            
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawObsoleteWarning()
        {
            EditorGUILayout.HelpBox(
@"Component is obsolete!
Better Horizontal- and Better Vertical Layout Groups only exist for backwards compatibility.
Please use 'Better Axis Aligned Layout Group' instead.
To do so, just right click on the component and select '♠ Make Better' as usual.", MessageType.Warning);

        }

        protected void DrawDefaultPaddingAndSpacing()
        {
            EditorGUILayout.LabelField("Padding", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(paddingFallback);
            EditorGUILayout.LabelField("Spacing", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spacingFallback);
        }

        protected void DrawPaddingAndSpacingConfigurations()
        {
            ScreenConfigConnectionHelper.DrawSizerGui("Padding", paddingConfigs, ref paddingFallback);
            ScreenConfigConnectionHelper.DrawSizerGui("Spacing", spacingConfigs, ref spacingFallback);
        }

        protected void DrawSettings(string configName, SerializedProperty parent)
        {
            Func<string, string, SerializedProperty> findProp = (nameOrig, nameSetting) =>
            {
                return (parent == null)
                    ? serializedObject.FindProperty(nameOrig)
                    : parent.FindPropertyRelative(nameSetting);
            };

            var orientation = findProp("orientation", "Orientation");

            var childAlignment = findProp("m_ChildAlignment", "ChildAlignment");


            if (parent != null)
            {
                EditorGUILayout.BeginVertical("box");
            }

            if (orientation != null)
            {
                EditorGUILayout.PropertyField(orientation);
            }

            // Child Alignment
            EditorGUILayout.PropertyField(childAlignment, true);

            // Reverse Arrangement
            if (hasReverseOption)
            {
                var reverseArrangement = findProp("m_ReverseArrangement", "ReverseArrangement");
                EditorGUILayout.PropertyField(reverseArrangement, true);
            }

            // Child Control Size
            var version = UnityEditorInternal.InternalEditorUtility.GetUnityVersion();
            if (version >= new Version(5, 5))
            {
                var childControlSizeWidth = findProp("m_ChildControlWidth", "ChildControlWidth");
                var childControlSizeHeight = findProp("m_ChildControlHeight", "ChildControlHeight");
                DrawWidthHeightProperty("Control Child Size", -1, childControlSizeWidth, childControlSizeHeight);
            }

            // Use Child Scale
            if (hasChildScale)
            {
                var childScaleWidth = findProp("m_ChildScaleWidth", "ChildScaleWidth");
                var childScaleHeight = findProp("m_ChildScaleHeight", "ChildScaleHeight");
                DrawWidthHeightProperty("Use Child Scale", -3, childScaleWidth, childScaleHeight);
            }

            // Child Force Expand
            var childForceExpandWidth = findProp("m_ChildForceExpandWidth", "ChildForceExpandWidth");
            var childForceExpandHeight = findProp("m_ChildForceExpandHeight", "ChildForceExpandHeight");
            DrawWidthHeightProperty("Child Force Expand", -2, childForceExpandWidth, childForceExpandHeight);

            if (parent != null)
            {
                EditorGUILayout.EndVertical();
            }

        }

        private static void DrawWidthHeightProperty(string label, int id, SerializedProperty widthProp, SerializedProperty heightProp)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, id, new GUIContent(label));
            rect.width = Mathf.Max(50f, (rect.width - 4f) / 3f);

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50f;

            ToggleLeft(rect, widthProp, new GUIContent("Width"));
            rect.x = rect.x + (rect.width + 2f);
            ToggleLeft(rect, heightProp, new GUIContent("Height"));

            EditorGUIUtility.labelWidth = labelWidth;
        }

        private static void ToggleLeft(Rect position, SerializedProperty property, GUIContent label)
        {
            bool flag = property.boolValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();

            int num = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            flag = EditorGUI.ToggleLeft(position, label, flag);
            EditorGUI.indentLevel = num;
            if (EditorGUI.EndChangeCheck())
            {
                property.boolValue = (!property.hasMultipleDifferentValues ? !property.boolValue : true);
            }

            EditorGUI.showMixedValue = false;
        }

    }
}
