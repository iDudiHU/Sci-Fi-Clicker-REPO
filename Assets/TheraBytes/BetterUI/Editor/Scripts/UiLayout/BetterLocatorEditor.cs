using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterLocator)), CanEditMultipleObjects]
    public class BetterLocatorEditor : UnityEditor.Editor
    {

        SerializedProperty transformFallback, transformConfigs;
        BetterLocator locator;

        Dictionary<RectTransformData, bool> anchorExpands = new Dictionary<RectTransformData, bool>();

        bool autoPullFromTransform = true;
        bool autoPushToTransform = false;

        bool pauseAutoPullOnce = false; // too early pull protection
        bool pauseAutoPushOnce = false; // recursive infinite loop protection

        protected virtual void OnEnable()
        {
            this.locator = target as BetterLocator;
            transformFallback = serializedObject.FindProperty("transformFallback");
            transformConfigs = serializedObject.FindProperty("transformConfigs");

            this.locator.OnValidate();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PrefixLabel("Live Update");
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!locator.isActiveAndEnabled);
            autoPullFromTransform = GUILayout.Toggle(autoPullFromTransform, "↓   Auto-Pull", "ButtonLeft", GUILayout.MinHeight(30));
            autoPushToTransform = GUILayout.Toggle(autoPushToTransform, "↑   Auto-Push", "ButtonRight", GUILayout.MinHeight(30));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (autoPullFromTransform && !(pauseAutoPullOnce))
            {
                locator.CurrentTransformData.PullFromTransform(locator.transform as RectTransform);
            }

            ScreenConfigConnectionHelper.DrawGui("Rect Transform Override", transformConfigs, ref transformFallback, DrawTransformData);

            if (autoPushToTransform && !(pauseAutoPushOnce))
            {
                locator.CurrentTransformData.PushToTransform(locator.transform as RectTransform);
            }

            pauseAutoPushOnce = false;
            pauseAutoPullOnce = !locator.isActiveAndEnabled;
        }

        void DrawTransformData(string configName, SerializedProperty prop)
        {
            RectTransformData data = prop.GetValue<RectTransformData>();
            bool isCurrent = locator.CurrentTransformData == data;
            
            if (!(anchorExpands.ContainsKey(data)))
            {
                anchorExpands.Add(data, true);
            }

            bool anchorExpand = anchorExpands[data];
            float height = (anchorExpand) 
                ? RectTransformDataDrawer.HeightWithAnchorsExpanded
                : RectTransformDataDrawer.HeightWithoutAnchorsExpanded;

            EditorGUILayout.BeginVertical("box");
            Rect bounds = EditorGUILayout.GetControlRect(false, height);
            

            bool canEdit = !(isCurrent) || !(autoPullFromTransform) || autoPushToTransform;

            // Pull
            EditorGUI.BeginDisabledGroup(isCurrent && autoPullFromTransform);
            if (GUI.Button(new Rect(bounds.position + new Vector2(5, 5), new Vector2(40, 40)), "Pull\n↓"))
            {
                Undo.RecordObject(locator, "Pull From Rect Transform");
                data.PullFromTransform(locator.transform as RectTransform);
            }
            EditorGUI.EndDisabledGroup();

            // Push
            EditorGUI.BeginDisabledGroup(!(canEdit) || (isCurrent && autoPushToTransform));
            if (GUI.Button(new Rect(bounds.position + new Vector2(50, 25), new Vector2(40, 40)), "↑\nPush"))
            {
                Undo.RecordObject(locator.transform, "Push To Rect Transform");

                data.PushToTransform(locator.transform as RectTransform);
                pauseAutoPushOnce = true;
            }
            EditorGUI.EndDisabledGroup();

            // Fields
            if(!canEdit)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            RectTransformDataDrawer.DrawFields(prop, data, bounds, ref anchorExpand);
            anchorExpands[data] = anchorExpand;

            if (!canEdit)
            {
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndVertical();
        }
        
        [MenuItem("CONTEXT/RectTransform/♠ Add Better Locator", false)]
        public static void AddBetterLocator(MenuCommand command)
        {
            var ctx = command.context as RectTransform;
            var locator = ctx.gameObject.AddComponent<BetterLocator>();

            while(UnityEditorInternal.ComponentUtility.MoveComponentUp(locator))
            { }
        }

        [MenuItem("CONTEXT/RectTransform/♠ Add Better Locator", true)]
        public static bool CheckBetterLocator(MenuCommand command)
        {
            var ctx = command.context as RectTransform;
            return ctx.gameObject.GetComponent<BetterLocator>() == null;
        }
    }
    
}
