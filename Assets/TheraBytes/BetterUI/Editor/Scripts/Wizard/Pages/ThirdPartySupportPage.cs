using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class ThirdPartySupportPage : WizardPage
    {
        const string PACKAGE_PATH_TMP_OLD = "TheraBytes/BetterUI/packages/BetterUI_TextMeshPro_UiEditorPanel.unitypackage";
        const string PACKAGE_PATH_TMP_NEW = "TheraBytes/BetterUI/packages/BetterUI_TextMeshPro_EditorPanelUI.unitypackage";
        private const string TMP_KEY = "TextMeshPro";
        public override string NameId { get { return "ThirdPartySupportPage"; } }

        public ThirdPartySupportPage(IWizard wizard)
            : base(wizard)
        {
        }

        protected override void OnInitialize()
        {
            Add(new InfoWizardPageElement("Additional Packages", InfoType.Header));

            // TextMesh Pro
            Add(new SeparatorWizardPageElement());
            Add(new InfoWizardPageElement("TextMeth Pro", InfoType.Header));

            string tmpAddOnPath = Path.Combine(Application.dataPath, "TheraBytes/BetterUI_TextMeshPro");
            if (Directory.Exists(tmpAddOnPath))
            {
                Add(new InfoWizardPageElement("TextMesh Pro add-on is already installed."));

                Add(new ValueWizardPageElement<string>(TMP_KEY,
                    (o, v) =>
                    {
                        if(GUILayout.Button("Remove TextMesh Pro add-on"))
                        {
                            if(EditorUtility.DisplayDialog("Remove TextMesh Pro add-on?",
                                "Are you sure you want to remove Better UI support for TextMesh Pro? (some UIs could break)",
                                "Remove it!", "Cancel"))
                            {
                                wizard.DoReloadOperation(this, () =>
                                {
                                    try
                                    {
                                        File.Delete(tmpAddOnPath + ".meta");
                                        Directory.Delete(tmpAddOnPath, true);
                                    }
                                    catch
                                    {
                                        Debug.LogError("Could not properly delete " + tmpAddOnPath);
                                    }
                                    AssetDatabase.Refresh();
                                    v = null;
                                });
                            }
                        }

                        return v;
                    }).MarkComplete());
            }
            else
            {
                int v1, v2, v3;
                bool foundTmpVersion = TryGetTextMethProPackageVersion(out v1, out v2, out v3);
                if (foundTmpVersion)
                {
                    string versionString = string.Format("{0}.{1}.{2}", v1, v2, v3);
                    Add(new InfoWizardPageElement(string.Format("TextMesh pro version {0} detected.", versionString)));
                    if (v1 > 3 || (v1 == 3 && v2 > 0) || (v1 == 3 && v2 == 0 && v3 > 6))
                    {
                        Add(new InfoWizardPageElement("The latest tested TextMesh Pro version is 3.0.6.\n" +
                            "It is very likely that the Better UI add-on for TextMesh Pro will also work with version " + versionString +
                            " but it cannot be guaranteed. If it doesnt work with this version, please write a mail to info@therabytes.de.",
                            InfoType.WarningBox));
                    }

                    if (v1 < 1 || (v1 == 1 && v2 == 0 && v3 < 54))
                    {
                        Add(new InfoWizardPageElement("Your version of TextMesh Pro is too old and not supported. Please upgrade at least to version 1.0.54.",
                            InfoType.ErrorBox));
                    }
                    else
                    {
                        const string assertionText = "You have a TextMesh Pro package installed which is not supported in your version of Unity. You should upgrade TextMesh Pro.";
                        bool isNewBaseClass = 
#if UNITY_2020_1_OR_NEWER
                            (v1 >= 3); // should always be the case
                            Debug.Assert(v1 >= 3, assertionText);
#elif UNITY_2019_4_OR_NEWER
                            (v1 == 2 && v2 >= 1) || (v1 > 2);
                            Debug.Assert(v1 == 2, assertionText);
#else
                            (v1 == 1 && v2 >= 5) || (v1 > 1);
                            Debug.Assert(v1 == 1, assertionText);
#endif

                        string packageName = (isNewBaseClass)
                            ? PACKAGE_PATH_TMP_NEW
                            : PACKAGE_PATH_TMP_OLD;

                        Add(new ValueWizardPageElement<string>(TMP_KEY,
                            (o, v) =>
                            {
                                if (GUILayout.Button("Import TextMesh Pro Add-On"))
                                {
                                    wizard.DoReloadOperation(this, () =>
                                    {
                                        AssetDatabase.ImportPackage(Path.Combine(Application.dataPath, packageName), false);
                                        v = packageName;
                                    });
                                }
                                return v;
                            }).MarkComplete());
                    }
                }
                else
                {
                    Add(new InfoWizardPageElement("TextMesh Pro could not be detected."));
                    Add(new InfoWizardPageElement("You may install the right Better UI add-on now " +
                        "but if you don't have TextMesh Pro installed in your project, " +
                        "you will face compile errors until TextMesh Pro is installed or the add on folder is deleted again.",
                        InfoType.WarningBox));


                    Add(new InfoWizardPageElement("Please select the add on for the text mesh pro version you have installed."));

#if UNITY_2020_1_OR_NEWER
                    string textNewVersion = "v3.0 or above";
                    string textOldVersion = "below v3.0";
#elif UNITY_2019_4_OR_NEWER
                    string textNewVersion = "v2.1 or above";
                    string textOldVersion = "below v2.1";
#else
                    string textNewVersion = "v1.5 or above";
                    string textOldVersion = "below v1.5 and above v1.0.54";
#endif
                    Add(new ValueWizardPageElement<string>(TMP_KEY,
                           (o, v) =>
                           {
                               if (GUILayout.Button("Import Add-On (TextMesh Pro " + textNewVersion + ")"))
                               {
                                   wizard.DoReloadOperation(this, () =>
                                    {
                                        AssetDatabase.ImportPackage(Path.Combine(Application.dataPath, PACKAGE_PATH_TMP_NEW), false);
                                        v = PACKAGE_PATH_TMP_NEW;
                                    });
                               }

                               if (GUILayout.Button("Import Add-On (TextMesh Pro " + textOldVersion + ")"))
                               {
                                   wizard.DoReloadOperation(this, () =>
                                   {
                                       AssetDatabase.ImportPackage(Path.Combine(Application.dataPath, PACKAGE_PATH_TMP_OLD), false);
                                       v = PACKAGE_PATH_TMP_OLD;
                                   });
                               }

                               return v;
                           }).MarkComplete());
                }
            }
        }

        private bool TryGetTextMethProPackageVersion(out int v1, out int v2, out int v3)
        {
            v1 = 0;
            v2 = 0;
            v3 = 0;

            string folder = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Packages");
            if (!Directory.Exists(folder))
                return false;

            string file = Path.Combine(folder, "manifest.json");
            if (!File.Exists(file))
                return false;

            string json = File.ReadAllText(file);
            var match = Regex.Match(json, @"""com.unity.textmeshpro"":\s?""(?<v1>\d *).(?<v2>\d *).(?<v3>\d *)""");
            if (!match.Success)
                return false;

            try
            {
                return int.TryParse(match.Groups["v1"].Value, out v1)
                    && int.TryParse(match.Groups["v2"].Value, out v2)
                    && int.TryParse(match.Groups["v3"].Value, out v3);
            }
            catch
            {
                return false;
            }
        }
    }
}
