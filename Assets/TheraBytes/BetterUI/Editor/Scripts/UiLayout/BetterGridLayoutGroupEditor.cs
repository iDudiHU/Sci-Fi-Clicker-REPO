using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterGridLayoutGroup)), CanEditMultipleObjects]
    public class BetterGridLayoutGroupGridEditor : UnityEditor.Editor
    {
        private SerializedProperty paddingFallback;
        private SerializedProperty paddingConfigs;

        private SerializedProperty cellSizeFallback;
        private SerializedProperty cellSizeConfigs;

        private SerializedProperty spacingFallback;
        private SerializedProperty spacingConfigs;

        private SerializedProperty settingsFallback;
        private SerializedProperty settingsConfigs;

        private SerializedProperty fit;

        private SerializedProperty startCorner;
        private SerializedProperty startAxis;
        private SerializedProperty childAlignment;
        private SerializedProperty constraint;
        private SerializedProperty constraintCount;

        [MenuItem("CONTEXT/GridLayoutGroup/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            GridLayoutGroup grid = command.context as GridLayoutGroup;
            var pad = new Margin(grid.padding);
            var space = grid.spacing;
            var size = grid.cellSize;

            var newGrid = Betterizer.MakeBetter<GridLayoutGroup, BetterGridLayoutGroup>(grid, "m_Padding");
            
            var betterGrid = newGrid as BetterGridLayoutGroup;
            if (betterGrid != null)
            {
                betterGrid.PaddingSizer.SetSize(newGrid, pad);
                betterGrid.SpacingSizer.SetSize(newGrid, space);
                betterGrid.CellSizer.SetSize(newGrid, size);
            }
            else
            {
                pad.CopyValuesTo(newGrid.padding);
            }

            Betterizer.Validate(newGrid);
        }

        protected virtual void OnEnable()
        {
            this.paddingFallback = base.serializedObject.FindProperty("paddingSizerFallback");
            this.paddingConfigs = base.serializedObject.FindProperty("customPaddingSizers");
            this.cellSizeFallback = base.serializedObject.FindProperty("cellSizerFallback");
            this.cellSizeConfigs = base.serializedObject.FindProperty("customCellSizers");
            this.spacingFallback = base.serializedObject.FindProperty("spacingSizerFallback");
            this.spacingConfigs = base.serializedObject.FindProperty("customSpacingSizers");
            this.settingsFallback = base.serializedObject.FindProperty("settingsFallback");
            this.settingsConfigs = base.serializedObject.FindProperty("customSettings");

            this.fit = base.serializedObject.FindProperty("fit");
            this.startCorner = base.serializedObject.FindProperty("m_StartCorner");
            this.startAxis = base.serializedObject.FindProperty("m_StartAxis");
            this.childAlignment = base.serializedObject.FindProperty("m_ChildAlignment");
            this.constraint = base.serializedObject.FindProperty("m_Constraint");
            this.constraintCount = base.serializedObject.FindProperty("m_ConstraintCount");
        }

        public override void OnInspectorGUI()
        {
            ScreenConfigConnectionHelper.DrawSizerGui("Padding", paddingConfigs, ref paddingFallback);
            ScreenConfigConnectionHelper.DrawSizerGui("Spacing", spacingConfigs, ref spacingFallback);
            ScreenConfigConnectionHelper.DrawSizerGui("Cell Size", cellSizeConfigs, ref cellSizeFallback);

            ScreenConfigConnectionHelper.DrawGui("Settings", settingsConfigs, ref settingsFallback, DrawSettings);

            serializedObject.ApplyModifiedProperties();


            //if(serializedObject.ApplyModifiedProperties())
            //{
            //    GridLayoutGroup grid = target as GridLayoutGroup;
                
            //}
        }

        private void DrawSettings(string configName, SerializedProperty prop)
        {

            fit = prop.FindPropertyRelative("Fit");
            startCorner = prop.FindPropertyRelative("StartCorner");
            startAxis = prop.FindPropertyRelative("StartAxis");
            childAlignment = prop.FindPropertyRelative("ChildAlignment");
            constraint = prop.FindPropertyRelative("Constraint");
            constraintCount = prop.FindPropertyRelative("ConstraintCount");

            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.PropertyField(startCorner);
            EditorGUILayout.PropertyField(startAxis);
            EditorGUILayout.PropertyField(childAlignment);
            EditorGUILayout.PropertyField(constraint);

            if (constraint.enumValueIndex > 0)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(constraintCount, true);
                EditorGUILayout.PropertyField(fit);

                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.EndVertical();
        }
    }
}
