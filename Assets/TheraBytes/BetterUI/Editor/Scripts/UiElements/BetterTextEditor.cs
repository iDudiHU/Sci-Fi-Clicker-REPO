using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterText), true), CanEditMultipleObjects]
    public class BetterTextEditor : GraphicEditor
    {
        SerializedProperty text;
        SerializedProperty sizerFallback, sizerCollection, fitting;
        SerializedProperty font, style, lineSpace, rich, align, geoAlign, overflowH, overflowV;
        SerializedProperty maskable;

        [MenuItem("CONTEXT/Text/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            Text txt = command.context as Text;
            int size = txt.fontSize;
            bool bestFit = txt.resizeTextForBestFit;
            int min = txt.resizeTextMinSize;
            int max = txt.resizeTextMaxSize;

            var newTxt = Betterizer.MakeBetter<Text, BetterText>(txt);

            var betterTxt = newTxt as BetterText;
            if(betterTxt != null)
            {
                if(bestFit)
                {
                    betterTxt.FontSizer.MinSize = min;
                    betterTxt.FontSizer.MaxSize = max;
                }

                betterTxt.FontSizer.SetSize(betterTxt, size);
            }

            Betterizer.Validate(newTxt);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.text = base.serializedObject.FindProperty("m_Text");

            this.sizerFallback = base.serializedObject.FindProperty("fontSizerFallback");
            this.sizerCollection = base.serializedObject.FindProperty("customFontSizers");
            this.fitting = base.serializedObject.FindProperty("fitting");

            var fontData = base.serializedObject.FindProperty("m_FontData");
            this.font = fontData.FindPropertyRelative("m_Font");
            this.style = fontData.FindPropertyRelative("m_FontStyle");
            this.lineSpace = fontData.FindPropertyRelative("m_LineSpacing");
            this.rich = fontData.FindPropertyRelative("m_RichText");
            this.align = fontData.FindPropertyRelative("m_Alignment");
            this.geoAlign = fontData.FindPropertyRelative("m_AlignByGeometry");
            this.overflowH = fontData.FindPropertyRelative("m_HorizontalOverflow");
            this.overflowV = fontData.FindPropertyRelative("m_VerticalOverflow"); 
            this.maskable = base.serializedObject.FindProperty("m_Maskable");
        }
        
        public override void OnInspectorGUI()
        {
            var obj = base.target as BetterText;

            base.serializedObject.Update();
            EditorGUILayout.PropertyField(this.text);

            EditorGUILayout.LabelField("Better Sizing", EditorStyles.boldLabel);

            ScreenConfigConnectionHelper.DrawSizerGui("Font Size", sizerCollection, ref sizerFallback);

            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(fitting);
            EditorGUI.indentLevel -= 1;
            
            EditorGUILayout.LabelField("Character", EditorStyles.boldLabel);

            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(font);
            EditorGUILayout.PropertyField(style);
            EditorGUILayout.PropertyField(lineSpace);
            EditorGUILayout.PropertyField(rich);
            EditorGUI.indentLevel -= 1;

            EditorGUILayout.LabelField("Paragraph", EditorStyles.boldLabel);

            EditorGUI.indentLevel += 1;
            //EditorGUILayout.PropertyField(align);
            DrawAnchorIcons(align, obj.alignment);

            if(geoAlign != null) // not present in old unity versions
                EditorGUILayout.PropertyField(geoAlign);

            EditorGUILayout.PropertyField(overflowH);
            EditorGUILayout.PropertyField(overflowV);
            EditorGUI.indentLevel -= 1;

            base.AppearanceControlsGUI();
            base.RaycastControlsGUI();

            if(maskable != null)
            {
                EditorGUILayout.PropertyField(maskable);
            }

            base.serializedObject.ApplyModifiedProperties();
        }
        
        void DrawAnchorIcons(SerializedProperty prop, TextAnchor anchor)
        {
            bool hLeft = anchor == TextAnchor.LowerLeft || anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.UpperLeft;
            bool hCenter = anchor == TextAnchor.LowerCenter || anchor == TextAnchor.MiddleCenter|| anchor == TextAnchor.UpperCenter;
            bool hRight = anchor == TextAnchor.LowerRight || anchor == TextAnchor.MiddleRight || anchor == TextAnchor.UpperRight;

            bool vTop    = anchor == TextAnchor.UpperLeft || anchor == TextAnchor.UpperCenter || anchor == TextAnchor.UpperRight;
            bool vCenter = anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleRight;
            bool vBottom = anchor == TextAnchor.LowerLeft || anchor == TextAnchor.LowerCenter || anchor == TextAnchor.LowerRight;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Alignment", GUILayout.Width(EditorGUIUtility.labelWidth - 12), GUILayout.ExpandWidth(false));
           
            bool hLeft2 = DrawAlignIcon(Styles.LeftAlignActive, Styles.LeftAlign, TextAlignment.Left, hLeft);
            bool hCenter2 = DrawAlignIcon(Styles.CenterAlignActive, Styles.CenterAlign, TextAlignment.Center, hCenter);
            bool hRight2 = DrawAlignIcon(Styles.RightAlignActive, Styles.RightAlign, TextAlignment.Right, hRight);
            
            if(hLeft != hLeft2 || hCenter != hCenter2 || hRight != hRight2)
            {
                hLeft = (hLeft == hLeft2) ? false : hLeft2;
                hCenter = (hCenter == hCenter2) ? false : hCenter2;
                hRight = (hRight == hRight2) ? false : hRight2;
            }
            

            EditorGUILayout.Separator();

            bool vTop2 = DrawAlignIcon(Styles.TopAlignActive, Styles.TopAlign, TextAlignment.Left, vTop);
            bool vCenter2 = DrawAlignIcon(Styles.MiddleAlignActive, Styles.MiddleAlign, TextAlignment.Center, vCenter);
            bool vBottom2 = DrawAlignIcon(Styles.BottomAlignActive, Styles.BottomAlign, TextAlignment.Right, vBottom);
            
            if(vTop != vTop2 || vCenter != vCenter2 || vBottom != vBottom2)
            {
                vTop = (vTop == vTop2) ? false : vTop2;
                vCenter = (vCenter == vCenter2) ? false : vCenter2;
                vBottom = (vBottom == vBottom2) ? false : vBottom2;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            TextAnchor prev = anchor;
            if(hLeft)
            {
                anchor = (vTop)
                    ? TextAnchor.UpperLeft
                    : ((vCenter)
                        ? TextAnchor.MiddleLeft
                        : TextAnchor.LowerLeft);
            }
            else if(hCenter)
            {
                anchor = (vTop)
                    ? TextAnchor.UpperCenter
                    : ((vCenter)
                        ? TextAnchor.MiddleCenter
                        : TextAnchor.LowerCenter);
            }
            else if(hRight)
            {

                anchor = (vTop)
                    ? TextAnchor.UpperRight
                    : ((vCenter)
                        ? TextAnchor.MiddleRight
                        : TextAnchor.LowerRight);
            }

            if (prev != anchor)
            {
                prop.intValue = (int)anchor;
                prop.serializedObject.ApplyModifiedProperties();
            }
        }

        bool DrawAlignIcon(GUIContent contentActive, GUIContent contentInactive, TextAlignment align, bool value)
        {
            GUIStyle style = null;

            switch (align)
            {
                case TextAlignment.Left:
                    style = Styles.alignmentButtonLeft;
                    break;
                case TextAlignment.Center:
                    style = Styles.alignmentButtonMid;
                    break;
                case TextAlignment.Right:
                    style = Styles.alignmentButtonRight;
                    break;
            }

            if (value)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            bool result = GUILayout.Toggle(value,
                (value) ? contentActive : contentInactive,
                style, GUILayout.ExpandWidth(false));

            if (value)
            {
                EditorGUI.EndDisabledGroup();
            }

            return result;
        }


        private static class Styles
        {
            public static GUIStyle alignmentButtonLeft;
            public static GUIStyle alignmentButtonMid;
            public static GUIStyle alignmentButtonRight;
            public static GUIContent EncodingContent;
            public static GUIContent LeftAlign;
            public static GUIContent CenterAlign;
            public static GUIContent RightAlign;
            public static GUIContent TopAlign;
            public static GUIContent MiddleAlign;
            public static GUIContent BottomAlign;
            public static GUIContent LeftAlignActive;
            public static GUIContent CenterAlignActive;
            public static GUIContent RightAlignActive;
            public static GUIContent TopAlignActive;
            public static GUIContent MiddleAlignActive;
            public static GUIContent BottomAlignActive;

            static Styles()
            {
                alignmentButtonLeft = new GUIStyle(EditorStyles.miniButtonLeft);
                alignmentButtonMid = new GUIStyle(EditorStyles.miniButtonMid);
                alignmentButtonRight = new GUIStyle(EditorStyles.miniButtonRight);

                EncodingContent = new GUIContent("Rich Text", "Use emoticons and colors");
                LeftAlign = EditorGUIUtility.IconContent("GUISystem/align_horizontally_left", "Left Align");
                CenterAlign = EditorGUIUtility.IconContent("GUISystem/align_horizontally_center", "Center Align");
                RightAlign = EditorGUIUtility.IconContent("GUISystem/align_horizontally_right", "Right Align");
                LeftAlignActive = EditorGUIUtility.IconContent("GUISystem/align_horizontally_left_active", "Left Align");
                CenterAlignActive = EditorGUIUtility.IconContent("GUISystem/align_horizontally_center_active", "Center Align");
                RightAlignActive = EditorGUIUtility.IconContent("GUISystem/align_horizontally_right_active", "Right Align");
                TopAlign = EditorGUIUtility.IconContent("GUISystem/align_vertically_top", "Top Align");
                MiddleAlign = EditorGUIUtility.IconContent("GUISystem/align_vertically_center", "Middle Align");
                BottomAlign = EditorGUIUtility.IconContent("GUISystem/align_vertically_bottom", "Bottom Align");
                TopAlignActive = EditorGUIUtility.IconContent("GUISystem/align_vertically_top_active", "Top Align");
                MiddleAlignActive = EditorGUIUtility.IconContent("GUISystem/align_vertically_center_active", "Middle Align");
                BottomAlignActive = EditorGUIUtility.IconContent("GUISystem/align_vertically_bottom_active", "Bottom Align");

                FixAlignmentButtonStyles(new GUIStyle[]{ alignmentButtonLeft, alignmentButtonMid, alignmentButtonRight });
            }

            private static void FixAlignmentButtonStyles(params GUIStyle[] styles)
            {
                GUIStyle[] gUIStyleArray = styles;
                for (int i = 0; i < (int)gUIStyleArray.Length; i++)
                {
                    GUIStyle gUIStyle = gUIStyleArray[i];
                    gUIStyle.padding.left = 2;
                    gUIStyle.padding.right = 2;
                }
            }
        }

        private enum VerticalTextAligment
        {
            Top,
            Middle,
            Bottom
        }
    }
}
