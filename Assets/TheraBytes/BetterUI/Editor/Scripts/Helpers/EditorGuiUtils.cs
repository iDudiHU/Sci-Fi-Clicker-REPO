using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace TheraBytes.BetterUi.Editor
{
    public static class EditorGuiUtils
    {

        public static void DrawLayoutList<T>(string listTitle,
            List<T> list, SerializedProperty listProp, ref int count, ref bool foldout,
            Action<SerializedProperty> createCallback, Action<T, SerializedProperty> drawItemCallback)
        {
            EditorGuiUtils.DrawLayoutList(
                listTitle, true,
                list, listProp,
                ref count, ref foldout,
                createCallback, drawItemCallback);
        }

        public static void DrawLayoutList<T>(string listTitle,
            List<T> list, SerializedProperty listProp, ref int count,
            Action<SerializedProperty> createCallback, Action<T, SerializedProperty> drawItemCallback)
        {
            bool foldout = true;
            EditorGuiUtils.DrawLayoutList(
                listTitle, false,
                list, listProp,
                ref count, ref foldout,
                createCallback, drawItemCallback);
        }

        public static void DrawLayoutList<T>(string listTitle, bool usingFoldout,
            List<T> list, SerializedProperty listProp, ref int count, ref bool foldout,
            Action<SerializedProperty> createCallback, Action<T, SerializedProperty> drawItemCallback)
        {

            if (usingFoldout)
            {
                foldout = EditorGUILayout.Foldout(foldout, listTitle);
            }
            else
            {
                EditorGUILayout.LabelField(listTitle, EditorStyles.boldLabel);
                foldout = true;
            }

            if (foldout)
            {
                count = EditorGUILayout.IntField("Count", list.Count);
                EditorGUILayout.Separator();

                if (count < list.Count)
                {
                    for (int i = list.Count - 1; i > count; i--)
                    {
                        listProp.DeleteArrayElementAtIndex(i);
                    }
                    list.RemoveRange(count, list.Count - count);

                }
                else if (count > list.Count)
                {
                    for (int i = list.Count; i < count; i++)
                    {
                        listProp.InsertArrayElementAtIndex(i);
                        listProp.serializedObject.ApplyModifiedProperties();
                        var elemProp = listProp.GetArrayElementAtIndex(i);

                        if (createCallback != null)
                        {
                            createCallback(elemProp);
                        }
                    }
                }


                for (int i = 0; i < list.Count; i++)
                {
                    drawItemCallback(list[i], listProp.GetArrayElementAtIndex(i));
                }
            }
        }

        public static void DrawTransitions(string title,
            List<Transitions> transitions, SerializedProperty transitionsProp, ref int count,
            params string[] stateNames)
        {
            EditorGuiUtils.DrawLayoutList(title,
                transitions, transitionsProp, ref count,
                createCallback: (p) =>
                {
                    var namesProp = p.FindPropertyRelative("stateNames");

                    // unity copies the previous content.
                    // so we need to fill the array only the first time.
                    if (namesProp.arraySize >= stateNames.Length)
                        return;

                    for (int i = 0; i < stateNames.Length; i++)
                    {
                        namesProp.InsertArrayElementAtIndex(i);
                        namesProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        string name = stateNames[i];
                        var elem = namesProp.GetArrayElementAtIndex(i);
                        elem.stringValue = name;
                    }
                    namesProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                },
                drawItemCallback: (item, p) =>
                {
                    TransitionsDrawer.DrawGui(item, p);

                    EditorGUILayout.Separator();
                });
        }
    }
}
