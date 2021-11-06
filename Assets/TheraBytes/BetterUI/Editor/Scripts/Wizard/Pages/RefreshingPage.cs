using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheraBytes.BetterUi;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class RefreshingPage : WizardPage
    {
        public override string NameId { get { return "RefreshingPage"; } }
        protected override string NextButtonText { get { return "..."; } }

        public RefreshingPage(IWizard wizard)
            : base(wizard)
        {
        }

        protected override void OnInitialize()
        {
            Add(new InfoWizardPageElement("Please Wait ...", InfoType.Header));
            Add(new InfoWizardPageElement(new GUIContent(Resources.Load<Texture2D>("wizard_banner"))));
            Add(new InfoWizardPageElement("If the wizard disappers after recompiling, select:"));
            Add(new InfoWizardPageElement("     Tools -> Better UI -> Settings -> Setup Wizard", InfoType.Header));
            Add(new CustomWizardPageElement((o) => { })); // disable "Next" button
        }
        protected override void AfterGui()
        {
            // no page info
        }
    }
}
