using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheraBytes.BetterUi;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class WelcomePage : WizardPage
    {
        public override string NameId { get { return "WelcomePage"; } }
        protected override string NextButtonText { get { return "Start Setup Wizard"; } }

        public WelcomePage(IWizard wizard)
            : base(wizard)
        {
        }

        protected override void OnInitialize()
        {
            var data = wizard.PersistentData;

            Add(new InfoWizardPageElement(new GUIContent(Resources.Load<Texture2D>("wizard_banner"))));
            Add(new InfoWizardPageElement("Welcome", InfoType.Header));
            Add(new InfoWizardPageElement("Welcome to the setup wizard of Better UI!\n" +
                "It will guide you configuring Better UI properly for your project."));

            bool skippingIsRecommended = false;
            if (data.FileExists() && data.SavedDataCount > 1)
            {
                Add(new InfoWizardPageElement("It seems like you did the setup wizard for this project already.\n" +
                    "You can do it again but if you change certain settings, it may lead to broken parts of your UIs.\n" +
                    "So, make sure you made a backup of the 'TheraBytes' folder before starting the wizard.",
                    InfoType.WarningBox));

                skippingIsRecommended = true;
            }
            else if (ResolutionMonitor.ScriptableObjectFileExists)
            {
                Add(new InfoWizardPageElement("It seems like you already have Better UI prepared for your project.\n" +
                    "Probably you upgraded from an older version of Better UI." +
                    "In this case it is recommended to skip the setup wizard to prevent breaking your existing UIs.\n\n" +
                    "If you decide to do the Wizard anyway, make sure you have a backup of the 'TheraBytes' folder.",
                    InfoType.WarningBox));

                skippingIsRecommended = true;
            }


            if (!skippingIsRecommended)
            {
                Add(new InfoWizardPageElement("It is recommended to follow this wizard before using any features of Better UI.\n" +
                    "If you decide to skip the wizard for now, you will not be prompted to do it again.\n\n" +
                    "However, you can start it manually:\n" +
                    "Tools -> Better UI -> Settings -> Setup Wizard",
                    InfoType.InfoBox));
            }

            if (skippingIsRecommended)
            {
                Add(new UnserializedButtonPageElement("Jump to Third-Party-Support Page",
                    () => wizard.JumpToPage<ThirdPartySupportPage>()));

#if UNITY_2017_3_OR_NEWER
                Add(new UnserializedButtonPageElement("Jump to Assembly-Definitions Page",
                    () => wizard.JumpToPage<AssemblyDefinitionsPage>()));
#endif

                Add(new UnserializedButtonPageElement("Jump to Example-Scenes Page",
                    () => wizard.JumpToPage<ExampleScenesPage>()));

                Add(new UnserializedButtonPageElement("Jump to Tools Page",
                    () => wizard.JumpToPage<ToolsPage>()));

                Add(new UnserializedButtonPageElement("Jump to Final Page",
                    () => wizard.JumpToPage<FinalPage>()));

                Add(new SeparatorWizardPageElement());
            }

            Add(new ValueWizardPageElement<bool>("SkipWizard",
                    (o, v) =>
                    {
                        if (GUILayout.Button("Skip Wizard"))
                        {
                            if (skippingIsRecommended
                            || EditorUtility.DisplayDialog("Skip Wizard", "Do you really want to skip the wizard?", "Yes", "No"))
                            {
                                EditorUtility.DisplayDialog("Wizard Skipped",
                                    "You can start the wizard later by hand:\nTools -> Better UI -> Settings -> Setup Wizard.",
                                    "Ok");

                                v = true;
                            }
                        }

                        return v;
                    },
                    valueChangedCallback: (o) =>
                    {
                        if (o.Value)
                        {
                            wizard.PersistentData.RegisterValue(o.SerializationKey, o.GetValueAsString());
                            wizard.PersistentData.Save();
                            wizard.Close();
                        }
                    })
                    .MarkComplete());

        }

    }
}
