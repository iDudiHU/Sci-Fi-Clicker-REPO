using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterTextMeshProUGUI)), CanEditMultipleObjects]
    public class BetterTextMeshProUGUIEditor : TMPro.EditorUtilities.TMP_EditorPanelUI
    {
        bool foldout = true;

        SerializedProperty fitting_prop, 
            fontSizerList_prop, fontSizerFallback_prop,
            minFontSizerList_prop, minFontSizerFallback_prop,
            maxFontSizerList_prop, maxFontSizerFallback_prop,
            marginSizerList_prop, marginSizerFallback_prop;

        void Init()
        {
            if (fitting_prop != null)
                return;

            fitting_prop = serializedObject.FindProperty("fitting");

            fontSizerFallback_prop = serializedObject.FindProperty("fontSizerFallback");
            minFontSizerFallback_prop = serializedObject.FindProperty("minFontSizerFallback");
            maxFontSizerFallback_prop = serializedObject.FindProperty("maxFontSizerFallback");
            marginSizerFallback_prop = serializedObject.FindProperty("marginSizerFallback");

            fontSizerList_prop = serializedObject.FindProperty("customFontSizers");
            minFontSizerList_prop = serializedObject.FindProperty("customMinFontSizers");
            maxFontSizerList_prop = serializedObject.FindProperty("customMaxFontSizers");
            marginSizerList_prop = serializedObject.FindProperty("customMarginSizers");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            var origFontStyle = EditorStyles.foldout.fontStyle;
            EditorStyles.foldout.fontStyle = FontStyle.Bold;

            foldout = EditorGUILayout.Foldout(foldout, new GUIContent("Better UI"));

            EditorStyles.foldout.fontStyle = origFontStyle;

            Init();

            if (foldout)
            {
                EditorGUI.indentLevel++;

                // fitting mode
                EditorGUILayout.PropertyField(fitting_prop);

                if (fitting_prop.intValue != (int)BetterText.FittingMode.BestFit)
                {
                    // font size
                    ScreenConfigConnectionHelper.DrawSizerGui("Font Size", fontSizerList_prop, ref fontSizerFallback_prop);
                }

                BetterText.FittingMode fitting = (BetterText.FittingMode)fitting_prop.intValue;
                if (fitting != BetterText.FittingMode.SizerOnly)
                {
                    // min font size
                    ScreenConfigConnectionHelper.DrawSizerGui("Min Size", minFontSizerList_prop, ref minFontSizerFallback_prop);

                    if (fitting == BetterText.FittingMode.BestFit)
                    {
                        // max font size
                        ScreenConfigConnectionHelper.DrawSizerGui("Max Size", maxFontSizerList_prop, ref maxFontSizerFallback_prop);
                    }
                }

                EditorGUILayout.Space();

                // Margin
                ScreenConfigConnectionHelper.DrawSizerGui("Margin", marginSizerList_prop, ref marginSizerFallback_prop);


                serializedObject.ApplyModifiedProperties();

                EditorGUI.indentLevel--;
            }


            EditorGUILayout.HelpBox("Better UI properties will control some properties below.\nSo, changes below may instantly reset.", 
                MessageType.Info);
            base.OnInspectorGUI();
        }

        [MenuItem("CONTEXT/TextMeshProUGUI/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            TextMeshProUGUI ctrl = command.context as TextMeshProUGUI;

            float fontSize = ctrl.fontSize;
            float fontSizeMin = ctrl.fontSizeMin;
            float fontSizeMax = ctrl.fontSizeMax;
            Vector4 margin = ctrl.margin;

            Vector2 rectSize = ctrl.rectTransform.sizeDelta;
            Vector2 rectPos = ctrl.rectTransform.anchoredPosition;

            var newCtrl = Betterizer.MakeBetter<TextMeshProUGUI, BetterTextMeshProUGUI>(ctrl,
                "m_mesh", "m_subTextObjects", "m_fontSharedMaterials", "m_fontMaterials", "m_textInfo");
            var better = newCtrl as BetterTextMeshProUGUI;

            if (newCtrl != null)
            {
                newCtrl.SetCharArray(newCtrl.text.ToCharArray());

                if (better != null)
                {
                    better.FontSizer.SetSize(newCtrl, fontSize);
                    better.MinFontSizer.SetSize(newCtrl, fontSizeMin);
                    better.MaxFontSizer.SetSize(newCtrl, fontSizeMax);
                    better.MarginSizer.SetSize(newCtrl, new Margin(margin));

                    better.Fitting = BetterText.FittingMode.StayInBounds;
                }

                newCtrl.ClearMesh();

                newCtrl.rectTransform.sizeDelta = rectSize;
                newCtrl.rectTransform.anchoredPosition = rectPos;
            }
        }
    }
}
