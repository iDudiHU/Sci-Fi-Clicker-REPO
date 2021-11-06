using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    [CustomPropertyDrawer(typeof(RectTransformData)), CanEditMultipleObjects]
    public class RectTransformDataDrawer : PropertyDrawer
    {
        static int floatFieldHash = "FloatFieldHash".GetHashCode();
        private class Styles
        {
            public static readonly GUIStyle lockStyle = EditorStyles.miniButton;
            public static readonly GUIStyle measuringLabelStyle = new GUIStyle("PreOverlayLabel");
            public static readonly GUIContent anchorsContent = new GUIContent("Anchors");
            public static readonly GUIContent anchorMinContent = new GUIContent("Min", "The normalized position in the parent rectangle that the lower left corner is anchored to.");
            public static readonly GUIContent anchorMaxContent = new GUIContent("Max", "The normalized position in the parent rectangle that the upper right corner is anchored to.");
            public static readonly GUIContent positionContent = new GUIContent("Position", "The local position of the rectangle. The position specifies this rectangle's pivot relative to the anchor reference point.");
            public static readonly GUIContent sizeContent = new GUIContent("Size", "The size of the rectangle.");
            public static readonly GUIContent pivotContent = new GUIContent("Pivot", "The pivot point specified in normalized values between 0 and 1. The pivot point is the origin of this rectangle. Rotation and scaling is around this point.");
            public static readonly GUIContent transformScaleContent = new GUIContent("Scale", "The local scaling of this Game Object relative to the parent. This scales everything including image borders and text.");
            public static readonly GUIContent transformPositionZContent = new GUIContent("Pos Z", "Distance to offset the rectangle along the Z axis of the parent. The effect is visible if the Canvas uses a perspective camera, or if a parent RectTransform is rotated along the X or Y axis.");
            public static readonly GUIContent X = new GUIContent("X");
            public static readonly GUIContent Y = new GUIContent("Y");
            public static readonly GUIContent Z = new GUIContent("Z");
        }

        public static float HeightWithAnchorsExpanded { get { return 11.5f * EditorGUIUtility.singleLineHeight + 2; } }
        public static float HeightWithoutAnchorsExpanded { get { return 9.5f * EditorGUIUtility.singleLineHeight + 2; } }

        static Action<RectTransform, RectTransformData> pushCallback, pullCallback;

        public static void OverwritePushPullMethods(Action<RectTransform, RectTransformData> push, Action<RectTransform, RectTransformData> pull)
        {
            pushCallback = push;
            pullCallback = pull;
        }


        bool anchorExpand = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (anchorExpand)
                ? HeightWithAnchorsExpanded
                : HeightWithoutAnchorsExpanded;
        }

        public override void OnGUI(Rect bounds, SerializedProperty prop, GUIContent label)
        {
            RectTransformData data = prop.GetValue<RectTransformData>();
            MonoBehaviour target = prop.serializedObject.targetObject as MonoBehaviour;
            
            GUI.Box(bounds, "");

            if (target != null && target.transform is RectTransform)
            {
                RectTransform rt = target.transform as RectTransform;

                // Pull
                if (GUI.Button(new Rect(bounds.position + new Vector2(5, 5), new Vector2(40, 40)), "Pull\n↓"))
                {
                    Undo.RecordObject(target, "Pull From Rect Transform");

                    if (pullCallback != null)
                    {
                        pullCallback(rt, data);
                    }
                    else
                    {
                        data.PullFromTransform(rt);
                    }
                }

                // Push
                if (GUI.Button(new Rect(bounds.position + new Vector2(50, 25), new Vector2(40, 40)), "↑\nPush"))
                {
                    Undo.RecordObject(target.transform, "Push To Rect Transform");

                    if (pushCallback != null)
                    {
                        pushCallback(rt, data);
                    }
                    else
                    {
                        data.PushToTransform(rt);
                    }
                }
            }

            // Fields
            DrawFields(prop, data, bounds, ref anchorExpand);
        }

        public static void DrawFields(SerializedProperty prop, RectTransformData data, Rect rect, ref bool anchorExpand)
        {
            SerializedProperty pivot = prop.FindPropertyRelative("Pivot");
            SerializedProperty rotation = prop.FindPropertyRelative("rotation");
            SerializedProperty euler = prop.FindPropertyRelative("EulerAngles");
            SerializedProperty saveAsEuler = prop.FindPropertyRelative("saveRotationAsEuler");
            SerializedProperty scale = prop.FindPropertyRelative("Scale");

            rect.width -= 0.5f * EditorGUIUtility.singleLineHeight;
            float yPos = 0;

            Space(ref yPos);

            SmartPositionAndSizeFields(prop, data, rect, ref yPos);
            SmartAnchorFields(prop, data, rect, ref yPos, ref anchorExpand);
            yPos += 2;
            SmartPivotField(pivot, data, rect, ref yPos);
            
            Space(ref yPos);

            RotationField(rotation, euler, saveAsEuler, data, rect, ref yPos);
            ScaleField(scale, data, rect, ref yPos);
            
            Space(ref yPos);

            prop.serializedObject.ApplyModifiedProperties();
        }

        static void Space(ref float yPos)
        {
            yPos += 0.5f * EditorGUIUtility.singleLineHeight;
        }

        private static Rect GetControlRect(float height, Rect rect, ref float yPos)
        {
            float y = yPos;
            yPos += height;

            return new Rect(rect.x, rect.y + y, rect.width, height);
        }

        private static void SmartPositionAndSizeFields(SerializedProperty prop, RectTransformData data, Rect rect, ref float yPos)
        {
            Rect controlRect = GetControlRect(EditorGUIUtility.singleLineHeight * 4f, rect, ref yPos);
            controlRect.height = EditorGUIUtility.singleLineHeight * 2f;


            bool equalAnchorX = data.AnchorMin.x == data.AnchorMax.x;
            bool equalAnchorY = data.AnchorMin.y == data.AnchorMax.y;

            // POS X
            Rect columnRect = GetColumnRect(controlRect, 0);
            if (equalAnchorX)
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("x"));
                FloatFieldLabelAbove(prop, columnRect, () => data.AnchoredPosition.x, (float val) => data.AnchoredPosition = new Vector2(val, data.AnchoredPosition.y), DrivenTransformProperties.AnchoredPositionX, new GUIContent("Pos X"));
                
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("x"));
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("x"));

                FloatFieldLabelAbove(prop,  columnRect, () => data.OffsetMin.x,
                    (float val) => data.OffsetMin = new Vector2(val, data.OffsetMin.y),
                    DrivenTransformProperties.None, new GUIContent("Left"));
                
                EditorGUI.EndProperty();
                EditorGUI.EndProperty();
            }

            // POS Y
            columnRect = GetColumnRect(controlRect, 1);
            if (equalAnchorY)
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("y"));
                FloatFieldLabelAbove(prop, columnRect, () => data.AnchoredPosition.y, (float val) => data.AnchoredPosition = new Vector2(data.AnchoredPosition.x, val), DrivenTransformProperties.AnchoredPositionY, new GUIContent("Pos Y"));
                
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("y"));
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("y"));
                FloatFieldLabelAbove(prop, columnRect, () => -data.OffsetMax.y, (float val) => data.OffsetMax = new Vector2(data.OffsetMax.x, -val), DrivenTransformProperties.None, new GUIContent("Top"));
                
                EditorGUI.EndProperty();
                EditorGUI.EndProperty();
            }

            // POS Z
            columnRect = GetColumnRect(controlRect, 2);
            EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("LocalPosition.z"));
            FloatFieldLabelAbove(prop, columnRect, () => data.LocalPosition.z, (float val) => data.LocalPosition = new Vector3(data.LocalPosition.x, data.LocalPosition.y, val), DrivenTransformProperties.AnchoredPositionZ, Styles.transformPositionZContent);
            EditorGUI.EndProperty();
            controlRect.y = controlRect.y + EditorGUIUtility.singleLineHeight * 2f;

            // Size Delta Width
            columnRect = GetColumnRect(controlRect, 0);
            if (equalAnchorX)
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("x"));
                FloatFieldLabelAbove(prop, columnRect, () => data.SizeDelta.x, (float val) => data.SizeDelta = new Vector2(val, data.SizeDelta.y), DrivenTransformProperties.SizeDeltaX, (equalAnchorX ? new GUIContent("Width") : new GUIContent("W Delta")));
                
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("x"));
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("x"));
                FloatFieldLabelAbove(prop, columnRect, () => -data.OffsetMax.x, (float val) => data.OffsetMax = new Vector2(-val, data.OffsetMax.y), DrivenTransformProperties.None, new GUIContent("Right"));
                
                EditorGUI.EndProperty();
                EditorGUI.EndProperty();
            }

            // Size Delta Height
            columnRect = GetColumnRect(controlRect, 1);
            if (equalAnchorY)
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("y"));
                FloatFieldLabelAbove(prop, columnRect, () => data.SizeDelta.y, (float val) => data.SizeDelta = new Vector2(data.SizeDelta.x, val), DrivenTransformProperties.SizeDeltaY, (equalAnchorY ? new GUIContent("Height") : new GUIContent("H Delta")));
                
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("y"));
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("y"));
                FloatFieldLabelAbove(prop, columnRect, () => data.OffsetMin.y, (float val) => data.OffsetMin = new Vector2(data.OffsetMin.x, val), DrivenTransformProperties.None, new GUIContent("Bottom"));
                
                EditorGUI.EndProperty();
                EditorGUI.EndProperty();
            }

            columnRect = controlRect;
            columnRect.height = EditorGUIUtility.singleLineHeight;
            columnRect.y = columnRect.y + EditorGUIUtility.singleLineHeight;
            columnRect.yMin = columnRect.yMin - 2f;
            columnRect.xMin = columnRect.xMax - 26f;
            columnRect.x = columnRect.x - columnRect.width;
        }

        private static void SmartAnchorFields(SerializedProperty prop, RectTransformData data, Rect rect, ref float yPos, ref bool anchorExpand)
        {
            Rect controlRect = GetControlRect(EditorGUIUtility.singleLineHeight * (float)((!anchorExpand ? 1 : 3)), rect, ref yPos);
            controlRect.x += 10;
            controlRect.width -= 10;

            controlRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            anchorExpand = EditorGUI.Foldout(controlRect, anchorExpand, Styles.anchorsContent);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("RectTransformEditor.showAnchorProperties", anchorExpand);
            }

            if (!anchorExpand)
                return;

            EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

            controlRect.y = controlRect.y + EditorGUIUtility.singleLineHeight;
            Vector2Field(controlRect,
                () => data.AnchorMin.x, (float val) => data.AnchorMin.x = val,
                () => data.AnchorMin.y, (float val) => data.AnchorMin.y = val,
                DrivenTransformProperties.AnchorMinX, DrivenTransformProperties.AnchorMinY,
                prop.FindPropertyRelative("AnchorMin").FindPropertyRelative("x"), prop.FindPropertyRelative("AnchorMin").FindPropertyRelative("y"),
                Styles.anchorMinContent);

            controlRect.y = controlRect.y + EditorGUIUtility.singleLineHeight;
            Vector2Field(controlRect,
                () => data.AnchorMax.x, (float val) => data.AnchorMax.x = val,
                () => data.AnchorMax.y, (float val) => data.AnchorMax.y = val,
                DrivenTransformProperties.AnchorMaxX, DrivenTransformProperties.AnchorMaxY,
                prop.FindPropertyRelative("AnchorMax").FindPropertyRelative("x"), prop.FindPropertyRelative("AnchorMax").FindPropertyRelative("y"),
                Styles.anchorMaxContent);

            EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
        }

        private static void SmartPivotField(SerializedProperty pivotProp, RectTransformData data, Rect rect, ref float yPos)
        {
            Rect controlRect = GetControlRect(EditorGUIUtility.singleLineHeight, rect, ref yPos);
            controlRect.x += 10;
            controlRect.width -= 10;

            Vector2Field(controlRect,
                () => data.Pivot.x, (float val) => data.Pivot.x = val,
                () => data.Pivot.y, (float val) => data.Pivot.y = val,
                DrivenTransformProperties.PivotX, DrivenTransformProperties.PivotY,
                pivotProp.FindPropertyRelative("x"), pivotProp.FindPropertyRelative("y"),
                Styles.pivotContent);
        }

        private static Rect GetColumnRect(Rect totalRect, int column)
        {
            totalRect.xMin = totalRect.xMin - 20 + (EditorGUIUtility.labelWidth - 1f);
            Rect rect = totalRect;
            rect.xMin = rect.xMin + ((totalRect.width - 4f) * ((float)column / 3f) + (float)(column * 2));
            rect.width = (totalRect.width - 4f) / 3f;
            return rect;
        }

        private static void Vector2Field(Rect position, Func<float> xGetter, Action<float> xSetter, Func<float> yGetter, Action<float> ySetter,
            DrivenTransformProperties xDriven, DrivenTransformProperties yDriven, SerializedProperty xProperty, SerializedProperty yProperty, GUIContent label)
        {
            EditorGUI.PrefixLabel(position, -1, label);
            float lblW = EditorGUIUtility.labelWidth;
            int ident = EditorGUI.indentLevel;
            Rect columnRect = GetColumnRect(position, 0);
            Rect rect = GetColumnRect(position, 1);
            EditorGUIUtility.labelWidth = 13f;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginProperty(columnRect, Styles.X, xProperty);
            FloatField(xProperty, columnRect, xGetter, xSetter, xDriven, Styles.X);
            EditorGUI.EndProperty();

            EditorGUI.BeginProperty(columnRect, Styles.Y, yProperty);
            FloatField(yProperty, rect, yGetter, ySetter, yDriven, Styles.Y);
            EditorGUI.EndProperty();

            EditorGUIUtility.labelWidth = lblW;
            EditorGUI.indentLevel = ident;
        }

        private static void ScaleField(SerializedProperty prop, RectTransformData data, Rect rect, ref float yPos)
        {
            Rect controlRect = GetControlRect(EditorGUIUtility.singleLineHeight, rect, ref yPos);
            controlRect.x += 10;
            controlRect.width -= 10;

            EditorGUI.PrefixLabel(controlRect, -1, Styles.transformScaleContent);
            float lblW = EditorGUIUtility.labelWidth;
            int ident = EditorGUI.indentLevel;
            Rect rectX = GetColumnRect(controlRect, 0);
            Rect rectY = GetColumnRect(controlRect, 1);
            Rect rectZ = GetColumnRect(controlRect, 2);
            EditorGUIUtility.labelWidth = 13f;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginProperty(rectX, Styles.X, prop.FindPropertyRelative("x"));
            FloatField(prop, rectX, () => data.Scale.x, (val) => data.Scale.x = val, DrivenTransformProperties.ScaleX, Styles.X);
            EditorGUI.EndProperty();

            EditorGUI.BeginProperty(rectX, Styles.Y, prop.FindPropertyRelative("y"));
            FloatField(prop, rectY, () => data.Scale.y, (val) => data.Scale.y = val, DrivenTransformProperties.ScaleY, Styles.Y);
            EditorGUI.EndProperty();


            EditorGUI.BeginProperty(rectX, Styles.Z, prop.FindPropertyRelative("z"));
            FloatField(prop, rectZ, () => data.Scale.z, (val) => data.Scale.z = val, DrivenTransformProperties.ScaleZ, Styles.Z);
            EditorGUI.EndProperty();

            EditorGUIUtility.labelWidth = lblW;
            EditorGUI.indentLevel = ident;
        }

        private static void RotationField(SerializedProperty rotationProp, SerializedProperty eulerProp, SerializedProperty saveAsEulerProp, RectTransformData data, Rect rect, ref float yPos)
        {
            Rect controlRect = GetControlRect(EditorGUIUtility.singleLineHeight, rect, ref yPos);
            controlRect.x += 10;
            controlRect.width -= 10;

            Rect prefixRect = new Rect(controlRect.x, controlRect.y, EditorGUIUtility.labelWidth - 24, controlRect.height);//EditorGUI.PrefixLabel(controlRect, new GUIContent(""));
            string[] options = { "Rotation (Quaternion)", "Rotation (Euler)" };
            int prevIdx = (data.SaveRotationAsEuler) ? 1 : 0;
            int newIdx = EditorGUI.Popup(prefixRect, prevIdx, options);

            if(prevIdx != newIdx)
            {
                saveAsEulerProp.boolValue = (newIdx == 0) ? false : true;
            }

            float lblW = EditorGUIUtility.labelWidth;
            int ident = EditorGUI.indentLevel;
            Rect rectX = GetColumnRect(controlRect, 0);
            Rect rectY = GetColumnRect(controlRect, 1);
            Rect rectZ = GetColumnRect(controlRect, 2);
            EditorGUIUtility.labelWidth = 13f;
            EditorGUI.indentLevel = 0;

            if (data.SaveRotationAsEuler)
            {
                EditorGUI.BeginProperty(controlRect, GUIContent.none, eulerProp);
                Vector3 euler = data.EulerAngles;

                FloatField(eulerProp, rectX, () => data.EulerAngles.x, (val) => data.EulerAngles = new Vector3(val, euler.y, euler.z), DrivenTransformProperties.Rotation, Styles.X);
                FloatField(eulerProp, rectY, () => data.EulerAngles.y, (val) => data.EulerAngles = new Vector3(euler.x, val, euler.z), DrivenTransformProperties.Rotation, Styles.Y);
                FloatField(eulerProp, rectZ, () => data.EulerAngles.z, (val) => data.EulerAngles = new Vector3(euler.x, euler.y, val), DrivenTransformProperties.Rotation, Styles.Z);

                EditorGUI.EndProperty();

                rotationProp.quaternionValue = Quaternion.Euler(eulerProp.vector3Value);
            }
            else
            {
                EditorGUI.BeginProperty(controlRect, GUIContent.none, rotationProp);
                Vector3 euler = rotationProp.quaternionValue.eulerAngles;

                FloatField(rotationProp, rectX, () => data.Rotation.eulerAngles.x, (val) => data.Rotation = Quaternion.Euler(val, euler.y, euler.z), DrivenTransformProperties.Rotation, Styles.X);
                FloatField(rotationProp, rectY, () => data.Rotation.eulerAngles.y, (val) => data.Rotation = Quaternion.Euler(euler.x, val, euler.z), DrivenTransformProperties.Rotation, Styles.Y);
                FloatField(rotationProp, rectZ, () => data.Rotation.eulerAngles.z, (val) => data.Rotation = Quaternion.Euler(euler.x, euler.y, val), DrivenTransformProperties.Rotation, Styles.Z);

                EditorGUI.EndProperty();

                eulerProp.vector3Value = rotationProp.quaternionValue.eulerAngles;
            }

            EditorGUIUtility.labelWidth = lblW;
            EditorGUI.indentLevel = ident;
        }

        private static void FloatFieldLabelAbove(SerializedProperty prop, Rect position, Func<float> getter, Action<float> setter, DrivenTransformProperties driven, GUIContent label)
        {
            FloatField(prop, position, getter, setter, driven, label, true);
        }

        private static void FloatField(SerializedProperty prop, Rect position, Func<float> getter, Action<float> setter, DrivenTransformProperties driven, GUIContent label, bool labelAbove = false)
        {
            float val = getter();

            EditorGUI.BeginChangeCheck();

            float newVal;
            if (labelAbove)
            {
                int controlID = GUIUtility.GetControlID(floatFieldHash, FocusType.Keyboard, position);
                

                Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                Rect inputRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);


                EditorGUI.HandlePrefixLabel(position, labelRect, label, controlID);
                newVal = EditorGUI.FloatField(inputRect, val);
                
            }
            else
            {
                newVal = EditorGUI.FloatField(position, label, val);
            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(prop.serializedObject.targetObject, "Inspector");
                setter(newVal);
            }
        }
    }
}
