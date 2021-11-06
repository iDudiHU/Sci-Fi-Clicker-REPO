using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterLayoutElement)), CanEditMultipleObjects]
    public class BetterLayoutElementEditor : UnityEditor.Editor
    {
        SerializedProperty settingsFallback, settingsList;

        SerializedProperty 
            minWidthSizerFallback,
            minHeightSizerFallback,
            preferredWidthSizerFallback,
            preferredHeightSizerFallback;

        SerializedProperty
            customMinWidthSizers,
            customMinHeightSizers,
            customPreferredWidthSizers,
            customPreferredHeightSizers;

        SerializedProperty layoutPriority;

        // key: fallback, value: customSizers
        Dictionary<SerializedProperty, SerializedProperty> allSizers = new Dictionary<SerializedProperty, SerializedProperty>();

        void OnEnable()   
        {                 
            this.settingsFallback = serializedObject.FindProperty("settingsFallback");
            this.settingsList = serializedObject.FindProperty("customSettings");


            customMinWidthSizers = serializedObject.FindProperty("customMinWidthSizers");
            customMinHeightSizers = serializedObject.FindProperty("customMinHeightSizers");
            customPreferredWidthSizers = serializedObject.FindProperty("customPreferredWidthSizers");
            customPreferredHeightSizers = serializedObject.FindProperty("customPreferredHeightSizers");
            layoutPriority = serializedObject.FindProperty("m_LayoutPriority");

            RestoreSizerFallbackReferences();
        }

        void RestoreSizerFallbackReferences()
        {
            minWidthSizerFallback = serializedObject.FindProperty("minWidthSizerFallback");
            minHeightSizerFallback = serializedObject.FindProperty("minHeightSizerFallback");
            preferredWidthSizerFallback = serializedObject.FindProperty("preferredWidthSizerFallback");
            preferredHeightSizerFallback = serializedObject.FindProperty("preferredHeightSizerFallback");

            allSizers.Clear();
            allSizers.Add(minWidthSizerFallback, customMinWidthSizers);
            allSizers.Add(minHeightSizerFallback, customMinHeightSizers);
            allSizers.Add(preferredWidthSizerFallback, customPreferredWidthSizers);
            allSizers.Add(preferredHeightSizerFallback, customPreferredHeightSizers);
        }

        public override void OnInspectorGUI()
        {
            ScreenConfigConnectionHelper.DrawGui("Settings", settingsList, ref settingsFallback, DrawSettings, AddSettings, DeleteSettings);

            if(layoutPriority != null) // Unity 2017+
            {
                EditorGUILayout.PropertyField(layoutPriority);
            }
        }

        private void DeleteSettings(string configName, SerializedProperty property)
        {
            foreach (var kv in allSizers)
            {
                int idx;
                SerializedProperty sizersProp = FindSizer(configName, null, kv.Value, out idx);
                if (sizersProp != null)
                {
                    SerializedProperty items = kv.Value.FindPropertyRelative("items");
                    items.DeleteArrayElementAtIndex(idx);
                }
            }
        }

        private void AddSettings(string configName, SerializedProperty property)
        {
            foreach(var kv in allSizers)
            {
                SerializedProperty sizersProp = FindSizer(configName, null, kv.Value);
                if(sizersProp == null)
                {
                    SerializedProperty items = kv.Value.FindPropertyRelative("items");
                    SerializedProperty fallback = kv.Key;
                    ScreenConfigConnectionHelper.AddSizerToList(configName, ref fallback, items);
                }
            }

            // after adding the fallback values are pointing somewhere because of copying of all properties
            RestoreSizerFallbackReferences();
        }

        private void DrawSettings(string configName, SerializedProperty settings)
        {
            SerializedProperty ignoreLayout = settings.FindPropertyRelative("IgnoreLayout");

            SerializedProperty minWidthEnabled = settings.FindPropertyRelative("MinWidthEnabled");
            SerializedProperty minHeightEnabled = settings.FindPropertyRelative("MinHeightEnabled");
            SerializedProperty preferredWidthEnabled = settings.FindPropertyRelative("PreferredWidthEnabled");
            SerializedProperty preferredHeightEnabled = settings.FindPropertyRelative("PreferredHeightEnabled");
            SerializedProperty flexibleWidthEnabled = settings.FindPropertyRelative("FlexibleWidthEnabled");
            SerializedProperty flexibleHeightEnabled = settings.FindPropertyRelative("FlexibleHeightEnabled");

            SerializedProperty flexibleWidth = settings.FindPropertyRelative("FlexibleWidth");
            SerializedProperty flexibleHeight = settings.FindPropertyRelative("FlexibleHeight");
            

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(ignoreLayout);

            if(!(ignoreLayout.boolValue))
            {
                EditorGUILayout.Space();
                
                EditorGUILayout.PropertyField(minWidthEnabled, new GUIContent("Min Width"));
                if(minWidthEnabled.boolValue)
                {
                    DrawSizer(configName, minWidthSizerFallback, customMinWidthSizers);
                }

                EditorGUILayout.PropertyField(minHeightEnabled, new GUIContent("Min Height"));
                if (minHeightEnabled.boolValue)
                {
                    DrawSizer(configName, minHeightSizerFallback, customMinHeightSizers);
                }

                EditorGUILayout.PropertyField(preferredWidthEnabled, new GUIContent("Preferred Width"));
                if (preferredWidthEnabled.boolValue)
                {
                    DrawSizer(configName, preferredWidthSizerFallback, customPreferredWidthSizers);
                }

                EditorGUILayout.PropertyField(preferredHeightEnabled, new GUIContent("Preferred Height"));
                if (preferredHeightEnabled.boolValue)
                {
                    DrawSizer(configName, preferredHeightSizerFallback, customPreferredHeightSizers);
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(flexibleWidthEnabled, new GUIContent("Flexible Width"));
                if(flexibleWidthEnabled.boolValue)
                {
                    EditorGUILayout.PropertyField(flexibleWidth, GUIContent.none);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(flexibleHeightEnabled, new GUIContent("Flexible Height"));
                if (flexibleHeightEnabled.boolValue)
                {
                    EditorGUILayout.PropertyField(flexibleHeight, GUIContent.none);
                }

                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSizer(string configName, SerializedProperty fallback, SerializedProperty customSizers)
        {
            EditorGUI.indentLevel++;

            SerializedProperty prop = FindSizer(configName, fallback, customSizers);
            if(prop != null)
            {
                EditorGUILayout.PropertyField(prop);
            }
            else
            {
                EditorGUILayout.HelpBox(string.Format("could not find sizer for config '{0}'", configName), MessageType.Error);
            }

            EditorGUI.indentLevel--;
        }

        SerializedProperty FindSizer(string configName, SerializedProperty fallback, SerializedProperty customSizers)
        {
            int idx;
            return FindSizer(configName, fallback, customSizers, out idx);
        }

        SerializedProperty FindSizer(string configName, SerializedProperty fallback, SerializedProperty customSizers, out int sizerIndex)
        {
            bool isFallback = !(ResolutionMonitor.Instance.OptimizedScreens.Any(o => o.Name == configName));
            sizerIndex = -1;

            if (isFallback)
            {
                return fallback;
            }
            else
            {
                SerializedProperty items = customSizers.FindPropertyRelative("items");
                for (int i = 0; i < items.arraySize; i++)
                {
                    SerializedProperty prop = items.GetArrayElementAtIndex(i);
                    SerializedProperty propConfig = prop.FindPropertyRelative("screenConfigName");
                    if (propConfig.stringValue == configName)
                    {
                        sizerIndex = i;
                        return prop;
                    }
                }
            }

            return null;
        }

        [MenuItem("CONTEXT/LayoutElement/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            LayoutElement layout = command.context as LayoutElement;
            var ignore = layout.ignoreLayout;
            var minWidth = layout.minWidth;
            var minHeight = layout.minHeight;
            var prefWidth = layout.preferredWidth;
            var prefHeight = layout.preferredHeight;
            var flexWidth = layout.flexibleWidth;
            var flexHeight = layout.flexibleHeight;

            var newLayout = Betterizer.MakeBetter<LayoutElement, BetterLayoutElement>(layout) as BetterLayoutElement;
            if(newLayout != null)
            {
                newLayout.CurrentSettings.IgnoreLayout = ignore;

                newLayout.CurrentSettings.MinWidthEnabled = (minWidth >= 0);
                newLayout.CurrentSettings.MinHeightEnabled = (minHeight >= 0);

                newLayout.CurrentSettings.PreferredWidthEnabled = (prefWidth >= 0);
                newLayout.CurrentSettings.PreferredHeightEnabled = (prefHeight >= 0);

                newLayout.CurrentSettings.FlexibleWidthEnabled = (flexWidth >= 0);
                newLayout.CurrentSettings.FlexibleHeightEnabled = (flexHeight >= 0);


                newLayout.MinHeightSizer.SetSize(newLayout, minHeight);
                newLayout.MinWidthSizer.SetSize(newLayout, minWidth);

                newLayout.PreferredWidthSizer.SetSize(newLayout, prefWidth);
                newLayout.PreferredHeightSizer.SetSize(newLayout, prefHeight);

                newLayout.CurrentSettings.FlexibleWidth = flexWidth;
                newLayout.CurrentSettings.FlexibleHeight = flexHeight;
            }
        }
    }
}
