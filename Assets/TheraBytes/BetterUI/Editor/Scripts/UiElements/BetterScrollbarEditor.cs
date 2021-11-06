using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterScrollbar)), CanEditMultipleObjects]
    public class BetterScrollbarEditor : ScrollbarEditor
    {
        BetterElementHelper<Scrollbar, BetterScrollbar> helper =
            new BetterElementHelper<Scrollbar, BetterScrollbar>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            helper.DrawGui(serializedObject, target);

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/Scrollbar/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            Scrollbar obj = command.context as Scrollbar;
            Betterizer.MakeBetter<Scrollbar, BetterScrollbar>(obj);
        }
    }
}
