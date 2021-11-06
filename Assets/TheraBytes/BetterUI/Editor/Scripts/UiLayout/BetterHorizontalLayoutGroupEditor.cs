using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
#pragma warning disable 0618

    [CustomEditor(typeof(BetterHorizontalLayoutGroup)), CanEditMultipleObjects]
    public class BetterHorizontalLayoutGroupEditor
        : BetterHorizontalOrVerticalLayoutGroupEditor<HorizontalLayoutGroup, BetterHorizontalLayoutGroup>
    {

        public override void OnInspectorGUI()
        {
            base.DrawObsoleteWarning();
            base.OnInspectorGUI();
        }
    }
#pragma warning restore 0618

}
