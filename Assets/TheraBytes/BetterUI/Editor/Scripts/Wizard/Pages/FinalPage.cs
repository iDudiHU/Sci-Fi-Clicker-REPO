using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheraBytes.BetterUi;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class FinalPage : WizardPage
    {
        public override string NameId { get { return "FinalPage"; } }
        protected override string NextButtonText { get { return "Close"; } }

        public FinalPage(IWizard wizard)
            : base(wizard)
        {
        }

        protected override void OnInitialize()
        {
            var data = wizard.PersistentData;

            Add(new InfoWizardPageElement("Setup Complete!", InfoType.Header));
            Add(new InfoWizardPageElement("Thank you for choosing Better UI."));

            Add(new SeparatorWizardPageElement());
            Add(new InfoWizardPageElement("What to do next ...?", InfoType.Header));

            Add(new SeparatorWizardPageElement());
            Add(new InfoWithButtonWizardPageElement("You may want to check out the documentation.",
                "Open Documentation", () => Application.OpenURL("https://documentation.therabytes.de/better-ui/")));

            Add(new SeparatorWizardPageElement());
            Add(new InfoWithButtonWizardPageElement("If you have any trouble with Better UI, found a bug or have a feature request, feel free to write into the forums thread",
                "Open Thread in Unity Forum", () => Application.OpenURL("https://forum.unity.com/threads/better-ui.453808/")));

            Add(new InfoWithButtonWizardPageElement("Alternatively, you can send us a mail.",
                "Send Mail", () => Application.OpenURL("mailto:info@therabytes.de")));

            Add(new SeparatorWizardPageElement());
            Add(new InfoWithButtonWizardPageElement("If you want to watch some explanatory videos, visit the Asset Store page.\n" +
                "If you find Better UI useful you may consider to leave us a 5-Star review. We would be very thankful :)",
                "Open Asset Store Page", () => Application.OpenURL("https://assetstore.unity.com/packages/tools/gui/better-ui-79031")));

        }

        protected override void BeforeGui()
        {
            InfoWithButtonWizardPageElement.ButtonWidth = 200;
        }
    }
}
