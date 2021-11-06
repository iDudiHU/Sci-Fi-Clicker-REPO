using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public enum InstallSelectionState
    {
        None,
        Install,
        Remove,
    }

    public class InstallPackageSelectionWizardPageElement : WizardPageElementBase
    {
        string title;
        string pathToPackage;
        string pathToFolder;
        InstallSelectionState selectionState;

        public string PathToPackage { get { return pathToPackage; } }
        public string PathToFolder { get { return pathToFolder; } }
        public InstallSelectionState SelectionState { get { return selectionState; } }

        public InstallPackageSelectionWizardPageElement(string title, string pathToPackage, string pathToFolder)
        {
            this.title = title;
            this.pathToPackage = pathToPackage;
            this.pathToFolder = pathToFolder;
        }

        public override void DrawGui()
        {
            bool isInstalled = Directory.Exists(pathToFolder);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (isInstalled)
            {
                EditorGUILayout.HelpBox("✓ installed", MessageType.None);
            }


            if(isInstalled)
            {
                bool remove = GUILayout.Toggle(selectionState == InstallSelectionState.Remove, "Remove", EditorStyles.miniButton, GUILayout.Width(100));
                selectionState = (remove) ? InstallSelectionState.Remove : InstallSelectionState.None;
            }
            else
            {
                bool install = GUILayout.Toggle(selectionState == InstallSelectionState.Install, "Install", EditorStyles.miniButton, GUILayout.Width(100));
                selectionState = (install) ? InstallSelectionState.Install : InstallSelectionState.None;
            }
            EditorGUILayout.EndHorizontal();

        }
    }
}
