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
    [CustomEditor(typeof(BetterSlider)), CanEditMultipleObjects]
    public class BetterSliderEditor : SliderEditor
    {
        BetterElementHelper<Slider, BetterSlider> helper =
            new BetterElementHelper<Slider, BetterSlider>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BetterSlider obj = target as BetterSlider;
            helper.DrawGui(serializedObject, obj);

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/Slider/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            Slider obj = command.context as Slider;
            Betterizer.MakeBetter<Slider, BetterSlider>(obj);
        }
    }
}
