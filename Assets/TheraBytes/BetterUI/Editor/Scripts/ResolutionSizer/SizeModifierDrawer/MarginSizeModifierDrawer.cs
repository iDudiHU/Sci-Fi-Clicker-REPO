using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace TheraBytes.BetterUi.Editor
{
    [CustomPropertyDrawer(typeof(MarginSizeModifier))]
    public class MarginSizeModifierDrawer : ScreenDependentSizeDrawer<Margin>
    {
        Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        protected override void DrawModifiers(SerializedProperty property)
        {
            var modLeft = property.FindPropertyRelative("ModLeft");
            DrawModifierList(modLeft, "Left Modification");

            var modRight = property.FindPropertyRelative("ModRight");
            DrawModifierList(modRight, "Right Modification");

            var modTop = property.FindPropertyRelative("ModTop");
            DrawModifierList(modTop, "Top Modification");

            var modBottom = property.FindPropertyRelative("ModBottom");
            DrawModifierList(modBottom, "Bottom Modification");
        }

        protected override void ShowField(SerializedProperty parentProp, string propName, string displayName, ref Margin value)
        {
            if (!(foldouts.ContainsKey(displayName)))
            {
                foldouts.Add(displayName, false);
            }

            string title = string.Format("{0} {1}", displayName, GetValueString(value));

            EditorGUI.indentLevel += 1;
            foldouts[displayName] = EditorGUILayout.Foldout(foldouts[displayName], title);

            if (foldouts[displayName])
            {
                SerializedProperty prop = parentProp.FindPropertyRelative(propName);
                SerializedProperty left = prop.FindPropertyRelative("left");
                EditorGUILayout.PropertyField(left);
                SerializedProperty right = prop.FindPropertyRelative("right");
                EditorGUILayout.PropertyField(right);
                SerializedProperty top = prop.FindPropertyRelative("top");
                EditorGUILayout.PropertyField(top);
                SerializedProperty bottom = prop.FindPropertyRelative("bottom");
                EditorGUILayout.PropertyField(bottom);
            }

            EditorGUI.indentLevel -= 1;
        }

        protected override string GetValueString(Margin obj)
        {
            if (obj == null)
                return "(null)";

            return string.Format("({0}, {1}, {2}, {3})", obj.Left, obj.Right, obj.Top, obj.Bottom);
        }
    }
}
