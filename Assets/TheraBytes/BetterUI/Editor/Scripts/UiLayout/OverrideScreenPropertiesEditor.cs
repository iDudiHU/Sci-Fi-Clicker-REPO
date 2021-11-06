using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(OverrideScreenProperties)), CanEditMultipleObjects]
    public class OverrideScreenPropertiesEditor : UnityEditor.Editor
    {
        SerializedProperty settingsFallback, settingsList;

        void OnEnable()
        {
            OverrideScreenProperties osp = target as OverrideScreenProperties;

            this.settingsFallback = serializedObject.FindProperty("settingsFallback");
            this.settingsList = serializedObject.FindProperty("customSettings");

            if (osp.FallbackSettings.PropertyIterator().All(o => (int)o.Mode == 0 && o.Value == 0))
            {
                InitElement(osp.FallbackSettings.ScreenConfigName, settingsFallback);
            }
        }
        public override void OnInspectorGUI()
        {
            ScreenConfigConnectionHelper.DrawGui("Settings", settingsList, ref settingsFallback, DrawSettings, InitElement);
        }

        private void InitElement(string configName, SerializedProperty settings)
        {
            SerializedProperty width = settings.FindPropertyRelative("OptimizedWidthOverride");
            SerializedProperty height = settings.FindPropertyRelative("OptimizedHeightOverride");
            SerializedProperty dpi = settings.FindPropertyRelative("OptimizedDpiOverride");

            var info = ResolutionMonitor.GetOpimizedScreenInfo(configName);

            SetValue(width, OverrideScreenProperties.OverrideMode.Override, info.Resolution.x);
            SetValue(height, OverrideScreenProperties.OverrideMode.Override, info.Resolution.y);
            SetValue(dpi, OverrideScreenProperties.OverrideMode.Inherit, info.Dpi);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSettings(string configName, SerializedProperty settings)
        {
            SerializedProperty width = settings.FindPropertyRelative("OptimizedWidthOverride");
            SerializedProperty height = settings.FindPropertyRelative("OptimizedHeightOverride");
            SerializedProperty dpi = settings.FindPropertyRelative("OptimizedDpiOverride");

            EditorGUILayout.BeginVertical("box");

            DrawProperty("Width", width);
            DrawProperty("Height", height);
            DrawProperty("DPI", dpi);

            EditorGUILayout.EndVertical();
        }

        private void DrawProperty(string label, SerializedProperty property)
        {
            SerializedProperty value = property.FindPropertyRelative("value");
            SerializedProperty mode = property.FindPropertyRelative("mode");

            EditorGUILayout.BeginHorizontal();

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUILayout.PropertyField(mode, new GUIContent(label));

            if(mode.intValue == (int)OverrideScreenProperties.OverrideMode.Override)
            {
                EditorGUIUtility.labelWidth = 1;

                EditorGUILayout.PropertyField(value, new GUIContent());
                EditorGUIUtility.labelWidth = labelWidth;
            }

            EditorGUILayout.EndHorizontal();
        }

        public void SetValue(SerializedProperty property, OverrideScreenProperties.OverrideMode mode, float value)
        {
            SerializedProperty modeProp = property.FindPropertyRelative("mode");
            SerializedProperty valueProp = property.FindPropertyRelative("value");

            modeProp.intValue = (int)mode;
            valueProp.floatValue = value;
        }

    }
}
