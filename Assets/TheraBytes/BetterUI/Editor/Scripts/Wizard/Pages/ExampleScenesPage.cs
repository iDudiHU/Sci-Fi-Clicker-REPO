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
    public class ExampleScenesPage : WizardPage
    {

        const string OVERVIEW_PACK = "TheraBytes/BetterUI/packages/Example_Overiew.unitypackage";

        InstallPackageSelectionWizardPageElement customScreenTagExample;

        public override string NameId { get { return "ExampleScenesPage"; } }

        protected override string NextButtonText
        {
            get
            {
                var packages = base.elements.OfType<InstallPackageSelectionWizardPageElement>();
                bool installAny = packages.Any(o => o.SelectionState == InstallSelectionState.Install);
                bool removeAny = packages.Any(o => o.SelectionState == InstallSelectionState.Remove);

                if(installAny && removeAny)
                {
                    return "Install / Remove selected Example Scenes";
                }
                else if(installAny)
                {
                    return "Install selected Example Scenes";
                }
                else if(removeAny)
                {
                    return "Remove selected Example Scenes";
                }

                return base.NextButtonText;
            }
        }

        public ExampleScenesPage(IWizard wizard)
            : base(wizard)
        {
        }

        protected override void OnInitialize()
        {
            Add(new InfoWizardPageElement("Example Scenes", InfoType.Header));
            Add(new InfoWizardPageElement("This page allows you to install example scenes.", InfoType.InfoBox));


            // Overview
            Add(new SeparatorWizardPageElement());
            Add(CreateSelection("Overview"));
            Add(new InfoWizardPageElement("This is the oldest example scene of Better UI. It shows the most releant features and is devided into categories: " +
                "Transitions, Better Image, Better Text and Layout"));
            if(IsFallbackLandscape())
            {
                Add(new InfoWizardPageElement("This Example scene was created for Portrait resolutions as fallback. " +
                    "Your fallback is Landscape, so the example may look broken.", InfoType.ErrorBox));
            }

            // Ghost
            Add(new SeparatorWizardPageElement());
            Add(CreateSelection("Ghost"));
            Add(new InfoWizardPageElement("This example shows some animation features as well as a movable mask where the masked sprite in the hierarchy below stays at a certain position. " +
                "This is achieved using Anchor Overrides."));


            // ResolutionTracking
            Add(new SeparatorWizardPageElement());
            Add(CreateSelection("ResolutionTracking"));
            Add(new InfoWithButtonWizardPageElement("This example shows realtime detection of screen sizes and portrait / landscape detection. " +
                "You may preview this example in your browser.", 
                "Preview", () => Application.OpenURL("https://documentation.therabytes.de/better-ui-webgl-dpi-test/container.html")));
            if (IsFallbackPortrait())
            {
                Add(new InfoWizardPageElement("This Example scene was created for Landscape resolutions as fallback. " +
                    "Your fallback is Portrait, so the example may not work as intended.", InfoType.ErrorBox));
            }
            else if(!ResolutionMonitor.Instance.OptimizedScreens.Any(o => o.Name == "Portrait"))
            {
                Add(new InfoWizardPageElement("For this example to work properly, it is required to have a screen configuration named 'Portrait'.", InfoType.WarningBox));
            }

            // AnchorAnimations
            Add(new SeparatorWizardPageElement());
            Add(CreateSelection("AnchorAnimations"));
            Add(new InfoWizardPageElement("This example shows how you can use AnchorOverrides to create animated UIs. " +
                "The example contains a visually resorting leaderboard as well as an object which can transform into other objects per click."));
            if (IsFallbackLandscape())
            {
                Add(new InfoWizardPageElement("This Example scene was created for Portrait resolutions as fallback. " +
                    "Your fallback is Landscape, so the example may look broken.", InfoType.WarningBox));
            }

            // CustomScreenTag
            Add(new SeparatorWizardPageElement());
            customScreenTagExample = CreateSelection("CustomScreenTag");
            Add(customScreenTagExample);
            Add(new InfoWizardPageElement("You will find just a long text and a toggle in this exampke. If you hit the toggle everything becomes big. " +
                "This is achieved through custom screen tags in screen configurations."));
            Add(new InfoWizardPageElement("If you install this example, another screen configuration 'Accessibility Mode' is added to the Resolution Monitor. " +
                "Note that it will not be removed again if you remove this example. You may want to remove it manually later.", InfoType.WarningBox));

        }

        protected override void NextButtonClicked()
        {
            var items = elements.OfType<InstallPackageSelectionWizardPageElement>();

            if (items.All(o => o.SelectionState == InstallSelectionState.None))
            {
                base.NextButtonClicked();
                return;
            }

            wizard.DoReloadOperation(this, () =>
            {
                // add accessibility mode if custom screen tag example is installed
                if (customScreenTagExample.SelectionState == InstallSelectionState.Install
                    && !ResolutionMonitor.Instance.OptimizedScreens.Any(x => x.Name == "Accessibility Mode"))
                {
                    var screenCondition = new ScreenTypeConditions("Accessibility Mode");
                    screenCondition.CheckScreenTag.IsActive = true;
                    screenCondition.CheckScreenTag.ScreenTag = "Accessibility_Mode";

                    ResolutionMonitor.Instance.OptimizedScreens.Add(screenCondition);
                    EditorUtility.SetDirty(ResolutionMonitor.Instance);
                    AssetDatabase.SaveAssets();
                }

                foreach (var itm in items)
                {
                    switch (itm.SelectionState)
                    {
                        case InstallSelectionState.Install:
                            try
                            {
                                AssetDatabase.ImportPackage(itm.PathToPackage, false);
                            }
                            catch
                            {
                                Debug.LogWarningFormat("Could not import package '{0}'", itm.PathToPackage);
                            }
                            break;
                        case InstallSelectionState.Remove:
                            try
                            {
                                File.Delete(itm.PathToFolder + ".meta");
                                Directory.Delete(itm.PathToFolder, true);
                            }
                            catch
                            {
                                Debug.LogWarningFormat("Could not properly delete '{0}'", itm.PathToFolder);
                            }
                            break;
                    }
                }

                AssetDatabase.Refresh();
            });
        }

        InstallPackageSelectionWizardPageElement CreateSelection(string exampleName)
        {
            string package = Path.Combine(Application.dataPath, string.Format("TheraBytes/BetterUI/packages/Example_{0}.unitypackage", exampleName));
            string folder = Path.Combine(Application.dataPath, string.Format("TheraBytes/BetterUI/Example/{0}", exampleName));

            var result = new InstallPackageSelectionWizardPageElement(exampleName, package, folder);
            result.MarkComplete();

            return result;
        }
        
        bool IsFallbackLandscape()
        {
            return !IsFallbackPortrait();
        }

        bool IsFallbackPortrait()
        {
            var fallback = ResolutionMonitor.OptimizedResolutionFallback;
            return fallback.x < fallback.y;
        }
    }
}
