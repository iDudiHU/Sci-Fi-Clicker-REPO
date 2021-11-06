using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class BetterElementHelper<TBase, TBetter>
        where TBase : MonoBehaviour
        where TBetter : TBase, IBetterTransitionUiElement
    {
        int count = 0;

        public void DrawGui(SerializedObject serializedObject, UnityEngine.Object target)
        {
            TBetter obj = target as TBetter;
            var listProp = serializedObject.FindProperty("betterTransitions");
            
            // Transitions
            EditorGuiUtils.DrawTransitions("Better Transitions", obj.BetterTransitions, listProp,
                ref count, Transitions.SelectionStateNames);
        }
    }
}
