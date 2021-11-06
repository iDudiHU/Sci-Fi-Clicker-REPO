using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public abstract class ScreenDependentSizeDrawer<T> : ScreenDependentSizeModifierDrawer
    {
        Dictionary<string, ReorderableList> lists = new Dictionary<string, ReorderableList>();
        bool foldout;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var sizer = property.GetValue<ScreenDependentSize<T>>();

            GUILayout.Space(-20); // cheat to draw over allocated rect

            EditorGUILayout.BeginVertical(GUI.skin.box);//"RegionBg");// GUI.skin.box);

            //GUILayout.Space(-20); // cheat again because "RegionBg" adds additional space 
            
            //EditorGUILayout.LabelField(property.displayName, EditorStyles.boldLabel);

            EditorGUILayout.LabelField("CurrentValue", GetValueString(sizer.LastCalculatedSize), EditorStyles.boldLabel);
            ShowField(property, "OptimizedSize", "Optimized Size", ref sizer.OptimizedSize);


            EditorGUI.indentLevel += 1;
            foldout = EditorGUILayout.Foldout(foldout, "Size Modification");

            if (foldout)
            {
                EditorGUILayout.BeginHorizontal();

                OverrideScreenProperties overrideProps = null;

                #region find optimized resolution

                MonoBehaviour obj = (property.serializedObject.targetObject as MonoBehaviour);
                ScreenInfo screenInfo = null;
                if(obj != null)
                {
                    overrideProps = obj.GetComponentInParent<OverrideScreenProperties>();
                    if(overrideProps != null)
                    {
                        var settings = overrideProps.SettingsList.Items.FirstOrDefault(o => o.ScreenConfigName == sizer.ScreenConfigName)
                            ?? overrideProps.FallbackSettings;

                        OverrideScreenProperties parent = (settings.PropertyIterator().Any(o => o.Mode == OverrideScreenProperties.OverrideMode.Inherit))
                           ? overrideProps.GetComponentInParent<OverrideScreenProperties>()
                           : null;

                        float optWidth = overrideProps.CalculateOptimizedValue(settings, OverrideScreenProperties.ScreenProperty.Width, parent);
                        float optHeight = overrideProps.CalculateOptimizedValue(settings, OverrideScreenProperties.ScreenProperty.Height, parent);
                        float optDpi = overrideProps.CalculateOptimizedValue(settings, OverrideScreenProperties.ScreenProperty.Dpi, parent);

                        screenInfo = new ScreenInfo(new Vector2(optWidth, optHeight), optDpi);
                    }
                }

                if(screenInfo == null)
                {
                    screenInfo = ResolutionMonitor.GetOpimizedScreenInfo(sizer.ScreenConfigName);
                }

                #endregion

                string opt = string.Format("{0} x {1} @ {2} DPI", screenInfo.Resolution.x, screenInfo.Resolution.y, screenInfo.Dpi);

                EditorGUILayout.LabelField(opt, EditorStyles.boldLabel);

                if (GUILayout.Button("Change"))
                {
                    Selection.activeObject = (UnityEngine.Object)overrideProps ?? ResolutionMonitor.Instance;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;


                ShowField(property, "MinSize", "Min Size", ref sizer.MinSize);
                ShowField(property, "MaxSize", "Max Size", ref sizer.MaxSize);

                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                EditorGUILayout.Space();


                DrawModifiers(property);

                EditorGUI.indentLevel -= 1;
            }

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        protected void DrawModifierList(SerializedProperty property, string title)
        {
            var listProp = property.FindPropertyRelative("SizeModifiers");
            var list = GetList(listProp, title);

            property.serializedObject.Update();
            list.DoLayoutList();
            property.serializedObject.ApplyModifiedProperties();
        }

        ReorderableList GetList(SerializedProperty property, string title)
        {
            if (!(lists.ContainsKey(title)))
            {
                ReorderableList list = new ReorderableList(property.serializedObject, property, true, true, true, true);
                list.elementHeight = EditorGUIUtility.singleLineHeight + 4;
                list.drawHeaderCallback = (Rect rect) =>
                {
                    int tmp = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    EditorGUI.LabelField(rect, title, EditorStyles.miniLabel);
                    EditorGUI.indentLevel = tmp;
                };

                list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    int tmp = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;

                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    float height = EditorGUIUtility.singleLineHeight;
                    rect.y += 2;

                    EditorGUI.PropertyField(
                      new Rect(rect.x, rect.y, 90, height),
                      element.FindPropertyRelative("Mode"), GUIContent.none);

                    EditorGUI.PropertyField(
                        new Rect(rect.x + 100, rect.y, rect.width - 100, height),
                        element.FindPropertyRelative("Impact"), GUIContent.none);

                    EditorGUI.indentLevel = tmp;
                };

                lists.Add(title, list);
            }

            return lists[title];
        }

        protected virtual void ShowField(SerializedProperty parentProp, string propName, string displayName, ref T value)
        {
            var prop = parentProp.FindPropertyRelative(propName);
            EditorGUILayout.PropertyField(prop, new GUIContent(displayName));
        }

        protected abstract void DrawModifiers(SerializedProperty property);
        protected abstract string GetValueString(T obj);
    }

    public abstract class ScreenDependentSizeModifierDrawer : PropertyDrawer
    {
        [CustomPropertyDrawer(typeof(SizeModifierCollection))]
        public class SizeModifierCollectionDrawer : PropertyDrawer
        {
            ReorderableList list;

            ReorderableList GetList(SerializedProperty property)
            {
                if (list == null)
                {
                    list = new ReorderableList(property.serializedObject, property, true, true, true, true);
                    list.elementHeight = EditorGUIUtility.singleLineHeight + 4;
                    list.drawHeaderCallback = (Rect rect) =>
                    {
                        EditorGUI.LabelField(rect, "Size Modifiers");
                    };
                    list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        var element = list.serializedProperty.GetArrayElementAtIndex(index);

                        float height = EditorGUIUtility.singleLineHeight;
                        rect.y += 2;

                        EditorGUI.PropertyField(
                          new Rect(rect.x, rect.y, 90, height),
                          element.FindPropertyRelative("Mode"), GUIContent.none);

                        EditorGUI.PropertyField(
                            new Rect(rect.x + 100, rect.y, rect.width - 100, height),
                            element.FindPropertyRelative("Impact"), GUIContent.none);
                    };

                }

                return list;
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return 0; // use layout
            }

            public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
            {
                var listProp = property.FindPropertyRelative("SizeModifiers");
                var list = GetList(listProp);

                property.serializedObject.Update();
                list.DoLayoutList();

            }
        }

    }
}
