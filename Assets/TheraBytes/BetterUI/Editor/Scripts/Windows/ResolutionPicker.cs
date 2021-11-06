using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TheraBytes.BetterUi.Editor
{
    public class ResolutionPicker : EditorWindow
    {
        internal enum TextDisplayMode
        {
            Size,
            Name,
            Both,
        }

        internal class GameViewSize
        {
            internal int width;
            internal int height;
            internal string baseText;
            internal string displayText;
            internal bool isAspectRatio;
            internal int index;
            internal bool isCustom;

            internal string sizeText { get { return (isAspectRatio) ? string.Format("{0}:{1}", width, height) : string.Format("{0}x{1}", width, height); } }

            public object ToInternalObject()
            {
                Assembly ass = typeof(EditorApplication).Assembly;
                Type t = ass.GetType("UnityEditor.GameViewSize");
                Type sizeType = ass.GetType("UnityEditor.GameViewSizeType");
                var constructor = t.GetConstructor(new Type[] { sizeType, typeof(int), typeof(int), typeof(string) });
                return constructor.Invoke(new object[] { (isAspectRatio) ? 0 : 1, width, height, baseText });
            }
        }


        static Assembly assembly;
        static EditorWindow gameView;

        [MenuItem("Tools/Better UI/Pick Resolution", false, 60)]
        public static void ShowWindow()
        {
            assembly = typeof(EditorWindow).Assembly;

            var win = EditorWindow.GetWindow<ResolutionPicker>("Pick Resolution") as ResolutionPicker;
            win.minSize = new Vector2(20, 40);
            win.RefreshSizes();
        }


        Type gameSizeType;
        PropertyInfo selectedIndex;

        List<GameViewSize> sizes = new List<GameViewSize>();
        StoredEditorBool
            displayPortrait = new StoredEditorBool("resolutionPicker.displayPortrait", true),
            displayLandscape = new StoredEditorBool("resolutionPicker.displayLandscape", true),
            displayFree = new StoredEditorBool("resolutionPicker.displayFree", true),
            displayBuiltin = new StoredEditorBool("resolutionPicker.displayBuiltin", true),
            displayCustom = new StoredEditorBool("resolutionPicker.displayCustom", true);

        StoredEditorBool
            showOrientationHint = new StoredEditorBool("resolutionPicker.showOrientationHint", true),
            markCustom = new StoredEditorBool("resolutionPicker.markCustom", true);

        StoredEditorBool useBigButtons = new StoredEditorBool("resolutionPicker.bigButtons", false);
        StoredEditorBool useVerticalLayout = new StoredEditorBool("resolutionPicker.verticalLayout", true);

        StoredEditorInt textMode = new StoredEditorInt("reslutionPicker.textMode", (int)TextDisplayMode.Both);

        StoredEditorBool
            displayScreenConfigs = new StoredEditorBool("resolutionPicker.displayScreenConfigs", true),
            applyScreenConfigResolution = new StoredEditorBool("resolutionPicker.applyScreenConfigResolution", true);

        int builtinCount;

        void RefreshSizes()
        {
            assembly = typeof(EditorWindow).Assembly;

            Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            gameView = Resources.FindObjectsOfTypeAll(gameViewType)
                    .FirstOrDefault() as UnityEditor.EditorWindow;

            if (gameView == null)
                return;

            Type gameSizesType = Type.GetType("UnityEditor.GameViewSizes," + assembly);
            Type gameSizeGroupType = Type.GetType("UnityEditor.GameViewSizeGroup," + assembly);
            gameSizeType = Type.GetType("UnityEditor.GameViewSize," + assembly);

            object gameSizesInstance = gameSizesType.BaseType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
            object gameViewGroup = gameSizesType.GetProperty("currentGroup").GetValue(gameSizesInstance, null);

            int count = (int)gameSizeGroupType.InvokeMember("GetTotalCount", BindingFlags.InvokeMethod, null, gameViewGroup, null);
            builtinCount = (int)gameSizeGroupType.InvokeMember("GetBuiltinCount", BindingFlags.InvokeMethod, null, gameViewGroup, null);

            sizes.Clear();
            for (int i = 0; i < count; i++)
            {
                object gameSize = gameSizeGroupType.InvokeMember("GetGameViewSize", BindingFlags.InvokeMethod, null, gameViewGroup, new object[] { i });
                bool isCustom = (i >= builtinCount);
                AddSize(gameSizeType, gameSize, i, isCustom);
            }

            selectedIndex = gameView.GetType().GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        void OnGUI()
        {
            if (gameView == null || gameSizeType == null)
            {
                RefreshSizes();
            }

            if (gameView == null) // still null?
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Please open a Game window.", MessageType.Warning);
                if (GUILayout.Button("Open Game View Window", GUILayout.Height(38)))
                {
                    gameView = EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView," + assembly));
                }
                EditorGUILayout.EndHorizontal();
                return;
            }

            Begin(mainSection: false);

            // Resolutions
            Begin(mainSection: true);

            DrawToolStrip(); // settings

            EditorGUILayout.Separator();
            var style = (useBigButtons) ? GUI.skin.button : EditorStyles.toolbarButton;
            var prevAlign = style.alignment;
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 10;

            int currentIndex = (int)selectedIndex.GetValue(gameView, null);

            for (int i = 0; i < sizes.Count; i++)
            {
                if ((displayFree && i == 1) || (displayBuiltin && i == builtinCount))
                {
                    EditorGUILayout.Separator();
                }

                var size = sizes[i];

                if (!(AllowedToShow(size)))
                    continue;

                bool isOptimizedRes = (ResolutionMonitor.IsOptimizedResolution(size.width, size.height));
                bool isSelected = currentIndex == size.index;
                style.fontStyle = (isOptimizedRes)
                    ? ((isSelected) ? FontStyle.BoldAndItalic : FontStyle.Italic)
                    : ((isSelected) ? FontStyle.Bold : FontStyle.Normal);

                if (GUILayout.Button(GetText(size), style))
                {
                    SetResolution(size);
                }
            }

            GUILayout.FlexibleSpace();

            style.fontStyle = FontStyle.Normal;
            End(mainSection: true);

            // Screen Configs
            if (this.displayScreenConfigs)
            {
                Begin(mainSection: true);

                string title = (useVerticalLayout) ? "♦ Screen Configurations" : "♦";
                if (GUILayout.Button(title, EditorStyles.toolbarButton, GUILayout.MinWidth(25)))
                {
                    Selection.activeObject = ResolutionMonitor.Instance;
                }

                EditorGUILayout.Space();


                Action<ScreenTypeConditions, int, int> applyScreenConfig = (config, width, height) =>
                {
                    if (config != null && ResolutionMonitor.SimulatedScreenConfig == config)
                    {
                        ResolutionMonitor.SimulatedScreenConfig = null;
                    }
                    else
                    {
                        ResolutionMonitor.SimulatedScreenConfig = config;

                        if (this.applyScreenConfigResolution)
                        {
                            RefreshSizes();
                            GameViewSize gvs = sizes.FirstOrDefault((o) =>
                                o.width == width && o.height == height);

                            if (gvs == null)
                            {
                                string name = (config != null) ? config.Name : ResolutionMonitor.Instance.FallbackName;
                                AddSizeToUnity(name, width, height);

                                gvs = sizes.FirstOrDefault((o) =>
                                    o.width == width && o.height == height);
                            }

                            if (gvs != null)
                            {
                                SetResolution(gvs);
                            }
                        }
                    }
                };

                if (GUILayout.Button(ResolutionMonitor.Instance.FallbackName + " (Fallback)", style))
                {
                    var resolution = ResolutionMonitor.OptimizedResolutionFallback;
                    applyScreenConfig(null, (int)resolution.x, (int)resolution.y);
                }

                EditorGUILayout.Space();

                foreach (var config in ResolutionMonitor.Instance.OptimizedScreens)
                {
                    if (GUILayout.Button(ResolutionMonitorEditor.GetButtonText(config), style))
                    {
                        applyScreenConfig(config, config.OptimizedWidth, config.OptimizedHeight);
                    }
                }

                GUILayout.FlexibleSpace();
                End(mainSection: true);
            }

            End(mainSection: false);

            style.alignment = prevAlign;
        }

        private void AddSizeToUnity(string name, int width, int height)
        {
            RefreshSizes();
            try
            {
                GameViewSize size = new GameViewSize()
                {
                    baseText = name,
                    displayText = name,
                    width = width,
                    height = height,
                    index = sizes.Count,
                    isCustom = true,
                    isAspectRatio = false,
                };

                Assembly ass = typeof(EditorApplication).Assembly;
                Type gameViewSizesType = ass.GetType("UnityEditor.GameViewSizes");
                var singleType = typeof(ScriptableSingleton<>).MakeGenericType(gameViewSizesType);
                PropertyInfo gameViewSizesInfo = singleType.GetProperty("instance");
                object gameViewSizes = gameViewSizesInfo.GetValue(null, new object[] { });


                PropertyInfo gameViewSizeGroupInfo = gameViewSizesType.GetMember("currentGroup")[0] as PropertyInfo;
                object gameViewSizeGroup = gameViewSizeGroupInfo.GetValue(gameViewSizes, new object[] { });

                MethodInfo addSizeMethod = gameViewSizeGroup.GetType().GetMethod("AddCustomSize");
                addSizeMethod.Invoke(gameViewSizeGroup, new object[] { size.ToInternalObject() });

                MethodInfo saveToHddMethod = gameViewSizesType.GetMethod("SaveToHDD");
                saveToHddMethod.Invoke(gameViewSizes, new object[] { });

                RefreshSizes();
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Couldn't create resolution: {0}", ex);
            }
        }

        void SetResolution(GameViewSize size)
        {
            var type = gameView.GetType();
            selectedIndex.SetValue(gameView, size.index, null);

            if (ResolutionMonitor.IsZoomPossible())
            {
                var method = type.GetMethod("UpdateZoomAreaAndParent", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(gameView, null);
            }
        }

        void Begin(bool mainSection)
        {
            if (useVerticalLayout == mainSection)
                EditorGUILayout.BeginVertical();
            else
                EditorGUILayout.BeginHorizontal();
        }

        void End(bool mainSection)
        {
            if (useVerticalLayout == mainSection)
                EditorGUILayout.EndVertical();
            else
                EditorGUILayout.EndHorizontal();
        }

        bool AllowedToShow(GameViewSize size)
        {
            // special treatment for free aspect
            if (size.width == 0 && size.height == 0)
                return displayFree;

            bool allow = (size.width >= size.height && displayLandscape)
                 || (size.width < size.height && displayPortrait);

            allow = allow && ((size.isCustom && displayCustom)
                        || (!(size.isCustom) && displayBuiltin));

            return allow;
        }

        string GetText(GameViewSize size)
        {
            string result = "";

            if (showOrientationHint)
            {
                if (size.width > size.height)
                {
                    result += "▬ ";
                }
                else if (size.width < size.height)
                {
                    result += " ▌";
                }
            }

            if (markCustom && size.isCustom)
            {
                result += "☺ ";
            }
            switch ((TextDisplayMode)textMode.Value)
            {
                case TextDisplayMode.Size: result += (size.width == 0 && size.height == 0) ? "X:Y" : size.sizeText; break;
                case TextDisplayMode.Name: result += (string.IsNullOrEmpty(size.baseText)) ? size.sizeText : size.baseText; break;
                case TextDisplayMode.Both: result += size.displayText; break;
                default:
                    throw new ArgumentException();
            }


            return result;
        }

        void DrawToolStrip()
        {
            string title = (useVerticalLayout) ? "♠ Settings" : "♠";
            if (GUILayout.Button(title, EditorStyles.toolbarDropDown, GUILayout.MinWidth(25)))
            {
                GenericMenu toolsMenu = new GenericMenu();
                toolsMenu.AddSeparator("");
                toolsMenu.AddDisabledItem(new GUIContent("♥ Resolution Filters"));
                toolsMenu.AddSeparator("");
                toolsMenu.AddItem(new GUIContent("Free Aspect"), displayFree, DisplayFree);
                toolsMenu.AddItem(new GUIContent("Portrait ( ▌ )"), displayPortrait, DisplayPortrait);
                toolsMenu.AddItem(new GUIContent("Landscape ( ▬ )"), displayLandscape, DisplayLandscape);
                toolsMenu.AddSeparator("");
                toolsMenu.AddItem(new GUIContent("Builtin"), displayBuiltin, DisplayBuiltin);
                toolsMenu.AddItem(new GUIContent("Custom ( ☺ )"), displayCustom, DisplayCustom);

                toolsMenu.AddSeparator("");
                toolsMenu.AddDisabledItem(new GUIContent("♦ Screen Configurations"));
                toolsMenu.AddSeparator("");

                toolsMenu.AddItem(new GUIContent("Show"), displayScreenConfigs, DisplayScreenConfigs);
                toolsMenu.AddItem(new GUIContent("Apply Resolution"), applyScreenConfigResolution, ApplyScreenConfigResolution);

                toolsMenu.AddSeparator("");
                toolsMenu.AddDisabledItem(new GUIContent("♣ Options"));
                toolsMenu.AddSeparator("");

                toolsMenu.AddItem(new GUIContent("Text Options/Name and Size"), textMode == (int)TextDisplayMode.Both, TextModeBoth);
                toolsMenu.AddItem(new GUIContent("Text Options/Name"), textMode == (int)TextDisplayMode.Name, TextModeName);
                toolsMenu.AddItem(new GUIContent("Text Options/Size"), textMode == (int)TextDisplayMode.Size, TextModeSize);
                toolsMenu.AddSeparator("Text Options/");
                toolsMenu.AddItem(new GUIContent("Text Options/Orientation Hint"), showOrientationHint, ShowOrientationHint);
                toolsMenu.AddSeparator("Text Options/");
                toolsMenu.AddItem(new GUIContent("Text Options/Mark Custom"), markCustom, MarkCustom);

                toolsMenu.AddItem(new GUIContent("Style/Big"), useBigButtons, UseBigButtons);
                toolsMenu.AddItem(new GUIContent("Style/Small"), !useBigButtons, UseSmallButtons);

                toolsMenu.AddItem(new GUIContent("Layout/Vertical"), useVerticalLayout, UseVerticalLayout);
                toolsMenu.AddItem(new GUIContent("Layout/Horizontal"), !useVerticalLayout, UseHorizontalLayout);

                toolsMenu.AddSeparator("");
                toolsMenu.AddItem(new GUIContent("Refresh List"), false, RefreshSizes);


                toolsMenu.DropDown(new Rect(0, 0, 0, 16));
                EditorGUIUtility.ExitGUI();
            }
        }

        void DisplayPortrait() { this.displayPortrait.Value = !(this.displayPortrait); }
        void DisplayLandscape() { this.displayLandscape.Value = !(this.displayLandscape); }
        void DisplayFree() { this.displayFree.Value = !(this.displayFree); }
        void DisplayBuiltin() { this.displayBuiltin.Value = !(this.displayBuiltin); }
        void DisplayCustom() { this.displayCustom.Value = !(this.displayCustom); }

        void ShowOrientationHint() { showOrientationHint.Value = !(this.showOrientationHint); }
        void MarkCustom() { markCustom.Value = !(this.markCustom); }
        void TextModeBoth() { this.textMode.Value = (int)TextDisplayMode.Both; }
        void TextModeSize() { this.textMode.Value = (int)TextDisplayMode.Size; }
        void TextModeName() { this.textMode.Value = (int)TextDisplayMode.Name; }
        void UseBigButtons() { this.useBigButtons.Value = true; }
        void UseSmallButtons() { this.useBigButtons.Value = false; }
        void UseVerticalLayout() { this.useVerticalLayout.Value = true; }
        void UseHorizontalLayout() { this.useVerticalLayout.Value = false; }
        void DisplayScreenConfigs() { this.displayScreenConfigs.Value = !(this.displayScreenConfigs); }
        void ApplyScreenConfigResolution() { this.applyScreenConfigResolution.Value = !(this.applyScreenConfigResolution); }


        void AddSize(Type gameSizeType, object gameSize, int index, bool isCustom)
        {
            GameViewSize item = new GameViewSize();
            item.index = index;
            item.isCustom = isCustom;
            item.width = (int)gameSizeType.GetProperty("width").GetValue(gameSize, null);
            item.height = (int)gameSizeType.GetProperty("height").GetValue(gameSize, null);
            item.baseText = (string)gameSizeType.GetProperty("baseText").GetValue(gameSize, null);
            item.displayText = (string)gameSizeType.GetProperty("displayText").GetValue(gameSize, null);
            item.isAspectRatio = ((int)gameSizeType.GetProperty("sizeType").GetValue(gameSize, null)) == 0;

            sizes.Add(item);

        }
    }
}
