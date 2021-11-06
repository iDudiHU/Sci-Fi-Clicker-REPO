using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class SmartParentWindow : EditorWindow
    {
        bool isFreeMovementEnabled;

        RectTransform selection;
        RectTransformData previousTransform;


        Texture2D snapAllPic, snapVerticalPic, snapHorizontalPic, freeParentModeOnPic, freeParentModeOffPic;
        GUIContent snapAllContent, snapVerticalContent, snapHorizontalContent, freeParentModeOnContent, freeParentModeOffContent;

        [MenuItem("Tools/Better UI/Smart Parent", false, 30)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SmartParentWindow), false, "Smart Parent");
        }

        void OnEnable()
        {
            minSize = new Vector2(195, 245);
            isFreeMovementEnabled = false;

            snapAllPic = Resources.Load<Texture2D>("snap_to_childs_all");
            snapHorizontalPic = Resources.Load<Texture2D>("snap_to_childs_h");
            snapVerticalPic = Resources.Load<Texture2D>("snap_to_childs_v");
            freeParentModeOnPic = Resources.Load<Texture2D>("free_parent_mode_on");
            freeParentModeOffPic = Resources.Load<Texture2D>("free_parent_mode_off");

            snapAllContent = new GUIContent(snapAllPic, "Trims size to children horizontally and vertically. Also snap Anchors to borders.");
            snapVerticalContent = new GUIContent(snapVerticalPic, "Trims size to children vertically. Also snap Anchors to borders vertically.");
            snapHorizontalContent = new GUIContent(snapHorizontalPic, "Trims size to children horizontally. Also snap Anchors to borders horizontally.");
            freeParentModeOnContent = new GUIContent(freeParentModeOnPic, "When this mode is enabled children are not moved along with the parent.");
            freeParentModeOffContent = new GUIContent(freeParentModeOffPic, "When this mode is enabled children are not moved along with the parent.");

            Selection.selectionChanged += SelectionChanged;
            EditorApplication.update += UpdateTransforms;

            SelectionChanged();
        }

        void OnDisable()
        {
            isFreeMovementEnabled = false;

            Selection.selectionChanged -= SelectionChanged;
            EditorApplication.update -= UpdateTransforms;
        }


        void OnGUI()
        {
            EditorGUILayout.Space();

            var go = Selection.activeObject as GameObject;
            bool canSelectParent = Selection.objects.Length == 1
                && go != null
                && go.transform as RectTransform != null
                && go.transform.parent != null;

            if (canSelectParent)
            {
                if (GUILayout.Button("Select Parent", EditorStyles.miniButton))
                {
                    Selection.activeObject = go.transform.parent.gameObject;
                }
            }
            else
            {
                GUILayout.Label("");
            }

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(selection == null);

            if (selection != null)
            {
                EditorGUILayout.LabelField(selection.name, EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                DrawEmphasisedLabel("No valid object selected.");
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            #region snap all
            if (GUILayout.Button(snapAllContent, GUILayout.Width(120), GUILayout.Height(120)))
            {
                SnapToChildren(true, true);
            }
            #endregion

            #region snap vertically

            if (GUILayout.Button(snapVerticalContent, GUILayout.Width(60), GUILayout.Height(120)))
            {
                SnapToChildren(false, true);
            }

            #endregion

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            #region snap horizontally
            if (GUILayout.Button(snapHorizontalContent, GUILayout.Width(120), GUILayout.Height(60)))
            {
                SnapToChildren(true, false);
            }
            #endregion

            EditorGUI.EndDisabledGroup();

            #region free parent mode

            bool prev = isFreeMovementEnabled;
            var content = (prev) ? freeParentModeOnContent : freeParentModeOffContent;
            isFreeMovementEnabled = GUILayout.Toggle(isFreeMovementEnabled, content, "Button", GUILayout.Width(60), GUILayout.Height(60));

            bool turnedOn = !prev && isFreeMovementEnabled;
            if (turnedOn && selection != null)
            {
                previousTransform = new RectTransformData(selection);
            }

            #endregion

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (isFreeMovementEnabled && selection != null)
            {
                DrawEmphasisedLabel("Children are detached.");
            }
        }

        private static void DrawEmphasisedLabel(string text)
        {
            GUIStyle warn = GUI.skin.GetStyle("WarningOverlay");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label(text, warn);
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }

        private void SelectionChanged()
        {
            var sel = Selection.GetFiltered(typeof(RectTransform), SelectionMode.TopLevel);

            if (sel.Length != 1)
            {
                selection = null;
                this.Repaint();
                return;
            }

            var rt = sel[0] as RectTransform;
            if(rt.childCount == 0 || rt.parent == null)
            {
                selection = null;
                this.Repaint();
                return;
            }

            if (rt  == selection)
                return;

            selection = rt;
            previousTransform = new RectTransformData(selection);

            this.Repaint();
        }

        private void UpdateTransforms()
        {
            if (!isFreeMovementEnabled || selection == null)
                return;

            RectTransformData currentTransform = new RectTransformData(selection);
            UpdateTransforms(selection, currentTransform, previousTransform);
            previousTransform = currentTransform;
        }

        private void SnapToChildren(bool snapHorizontally, bool snapVertically)
        {
            if (selection == null)
                return;

            if (selection.childCount == 0)
                return;

            float xMin = float.MaxValue;
            float yMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMax = float.MinValue;

            foreach (var child in selection)
            {
                var rt = child as RectTransform;
                if (rt == null)
                    continue;

                Rect rect = rt.ToScreenRect(startAtBottom: true);

                xMin = Mathf.Min(xMin, rect.xMin);
                yMin = Mathf.Min(yMin, rect.yMin);
                xMax = Mathf.Max(xMax, rect.xMax);
                yMax = Mathf.Max(yMax, rect.yMax);
            }

            Rect childBounds = Rect.MinMaxRect(xMin, yMin, xMax, yMax);

            var parent = selection.parent as RectTransform;
            Rect parentRect = (parent != null)
                ? parent.ToScreenRect(startAtBottom: true)
                : new Rect(0, 0, Screen.width, Screen.height);

            RectTransformData prev = new RectTransformData(selection);
            RectTransformData cur = new RectTransformData().PullFromData(prev);


            if (snapHorizontally)
            {
                Snap(cur, 0, childBounds.xMin, childBounds.xMax, parentRect.xMin, parentRect.width);
            }

            if (snapVertically)
            {
                Snap(cur, 1, childBounds.yMin, childBounds.yMax, parentRect.yMin, parentRect.height);
            }

            #region do actual operation with undo

            Undo.RecordObject(selection, "Snap To Children " + DateTime.Now.ToFileTime());
            int group = Undo.GetCurrentGroup();

            // push!
            cur.PushToTransform(selection);

            foreach(Transform child in selection)
            {
                Undo.RecordObject(child, "transform child");
            }

            if (!isFreeMovementEnabled)
            {
                // update child positions
                UpdateTransforms(selection, cur, prev);
            }

            Undo.CollapseUndoOperations(group);

            #endregion
        }

        private static void Snap(RectTransformData data, int axis, float min, float max, float parentMin, float parentSize)
        {
            data.AnchorMin[axis] = (min - parentMin) / parentSize;
            data.AnchorMax[axis] = (max - parentMin) / parentSize;

            data.AnchoredPosition[axis] = 0;
            data.SizeDelta[axis] = 0;
        }

        private static void UpdateTransforms(RectTransform selection, RectTransformData currentTransform, RectTransformData previousTransform)
        {
            if (currentTransform == previousTransform)
                return;

            RectTransform parent = selection.parent as RectTransform;
            Rect parentRect = parent.rect;

            Rect cur = currentTransform.ToRect(parentRect, relativeSpace: true);
            bool isCurZero = Mathf.Approximately(cur.width, 0) || Mathf.Approximately(cur.height, 0);

            Rect prev = previousTransform.ToRect(parentRect, relativeSpace: true);
            bool isPrevZero = Mathf.Approximately(prev.width, 0) || Mathf.Approximately(prev.height, 0);

            if (isCurZero || isPrevZero)
            {
                return;
            }

            float scaleH = 1 / cur.width;
            float scaleV = 1 / cur.height;

            foreach (var child in selection)
            {
                RectTransform rt = child as RectTransform;
                if (rt == null)
                    continue;


                // prev to parent-parent-relative-space

                float xMin = prev.x + prev.width * rt.anchorMin.x;
                float xMax = prev.x + prev.width * rt.anchorMax.x;

                float yMin = prev.y + prev.height * rt.anchorMin.y;
                float yMax = prev.y + prev.height * rt.anchorMax.y;


                // parent-parent-relative-space to cur

                xMin = xMin * scaleH - cur.x * scaleH;
                xMax = xMax * scaleH - cur.x * scaleH;

                yMin = yMin * scaleV - cur.y * scaleV;
                yMax = yMax * scaleV - cur.y * scaleV;


                // assign calculated values

                rt.anchorMin = new Vector2(xMin, yMin);
                rt.anchorMax = new Vector2(xMax, yMax);
            }
        }
    }
}
