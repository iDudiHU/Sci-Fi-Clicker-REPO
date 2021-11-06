using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{ 
    [CustomEditor(typeof(ResolutionMonitor))]
    public class ResolutionMonitorEditor : UnityEditor.Editor
    {
        enum Category
        {
            ScreenConfigs,
            StaticMethods,
            DpiOverwrites,
        }

        string[] aspectRatioNames;

        ReorderableList dpiList;
        SerializedProperty curOptRes, curOptDpi, fallbackName, dpiManager, staticSizerMethods;
        Category category = Category.ScreenConfigs;
        Vector2 scroll;

        public static string GetButtonText(ScreenTypeConditions config)
        {
            bool isSimulatedConfig = config == ResolutionMonitor.SimulatedScreenConfig;
            bool isCurrentConfig = config == ResolutionMonitor.CurrentScreenConfiguration;
            return string.Format("{0}{1} {2}", 
                (config.IsActive) ? "♦" : "◊", 
                (isSimulatedConfig) 
                ? " ⃰"
                : (isCurrentConfig) ? "¹" : " ",
                config.Name);
        }

        void OnEnable()
        {
            this.staticSizerMethods = serializedObject.FindProperty("staticSizerMethods");
            this.dpiManager = serializedObject.FindProperty("dpiManager");
            var dpiOverrides = dpiManager.FindPropertyRelative("overrides");

            this.curOptRes = serializedObject.FindProperty("optimizedResolutionFallback");
            this.curOptDpi = serializedObject.FindProperty("optimizedDpiFallback");
            this.fallbackName = serializedObject.FindProperty("fallbackName");
            this.aspectRatioNames = ((AspectRatio[])Enum.GetValues(typeof(AspectRatio)))
                .Select(o => o.ToShortDetailedString()).ToArray();

            dpiList = new ReorderableList(dpiManager.serializedObject, dpiOverrides);
            dpiList.drawHeaderCallback += (r) => EditorGUI.LabelField(r, "Device Model Name => DPI");
            dpiList.drawElementCallback += (r, i, a, f) =>
            {
                float tmp = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 1;
                Rect nR = new Rect(r.x + 2, r.y + 2, 3 * (r.width - 6) / 4, r.height - 2);
                Rect vR = new Rect(r.x + nR.width + 4, r.y + 2, (r.width - 6) / 4, r.height - 2);

                SerializedProperty prop = dpiOverrides.GetArrayElementAtIndex(i);
                SerializedProperty name = prop.FindPropertyRelative("deviceModel");
                SerializedProperty dpi = prop.FindPropertyRelative("dpi");

                EditorGUI.PropertyField(nR, name, new GUIContent(""));
                EditorGUI.PropertyField(vR, dpi, new GUIContent(""));

                EditorGUIUtility.labelWidth = tmp;
                serializedObject.ApplyModifiedProperties();
            };
            dpiList.onAddCallback += (r) =>
            {
                dpiOverrides.arraySize++;
                r.index = dpiOverrides.arraySize - 1;
                serializedObject.ApplyModifiedProperties();
            };
            dpiList.onRemoveCallback += (r) =>
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(r);
                serializedObject.ApplyModifiedProperties();
            };
        }

        public override void OnInspectorGUI()
        {
            ResolutionMonitor monitor = (ResolutionMonitor)target;

            scroll = EditorGUILayout.BeginScrollView(scroll);
            scroll.x = -10;
            
            DrawCategoryButton("Screen Configurations", Category.ScreenConfigs);

            if (category == Category.ScreenConfigs)
            {

                for (int i = 0; i < monitor.OptimizedScreens.Count; i++)
                {
                    var config = monitor.OptimizedScreens[i];


                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.BeginHorizontal();


                    if (GUILayout.Button(GetButtonText(config), EditorStyles.largeLabel, GUILayout.Height(20), GUILayout.Width(150)))
                    {
                        ResolutionMonitor.SimulatedScreenConfig = (ResolutionMonitor.SimulatedScreenConfig == config) ? null : config;
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("¶x"))
                    {
                        SetNameOrDeleteOptimizedScreen popup = new SetNameOrDeleteOptimizedScreen(config, () => PopupWindow.focusedWindow.Close());
                        PopupWindow.Show(new Rect(0, 0, 400, 300), popup);
                    }

                    EditorGUI.BeginDisabledGroup(i == 0);
                    if (GUILayout.Button("▲"))
                    {
                        MoveToIndex(config, i - 1);
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(i == monitor.OptimizedScreens.Count - 1);
                    if (GUILayout.Button("▼"))
                    {
                        MoveToIndex(config, i + 1);
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Optimized Screen Size", EditorStyles.boldLabel);
                    Vector2 prevRes = config.OptimizedScreenInfo.Resolution;
                    float prefDpi = config.OptimizedScreenInfo.Dpi;
                    config.OptimizedScreenInfo.Resolution = EditorGUILayout.Vector2Field("Optimized Resolution", config.OptimizedScreenInfo.Resolution);
                    config.OptimizedScreenInfo.Dpi = EditorGUILayout.FloatField("Optimized DPI", config.OptimizedScreenInfo.Dpi);

                    if (prevRes != config.OptimizedScreenInfo.Resolution || prefDpi != config.OptimizedScreenInfo.Dpi)
                    {
                        ResolutionMonitor.MarkDirty();
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("box");

                    DrawConfigPart("Check Screen Orientation", config.CheckOrientation,
                        (o) =>
                        {
                            o.ExpectedOrientation = (IsCertainScreenOrientation.Orientation)EditorGUILayout.EnumPopup("Orientation", o.ExpectedOrientation);
                        });


                    DrawConfigPart("Check Screen Size", config.CheckScreenSize,
                        (o) =>
                        {
                            EditorGUILayout.BeginHorizontal();
                            o.MeasureType = (IsScreenOfCertainSize.ScreenMeasure)EditorGUILayout.EnumPopup(o.MeasureType, GUILayout.MinWidth(95));
                            o.Units = (IsScreenOfCertainSize.UnitType)EditorGUILayout.EnumPopup(o.Units, GUILayout.MinWidth(100));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(">=", GUILayout.Width(50));
                            o.MinSize = EditorGUILayout.FloatField(o.MinSize);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("<", GUILayout.Width(50));
                            o.MaxSize = EditorGUILayout.FloatField(o.MaxSize);
                            EditorGUILayout.EndHorizontal();

                            if (o.MinSize >= o.MaxSize)
                            {
                                EditorGUILayout.HelpBox("First Value must be smaller than second value.", MessageType.Error);
                            }
                        });

                    DrawConfigPart("Check Aspect Ratio", config.CheckAspectRatio,
                        (o) =>
                        {
                            int selector = -1;

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(">=", GUILayout.Width(50));
                            o.MinAspect = EditorGUILayout.FloatField(o.MinAspect, GUILayout.Width(70));
                            selector = EditorGUILayout.Popup(selector, aspectRatioNames);

                            if (selector != -1)
                            {
                                o.MinAspect = ((AspectRatio)selector).GetRatioValue();
                                selector = -1;
                            }
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("<=", GUILayout.Width(50));
                            o.MaxAspect = EditorGUILayout.FloatField(o.MaxAspect, GUILayout.Width(70));
                            selector = EditorGUILayout.Popup(selector, aspectRatioNames);

                            if (selector != -1)
                            {
                                o.MaxAspect = ((AspectRatio)selector).GetRatioValue();
                                selector = -1;
                            }
                            EditorGUILayout.EndHorizontal();

                            o.Inverse = EditorGUILayout.ToggleLeft(" Inverse (ratio is not in given range)", o.Inverse);

                            if (o.MinAspect >= o.MaxAspect)
                            {
                                EditorGUILayout.HelpBox("First Value must be smaller than second value.", MessageType.Error);
                            }
                        });

                    DrawConfigPart("Check Device", config.CheckDeviceType,
                        (o) =>
                        {
                            o.ExpectedDeviceInfo = (IsScreenOfCertainDeviceInfo.DeviceInfo)EditorGUILayout.EnumPopup("Device Info", o.ExpectedDeviceInfo);
                        });

                    DrawConfigPart("Check Tag", config.CheckScreenTag,
                        (o) =>
                        {
                            o.ScreenTag = EditorGUILayout.TextField("Tag", o.ScreenTag);
                        });



                    // ADD FALLBACK
                    string[] options = ResolutionMonitor.Instance.OptimizedScreens
                                    .Where(o => (config != o) && !(config.Fallbacks.Contains(o.Name)))
                                    .Select(o => o.Name)
                                    .ToArray();

                    if (options.Length > 0 || config.Fallbacks.Count > 0)
                    {

                        Rect bgRect = EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("Fallback Order", EditorStyles.boldLabel);

                        if (options.Length > 0)
                        {

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            Rect r = new Rect(bgRect.x + bgRect.width - 20, bgRect.y + 3, 20, 20);
                            int idx = EditorGUI.Popup(r, -1, options, "OL Plus");

                            if (idx != -1)
                            {
                                string name = options[idx];
                                idx = -1;

                                config.Fallbacks.Insert(0, name);
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.indentLevel++;
                        for (int k = 0; k < config.Fallbacks.Count; k++)
                        {
                            string name = config.Fallbacks[k];
                            EditorGUILayout.BeginHorizontal();

                            EditorGUILayout.LabelField(string.Format("{0}. {1}", k + 1, name));
                            GUILayout.FlexibleSpace();

                            EditorGUI.BeginDisabledGroup(k == 0);
                            if (GUILayout.Button("↑"))
                            {
                                config.Fallbacks.RemoveAt(k);
                                config.Fallbacks.Insert(k - 1, name);
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUI.BeginDisabledGroup(k == config.Fallbacks.Count - 1);
                            if (GUILayout.Button("↓"))
                            {
                                config.Fallbacks.RemoveAt(k);
                                config.Fallbacks.Insert(k + 1, name);
                            }
                            EditorGUI.EndDisabledGroup();

                            if (GUILayout.Button("x"))
                            {
                                config.Fallbacks.RemoveAt(k);
                            }

                            EditorGUILayout.EndHorizontal();

                        }

                        EditorGUI.indentLevel--;

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();

                }

                if (GUILayout.Button("+"))
                {
                    SetNameOrDeleteOptimizedScreen popup = new SetNameOrDeleteOptimizedScreen(null, () => PopupWindow.focusedWindow.Close());
                    PopupWindow.Show(new Rect(0, 0, 400, 175), popup);

                }


                // Screen Configuration Global Fallback

                EditorGUILayout.Separator();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Fallback", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(fallbackName, new GUIContent("Fallback Name"));
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Optimized Screen Size", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(curOptRes, new GUIContent("Optimized Resolution"));
                EditorGUILayout.PropertyField(curOptDpi, new GUIContent("Optimized Dpi"));
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
            }

            // STATIC METHODS

            DrawCategoryButton("Static Sizer Methods", Category.StaticMethods);

            if (category == Category.StaticMethods)
            {
                EditorGUILayout.HelpBox(@"Static Sizer Methods should have the following declaration:", MessageType.Info);

                EditorGUILayout.SelectableLabel("public static float MethodName(Component caller, Vector2 optimizedResolution, Vector2 actualResolution, float optimizedDpi, float actualDpi)",
                    GUI.skin.textField);

                for(int i = 0; i < 5; i++)
                {
                    DrawStaticSizerMethod(staticSizerMethods, i);
                }

                EditorGUILayout.Space();
            }

            // DPI OVERRIDES

            DrawCategoryButton("DPI Overwrites", Category.DpiOverwrites);

            if (category == Category.DpiOverwrites)
            {
                EditorGUILayout.HelpBox(
                    "Some devices have no or wrong DPI values assigned by the manufacturer. " +
                    "Here these devices can be specified to assign the correct DPI value to them.", MessageType.Info);

                dpiList.DoLayoutList();

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();


            // SAVE

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save", GUILayout.Height(50)))
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(monitor);
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawCategoryButton(string text, Category category)
        {
            string prefix = (this.category == category) ? "▼" : "►";
            if(GUILayout.Button(string.Format("{0} {1}", prefix, text), EditorStyles.boldLabel))
            {
                this.category = category;
            }
        }

        private void DrawStaticSizerMethod(SerializedProperty staticSizerMethods, int i)
        {
            SerializedProperty prop = staticSizerMethods.GetArrayElementAtIndex(i);
            SerializedProperty aProp = prop.FindPropertyRelative("assemblyName");
            SerializedProperty tProp = prop.FindPropertyRelative("typeName");
            SerializedProperty mProp = prop.FindPropertyRelative("methodName");

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Static Sizer Method " + (i + 1), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(aProp);
            EditorGUILayout.PropertyField(tProp);
            EditorGUILayout.PropertyField(mProp);

            EditorGUILayout.EndVertical();
        }

        private void MoveToIndex(ScreenTypeConditions config, int idx)
        {
            // this seems not to be done immediately, so the adjustment of idx is not required for some reason.
            ResolutionMonitor.Instance.OptimizedScreens.Remove(config);
            ResolutionMonitor.Instance.OptimizedScreens.Insert(idx, config);
        }

        void DrawConfigPart<T>(string title, T obj, Action<T> drawObject)
            where T : IIsActive
        {
            GUIStyle style = (obj.IsActive) ? EditorStyles.boldLabel : EditorStyles.label;
            obj.IsActive = EditorGUILayout.ToggleLeft(title, obj.IsActive, style);
            
            if (obj.IsActive)
            {
                EditorGUI.indentLevel += 1;

                drawObject(obj);

                EditorGUI.indentLevel -= 1;
            }
        }
        
    }
}
