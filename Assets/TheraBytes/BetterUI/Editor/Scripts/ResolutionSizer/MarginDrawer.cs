using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [CustomPropertyDrawer(typeof(Margin))]
    public class MarginDrawer : PropertyDrawer
    {
        bool foldout;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var margin = fieldInfo.GetValue(property.serializedObject.targetObject)
                       as Margin;

            foldout = EditorGUILayout.Foldout(foldout, "Margin");

            if (foldout)
            {
                margin.Left = EditorGUILayout.IntField("left", margin.Left);
                margin.Right = EditorGUILayout.IntField("right", margin.Right);
                margin.Top = EditorGUILayout.IntField("top", margin.Top);
                margin.Bottom = EditorGUILayout.IntField("bottom", margin.Bottom);
            }

        }
    }
}
