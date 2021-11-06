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
    [CustomEditor(typeof(BetterSelectable)), CanEditMultipleObjects]
    public class BetterSelectableEditor : SelectableEditor
    {
        BetterElementHelper<Selectable, BetterSelectable> helper =
            new BetterElementHelper<Selectable, BetterSelectable>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            helper.DrawGui(serializedObject, target);

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/Selectable/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            Selectable sel = command.context as Selectable;
            Betterizer.MakeBetter<Selectable, BetterSelectable>(sel);
        }
    }
}
