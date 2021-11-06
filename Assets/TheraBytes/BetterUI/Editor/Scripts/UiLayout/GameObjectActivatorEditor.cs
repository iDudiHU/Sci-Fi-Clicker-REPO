using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(GameObjectActivator)), CanEditMultipleObjects]
    public class GameObjectActivatorrEditor : UnityEditor.Editor
    {
        GameObjectActivator activator;
        SerializedProperty settingsFallback, settingsList;

        void OnEnable()
        {
            activator = base.target as GameObjectActivator;
            this.settingsFallback = serializedObject.FindProperty("settingsFallback");
            this.settingsList = serializedObject.FindProperty("customSettings");
        }
        
        public override void OnInspectorGUI()
        {
            bool tmp = activator.EditorPreview;
            activator.EditorPreview = EditorGUILayout.ToggleLeft("Editor Preview", activator.EditorPreview);
            if(activator.EditorPreview && tmp != activator.EditorPreview)
            {
                activator.Apply();
            }

            ScreenConfigConnectionHelper.DrawGui("Settings", settingsList, ref settingsFallback, DrawSettings);
        }

        private void DrawSettings(string configName, SerializedProperty settings)
        {
            SerializedProperty active = settings.FindPropertyRelative("ActiveObjects");
            SerializedProperty inactive = settings.FindPropertyRelative("InactiveObjects");
            
            EditorGUILayout.BeginVertical("box");

            DrawList("Active Objects", active, inactive);
            DrawList("Inactive Objects", inactive, active);

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawList(string label, SerializedProperty list, SerializedProperty otherList)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            int i;
            for(i = 0; i < list.arraySize; i++)
            {
                SerializedProperty prop = list.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                //EditorGUILayout.ObjectField(prop.objectReferenceValue, typeof(GameObject), true);
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("x", GUILayout.Width(18)))
                {
                    list.DeleteArrayElementAtIndex(i); // nulls the object
                    list.DeleteArrayElementAtIndex(i); // deletes the entry
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            GameObject obj = EditorGUILayout.ObjectField("Add Object", null, typeof(GameObject), true) as GameObject;
            if(obj != null)
            {
                if(CheckObject(obj, list, otherList))
                {
                    list.InsertArrayElementAtIndex(i);
                    SerializedProperty prop = list.GetArrayElementAtIndex(i);
                    prop.objectReferenceValue = obj;
                }

            }

            EditorGUILayout.Space();

            EditorGUI.indentLevel--;
        }

        bool CheckObject(GameObject go, SerializedProperty list, SerializedProperty otherList)
        {
#if UNITY_2018_2_OR_NEWER
            if(PrefabUtility.GetCorrespondingObjectFromSource(go) == null && PrefabUtility.GetCorrespondingObjectFromSource(go) != null)
#else 
            if(PrefabUtility.GetPrefabParent(go) == null && PrefabUtility.GetPrefabObject(go) != null)
#endif
            {
                Debug.LogError("Object must be part of the scene.");
                return false;
            }

            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == go)
                {
                    Debug.Log("Object already added.");
                    return false;
                }
            }

            for (int i = 0; i < otherList.arraySize; i++)
            {
                if (otherList.GetArrayElementAtIndex(i).objectReferenceValue == go)
                {
                    Debug.LogError("Object already added to the other list.");
                    return false;
                }
            }

            Transform parentOrSelf = activator.transform;
            while(parentOrSelf != null)
            {
                if(parentOrSelf.gameObject == go)
                {
                    Debug.LogError("Object to add must not be this object or a parent of this object.");
                    return false;
                }

                parentOrSelf = parentOrSelf.parent;
            }

            return true;
        }

    }
}
