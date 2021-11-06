using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace TheraBytes.BetterUi.Editor
{
    [CustomPropertyDrawer(typeof(FloatSizeModifier))]
    public class FloatSizeModifierDrawer : ScreenDependentSizeDrawer<float>
    {
        protected override void DrawModifiers(SerializedProperty property)
        {
            var mod = property.FindPropertyRelative("Mod");
            DrawModifierList(mod, "Size Modification");
        }

        protected override string GetValueString(float obj)
        {
            return obj.ToString();
        }
    }
}
