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
    [CustomEditor(typeof(BetterDropdown)), CanEditMultipleObjects]
    public class BetterDropDownEditor : DropdownEditor
    {
        BetterElementHelper<Dropdown, BetterDropdown> helper =
            new BetterElementHelper<Dropdown, BetterDropdown>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BetterDropdown obj = target as BetterDropdown;
            helper.DrawGui(serializedObject, obj);

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/Dropdown/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            Dropdown sel = command.context as Dropdown;
            Betterizer.MakeBetter<Dropdown, BetterDropdown>(sel);
        }
    }
}
