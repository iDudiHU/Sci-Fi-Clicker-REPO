using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(TransformScaler)), CanEditMultipleObjects]
    public class TransformScalerEditor : UnityEditor.Editor
    {
        SerializedProperty scaleSizerFallback, scaleSizerCollection;

        void OnEnable()
        {
            scaleSizerFallback = serializedObject.FindProperty("scaleSizerFallback");
            scaleSizerCollection = serializedObject.FindProperty("customScaleSizers");
        }

        public override void OnInspectorGUI()
        {
            ScreenConfigConnectionHelper.DrawSizerGui("Scale Settings", scaleSizerCollection, ref scaleSizerFallback);
        }
    }
}
