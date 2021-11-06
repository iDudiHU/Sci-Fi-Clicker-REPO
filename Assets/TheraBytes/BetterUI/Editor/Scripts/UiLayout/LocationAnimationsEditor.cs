using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(LocationAnimations))]
    public class LocationAnimationsEditor : UnityEditor.Editor
    {
        const string ANY = "[ Any Location ]";
        const string NONE = "[ None ]";

        static readonly GUIContent pushContent = new GUIContent("↑", "Push current location data to Rect Transform.");
        static readonly GUIContent pullContent = new GUIContent("↓", "Pull Rect Transform to current location data.");

        SerializedProperty useRelativeProp, locationListProp, transitionListProp, startLocProp, startupAniProp, initCallbackProp;

        bool locationListExpanded = true;
        bool transitionListExpanded = true;
        bool initialStateExpanded = true;
        HashSet<string> expandedAnimations = new HashSet<string>();
        HashSet<string> expandedLocations = new HashSet<string>();
        LocationAnimations locAni;

        float lastTime;

        protected virtual void OnEnable()
        {
            locAni = target as LocationAnimations;
            useRelativeProp = serializedObject.FindProperty("useRelativeLocations");
            locationListProp = serializedObject.FindProperty("locations");
            transitionListProp = serializedObject.FindProperty("animations");

            startLocProp = serializedObject.FindProperty("startLocation");
            startupAniProp = serializedObject.FindProperty("startUpAnimation");
            initCallbackProp = serializedObject.FindProperty("actionOnInit");

            lastTime = (float)EditorApplication.timeSinceStartup;
            EditorApplication.update += UpdateAnimation;
        }

        protected virtual void OnDisable()
        {
            EditorApplication.update -= UpdateAnimation;
        }

        private void UpdateAnimation()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPaused)
                return;

            float now = (float)EditorApplication.timeSinceStartup;
            float delta = now - lastTime;
            lastTime = now;

            locAni.UpdateCurrentAnimation(delta);
        }


        public override void OnInspectorGUI()
        {

            // LOCATIONS

            if (BoldFoldout("Locations", ref locationListExpanded))
            {
                for (int i = 0; i < locationListProp.arraySize; i++)
                {
                    SerializedProperty prop = locationListProp.GetArrayElementAtIndex(i);

                    SerializedProperty nameProp = prop.FindPropertyRelative("name");
                    SerializedProperty fallbackProp = prop.FindPropertyRelative("transformFallback");
                    SerializedProperty configsProp = prop.FindPropertyRelative("transformConfigs");

                    EditorGUILayout.BeginVertical("box");

                    string name = nameProp.stringValue;

                    bool expanded = expandedLocations.Contains(name);
                    if (DrawNameAndDelete(nameProp, locationListProp, LocationNameChanged, ref i, () =>
                    {
                        if (GUILayout.Button(EditorGUIUtility.IconContent(expanded ? "d_ViewToolOrbit On" : "d_ViewToolOrbit"),
                            EditorStyles.label, GUILayout.Width(25), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                        {
                            expanded = !expanded;

                            if (expanded)
                                expandedLocations.Add(name);
                            else
                                expandedLocations.Remove(name);
                        }

                        if (GUILayout.Button(pullContent, GUILayout.Width(25)))
                        {
                            Undo.RecordObject(locAni, "pull from transform");
                            var loc = locAni.GetLocation(name);
                            PullFromTransform(loc);
                        }

                        if (GUILayout.Button(pushContent, GUILayout.Width(25)))
                        {
                            Undo.RecordObject(locAni.RectTransform, "push to transform");
                            var loc = locAni.GetLocation(name);
                            PushToTransform(loc);
                        }
                    }))
                    {
                        continue;
                    }

                    if (expanded)
                    {
                        if (locAni.UseRelativeLocations)
                        {
                            RectTransformDataDrawer.OverwritePushPullMethods(
                                push: (rt, data) => RectTransformData.Combine(data, locAni.ReferenceLocation).PushToTransform(rt),
                                pull: (rt, data) => data.PullFromData(RectTransformData.Separate(new RectTransformData(rt), locAni.ReferenceLocation)));
                        }

                        ScreenConfigConnectionHelper.DrawGui(nameProp.stringValue, configsProp, ref fallbackProp,
                            drawContent: null,
                            newElementInitCallback: (n, obj) =>
                            {
                                var euler = obj.FindPropertyRelative("saveRotationAsEuler");
                                euler.boolValue = true;
                            });

                        RectTransformDataDrawer.OverwritePushPullMethods(null, null);
                    }

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginHorizontal();

                // Use Relative Locations
                bool preUseRelative = useRelativeProp.boolValue;
                EditorGUILayout.PropertyField(useRelativeProp);
                bool postUseRelative = useRelativeProp.boolValue;

                if (preUseRelative != postUseRelative)
                {
                    ConvertLocationSpace(postUseRelative);
                }

                // "Add Location" button
                var newObj = DrawAddButton("Add Location", "Location", locationListProp);
                if (newObj != null)
                {
                    var rtd = newObj.FindPropertyRelative("transformFallback");
                    var euler = rtd.FindPropertyRelative("saveRotationAsEuler");
                    euler.boolValue = true;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                EditorGUILayout.EndHorizontal();
            }


            // ANIMATIONS

            List<string> options = locAni.Locations.Select(o => o.Name).ToList();
            string[] toOptions = options.ToArray();
            options.Insert(0, ANY);
            string[] fromOptions = options.ToArray();

            if (BoldFoldout("Animations", ref transitionListExpanded))
            {
                for (int i = 0; i < transitionListProp.arraySize; i++)
                {
                    SerializedProperty prop = transitionListProp.GetArrayElementAtIndex(i);

                    SerializedProperty nameProp = prop.FindPropertyRelative("name");
                    SerializedProperty fromProp = prop.FindPropertyRelative("from");
                    SerializedProperty toProp = prop.FindPropertyRelative("to");

                    EditorGUILayout.BeginVertical("box");

                    if (DrawNameAndDelete(nameProp, transitionListProp, TransitionNameChanged, ref i,
                        () =>
                        {
                            bool isAnimating = locAni.RunningAnimation != null && locAni.RunningAnimation.Animation.Name == nameProp.stringValue;
                            if (GUILayout.Button((isAnimating) ? "■" : "►", GUILayout.Width(20)))
                            {
                                if (isAnimating)
                                {
                                    locAni.StopCurrentAnimation();
                                }
                                else
                                {
                                    locAni.StartAnimation(nameProp.stringValue);
                                }
                            }
                        }))
                    {
                        continue;
                    }

                    EditorGUILayout.Space();

                    int fromSelection = Array.IndexOf<string>(fromOptions, fromProp.stringValue);
                    if (fromSelection < 0) // any
                        fromSelection = 0;

                    int newFromSelection = EditorGUILayout.Popup("From", fromSelection, fromOptions);
                    if (fromSelection != newFromSelection)
                    {
                        string val = fromOptions[newFromSelection];
                        fromProp.stringValue = (val != ANY) ? val : "";
                    }

                    int toSelection = Array.IndexOf<string>(toOptions, toProp.stringValue);
                    int newToSelection = EditorGUILayout.Popup("To", toSelection, toOptions);
                    if (toSelection != newToSelection)
                    {
                        toProp.stringValue = toOptions[newToSelection];
                    }


                    EditorGUI.indentLevel++;
                    bool actionsFoldout = EditorGUILayout.Foldout(expandedAnimations.Contains(nameProp.stringValue), "Advanced");
                    if (actionsFoldout)
                    {
                        SerializedProperty curveProp = prop.FindPropertyRelative("curve");
                        SerializedProperty timeScaleProp = prop.FindPropertyRelative("timeScale");
                        SerializedProperty eulerProp = prop.FindPropertyRelative("animateWithEulerRotation");
                        SerializedProperty actionBeforeProp = prop.FindPropertyRelative("actionBeforeStart");
                        SerializedProperty actionUpdateProp = prop.FindPropertyRelative("actionOnUpdating");
                        SerializedProperty actionAfterProp = prop.FindPropertyRelative("actionAfterFinish");

                        curveProp.animationCurveValue = EditorGUILayout.CurveField("Curve", curveProp.animationCurveValue);

                        float speedBefore = timeScaleProp.floatValue;
                        EditorGUILayout.PropertyField(timeScaleProp);
                        float speedAfter = timeScaleProp.floatValue;

                        if (speedBefore != speedAfter && locAni.RunningAnimation != null)
                        {
                            locAni.RunningAnimation.TimeScale = speedAfter;
                        }

                        string[] rotationOptions = { "Quaternion", "Euler" };
                        int prevIdx = (eulerProp.boolValue) ? 1 : 0;
                        int newIdx = EditorGUILayout.Popup("Rotation Mode", prevIdx, rotationOptions);
                        if (prevIdx != newIdx)
                        {
                            eulerProp.boolValue = (newIdx == 1);
                        }

                        EditorGUILayout.PropertyField(actionBeforeProp);
                        EditorGUILayout.PropertyField(actionUpdateProp);
                        EditorGUILayout.PropertyField(actionAfterProp);
                        expandedAnimations.Add(nameProp.stringValue);
                    }
                    else
                    {
                        expandedAnimations.Remove(nameProp.stringValue);
                    }
                    EditorGUI.indentLevel--;

                    serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.EndVertical();
                }

                var newAni = DrawAddButton("Add Animation", "Animation", transitionListProp);
                if (newAni != null)
                {
                    var timeScaleProp = newAni.FindPropertyRelative("timeScale");
                    timeScaleProp.floatValue = 1;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

            }


            // INITIAL STATE
            if (BoldFoldout("Initial State", ref initialStateExpanded))
            {
                // Start Location
                int startLocSelection = Array.IndexOf<string>(fromOptions, startLocProp.stringValue);
                if (startLocSelection < 0) // any
                    startLocSelection = 0;

                int newFromSelection = EditorGUILayout.Popup("Start Location", startLocSelection, fromOptions);
                if (startLocSelection != newFromSelection)
                {
                    string val = fromOptions[newFromSelection];
                    startLocProp.stringValue = (val != ANY) ? val : "";
                }


                // Start Animation
                List<string> opt = locAni.Animations.Select(o => o.Name).ToList();
                opt.Insert(0, NONE);
                string[] startupOptions = opt.ToArray();


                int sel = Array.IndexOf<string>(startupOptions, startupAniProp.stringValue);
                if (sel < 0) // none
                    sel = 0;

                int newSel = EditorGUILayout.Popup("Startup Animation", sel, startupOptions);
                if (sel != newSel)
                {
                    string val = startupOptions[newSel];
                    startupAniProp.stringValue = (val != NONE) ? val : "";
                    serializedObject.ApplyModifiedProperties();
                }

                // Init Callback
                EditorGUILayout.PropertyField(initCallbackProp);

                serializedObject.ApplyModifiedProperties();
            }

        }

        private void ConvertLocationSpace(bool toRelativeSpace)
        {
            foreach (var loc in locAni.Locations)
            {
                var rtd = (toRelativeSpace)
                    ? RectTransformData.Separate(loc.CurrentTransformData, locAni.ReferenceLocation)
                    : RectTransformData.Combine(loc.CurrentTransformData, locAni.ReferenceLocation);

                loc.CurrentTransformData.PullFromData(rtd);
            }

            locAni.UseRelativeLocations = toRelativeSpace;
            serializedObject.Update();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void PushToTransform(LocationAnimations.LocationData loc)
        {
            if (locAni.UseRelativeLocations)
            {
                var rtd = RectTransformData.Combine(loc.CurrentTransformData, locAni.ReferenceLocation);
                rtd.PushToTransform(locAni.RectTransform);
            }
            else
            {
                loc.CurrentTransformData.PushToTransform(locAni.RectTransform);
            }
        }

        private void PullFromTransform(LocationAnimations.LocationData loc)
        {
            if (locAni.UseRelativeLocations)
            {
                var rtd = RectTransformData.Separate(new RectTransformData(locAni.RectTransform), locAni.ReferenceLocation);
                loc.CurrentTransformData.PullFromData(rtd);
            }
            else
            {
                loc.CurrentTransformData.PullFromTransform(locAni.RectTransform);
            }
        }

        private void LocationNameChanged(int index, string before, string after)
        {
            if (locAni.Locations.Any(o => o.Name == before))
            {
                // the name is still valid.
                return;
            }

            foreach (var tr in locAni.Animations)
            {
                if (tr.From == before)
                {
                    tr.From = after;
                }

                if (tr.To == before)
                {
                    tr.To = after;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void TransitionNameChanged(int index, string before, string after)
        {
            if (locAni.Animations.Any(o => o.Name == before))
            {
                // the name is still valid.
                return;
            }

            if (locAni.StartUpAnimation == before)
            {
                locAni.StartUpAnimation = after;
            }

            serializedObject.ApplyModifiedProperties();
        }

        bool BoldFoldout(string text, ref bool expanded)
        {
            string prefix = (expanded) ? "▼" : "►";
            if (GUILayout.Button(string.Format("{0} {1}", prefix, text), EditorStyles.boldLabel))
            {
                expanded = !expanded;
            }

            return expanded;
        }

        SerializedProperty DrawAddButton(string text, string namePrefix, SerializedProperty prop)
        {
            SerializedProperty result = null;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(text, GUILayout.Width(150)))
            {
                prop.InsertArrayElementAtIndex(prop.arraySize);

                result = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                SerializedProperty nameProp = result.FindPropertyRelative("name");
                if (nameProp != null)
                {
                    nameProp.stringValue = string.Format("{0} {1}", namePrefix, prop.arraySize);
                }

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorGUILayout.EndHorizontal();

            return result;
        }

        bool DrawNameAndDelete(SerializedProperty nameProp, SerializedProperty listProp, Action<int, string, string> nameChanged, ref int idx, Action drawNextToText = null)
        {
            EditorGUILayout.BeginHorizontal();

            string nameBefore = nameProp.stringValue;

            EditorGUILayout.PropertyField(nameProp, GUIContent.none);

            string nameAfter = nameProp.stringValue;

            if (nameBefore != nameAfter && nameChanged != null)
            {
                int index = idx;
                serializedObject.ApplyModifiedProperties();
                nameChanged(index, nameBefore, nameAfter);
            }

            if (drawNextToText != null)
            {
                drawNextToText();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("x", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("Delete Item", string.Format("Do you really want to delete the item '{0}'?", nameProp.stringValue), "Yes", "No"))
                {
                    listProp.DeleteArrayElementAtIndex(idx);
                    idx--;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    EditorGUILayout.EndHorizontal();
                    return true;
                }
            }

            EditorGUILayout.EndHorizontal();
            return false;
        }
    }
}
