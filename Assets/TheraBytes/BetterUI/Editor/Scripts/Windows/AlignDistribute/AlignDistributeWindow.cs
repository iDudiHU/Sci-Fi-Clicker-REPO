using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor.AlignDistribute
{
    public class AlignDistributeWindow : EditorWindow
    {
        private static ActiveWindow activeWindow = ActiveWindow.Align;
        private static AlignTo alignTo = AlignTo.SelectionBounds;
        internal static DistributeTo distributeTo = DistributeTo.SelectionBounds;
        private static DistanceOption distanceOption = DistanceOption.Space;
        private static SortOrder sortOrder;
        internal static AnchorMode anchorMode = AnchorMode.FollowObject;

        private static readonly string[] alignToOptions = System.Enum.GetNames(typeof(AlignTo));
        private static readonly string[] distanceOptions = System.Enum.GetNames(typeof(DistanceOption));
        private static readonly string[] anchorModeOptions = System.Enum.GetNames(typeof(AnchorMode));

        private static bool showPadding = true;
        private static float paddingLeftBottomPixels = 0f;
        private static float paddingRightTopPixels = 0f;

        private Texture2D alignLeft, alignCenter, alignRight, alignBottom, alignMiddle, alignTop;
        private Texture2D distributeHorizontal, distributeVertical;

        [MenuItem("Tools/Better UI/Align and Distribute", false, 31)]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(AlignDistributeWindow), false, "Align/Distribute");
            window.minSize = new Vector2(270, 350);
        }

        private void OnEnable()
        {
            alignLeft = Resources.Load<Texture2D>("allign_left");
            alignCenter = Resources.Load<Texture2D>("allign_center");
            alignRight = Resources.Load<Texture2D>("allign_right");
            alignBottom = Resources.Load<Texture2D>("allign_bottom");
            alignMiddle = Resources.Load<Texture2D>("allign_middle");
            alignTop = Resources.Load<Texture2D>("allign_top");

            distributeHorizontal = Resources.Load<Texture2D>("distribute_horizontally");
            distributeVertical = Resources.Load<Texture2D>("distribute_vertically");
        }

        private void OnSelectionChange()
        {
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 2f;
            EditorGUILayout.Space();
            DrawModeSelection();
            EditorGUILayout.Space();

            DrawSelectionInfo();

            switch (activeWindow)
            {
                case ActiveWindow.Align:
                    DrawAlignButtons();
                    EditorGUILayout.Space();

                    DrawAlignTo();
                    break;

                case ActiveWindow.Distribute:
                    DrawDistributeButtons();
                    EditorGUILayout.Space();

                    DrawPadding();
                    EditorGUILayout.Space();

                    DrawOrderOptions();
                    EditorGUILayout.Space();

                    DrawDistributeTo();
                    EditorGUILayout.Space();

                    DrawDistanceOptions();
                    break;
            }

            EditorGUILayout.Space();
            DrawAnchorMode();

            EditorGUIUtility.labelWidth = 0f;
        }

        private void DrawModeSelection()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle((activeWindow == ActiveWindow.Align), "Align", EditorStyles.miniButtonLeft))
            {
                activeWindow = ActiveWindow.Align;
            }

            if (GUILayout.Toggle((activeWindow == ActiveWindow.Distribute), "Distribute", EditorStyles.miniButtonRight))
            {
                activeWindow = ActiveWindow.Distribute;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelectionInfo()
        {
            SelectionStatus selectionStatus = Utility.IsSelectionValid();
            if (selectionStatus != SelectionStatus.Valid)
            {
                DrawInvalidSelectionInfo(selectionStatus);
            }
            else
            {
                Transform[] selection = Selection.transforms;

                string label = (selection.Length == 1) ? selection[0].name : string.Format("{0} UI Elements", selection.Length);
                EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space();
        }

        private void DrawInvalidSelectionInfo(SelectionStatus selectionStatus)
        {
            GUIStyle warn = GUI.skin.GetStyle("WarningOverlay");
            
            EditorGUI.BeginDisabledGroup(true);

            string message;

            switch (selectionStatus)
            {
                case SelectionStatus.NothingSelected:
                    message = "Nothing selected";
                    break;

                case SelectionStatus.ParentIsNull:
                case SelectionStatus.ParentIsNoRectTransform:
                    message = "Objects must be inside a Canvas.";
                    break;

                case SelectionStatus.ContainsNoRectTransform:
                    message = "All objects must have a RectTransform.";
                    break;

                case SelectionStatus.UnequalParents:
                    message = "Objects must have the same parent.";
                    break;

                case SelectionStatus.Valid:
                    // Function should never be called when selection is valid.
                    message = "Unknown problem discovered.";
                    break;

                default:
                    Debug.LogError("Invalid SelectionStatus: " + selectionStatus);
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.TextArea(message, warn);
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAlignButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(alignLeft, "Align to the left"), GUILayout.Width(60f), GUILayout.Height(60f)))
            {
                Align.AlignSelection(AlignMode.Left, alignTo);
            }
            if (GUILayout.Button(new GUIContent(alignCenter, "Align to the center"), GUILayout.Width(60f), GUILayout.Height(60f)))
            {
                Align.AlignSelection(AlignMode.Vertical, alignTo);
            }
            if (GUILayout.Button(new GUIContent(alignRight, "Align to the right"), GUILayout.Width(60f), GUILayout.Height(60f)))
            {
                Align.AlignSelection(AlignMode.Right, alignTo);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(alignTop, "Align to the top"), GUILayout.Width(60f), GUILayout.Height(60f)))
            {
                Align.AlignSelection(AlignMode.Top, alignTo);
            }
            if (GUILayout.Button(new GUIContent(alignMiddle, "Align to the middle"), GUILayout.Width(60f), GUILayout.Height(60f)))
            {
                Align.AlignSelection(AlignMode.Horizontal, alignTo);
            }
            if (GUILayout.Button(new GUIContent(alignBottom, "Align to the bottom"), GUILayout.Width(60f), GUILayout.Height(60f)))
            {
                Align.AlignSelection(AlignMode.Bottom, alignTo);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDistributeButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(distributeHorizontal, "Distribute horizontally"), GUILayout.Width(60f), GUILayout.Height(60f)))
            {
                if (Selection.GetTransforms(SelectionMode.Unfiltered).Length > 1)
                {
                    Distribute.DistributeSelection(AlignMode.Horizontal, distanceOption, sortOrder,
                        paddingLeftBottomPixels, paddingRightTopPixels);
                }
                else
                {
                    Align.AlignSelection(AlignMode.Horizontal, AlignTo.Parent);
                }
            }
            if (GUILayout.Button(new GUIContent(distributeVertical, "Distribute vertically"), GUILayout.Width(60f), GUILayout.Height(60f)))
            {
                if (Selection.GetTransforms(SelectionMode.Unfiltered).Length > 1)
                {
                    Distribute.DistributeSelection(AlignMode.Vertical, distanceOption, sortOrder,
                        paddingLeftBottomPixels, paddingRightTopPixels);
                }
                else
                {
                    Align.AlignSelection(AlignMode.Vertical, AlignTo.Parent);
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDistanceOptions()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Align by", GUILayout.ExpandWidth(false));
            distanceOption = (DistanceOption) EditorGUILayout.Popup((int)distanceOption, distanceOptions);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPadding()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Padding");
                if (showPadding)
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    GUILayout.Space(25);
                    EditorGUILayout.LabelField("Left / Bottom", GUILayout.Width(80));
                    paddingLeftBottomPixels = EditorGUILayout.FloatField(paddingLeftBottomPixels);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(25);
                    EditorGUILayout.LabelField("Right / Top", GUILayout.Width(80));
                    paddingRightTopPixels = EditorGUILayout.FloatField(paddingRightTopPixels);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawOrderOptions()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sorting Order", GUILayout.Width(100f));

            EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Toggle((sortOrder == SortOrder.Positional), "Positional", "Radio"))
                {
                    sortOrder = SortOrder.Positional;
                }
                if (GUILayout.Toggle((sortOrder == SortOrder.Hierarchical), "Hierarchical", "Radio"))
                {
                    sortOrder = SortOrder.Hierarchical;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDistributeTo()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Distribute along", GUILayout.Width(100f));

            EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Toggle(distributeTo == DistributeTo.SelectionBounds, "Selection Bounds", "Radio"))
                {
                    distributeTo = DistributeTo.SelectionBounds;
                }
                if (GUILayout.Toggle(distributeTo == DistributeTo.Parent, "Parent", "Radio"))
                {
                    distributeTo = DistributeTo.Parent;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAlignTo()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Align to", GUILayout.ExpandWidth(false));
            alignTo = (AlignTo)EditorGUILayout.Popup((int)alignTo, alignToOptions);

            EditorGUILayout.EndHorizontal();
        }


        private void DrawAnchorMode()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Anchors", GUILayout.ExpandWidth(false));
            anchorMode = (AnchorMode)EditorGUILayout.Popup((int)anchorMode, anchorModeOptions);

            EditorGUILayout.EndHorizontal();
        }
    }
}
