using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheraBytes.BetterUi;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class ToolsPage : WizardPage
    {
        public override string NameId { get { return "ToolsPage"; } }

        public ToolsPage(IWizard wizard)
            : base(wizard)
        {
        }

        protected override void OnInitialize()
        {
            Add(new SeparatorWizardPageElement());
            Add(new InfoWizardPageElement("Tools", InfoType.Header));
            Add(new InfoWizardPageElement("You can open the tools via the buttons below. However, you can also find them in the menu 'Tools -> Better UI'.", InfoType.InfoBox));

            Add(new SeparatorWizardPageElement());
            Add(new InfoWizardPageElement("Snap Anchors", InfoType.Header));
            Add(new InfoWithButtonWizardPageElement("Snap Anchors allows you to move the anchors of selected UI elements to the desired position with one click. " +
                "Get rid of pixel positions and sizes and start working with relative sizes!",
                new InfoWithButtonWizardPageElement.ButtonInfo("Open", SnapAnchorsWindow.ShowWindow),
                new InfoWithButtonWizardPageElement.ButtonInfo("Documentation", () => Application.OpenURL("https://documentation.therabytes.de/better-ui/SnapAnchors.html"))));
            
            Add(new SeparatorWizardPageElement());

            Add(new InfoWizardPageElement("Align & Distribute", InfoType.Header));
            Add(new InfoWithButtonWizardPageElement("Do sou want to align multiple object? Or do you want to distribute predefined objects evenly without using a Layout Group? " +
                "The Align & Distribute Tool will allow this along with a lot of configuration options to you.",
                new InfoWithButtonWizardPageElement.ButtonInfo("Open", AlignDistribute.AlignDistributeWindow.ShowWindow),
                new InfoWithButtonWizardPageElement.ButtonInfo("Documentation", () => Application.OpenURL("https://documentation.therabytes.de/better-ui/AlignDistribute.html"))));

            Add(new SeparatorWizardPageElement());

            Add(new InfoWizardPageElement("Smart Parent", InfoType.Header));
            Add(new InfoWithButtonWizardPageElement("Do you need to move or resize an object but keep the screen positions of its childs? " +
                "Do you want to snap the size of the parent to the extents of its children? This tool makes it eaily possible!",
                new InfoWithButtonWizardPageElement.ButtonInfo("Open", SmartParentWindow.ShowWindow),
                new InfoWithButtonWizardPageElement.ButtonInfo("Documentation", () => Application.OpenURL("https://documentation.therabytes.de/better-ui/")))); // TODO

            Add(new SeparatorWizardPageElement());

            Add(new InfoWizardPageElement("Pick Resolution", InfoType.Header));
            Add(new InfoWithButtonWizardPageElement("Do you want to quickly switch between resolutions without having to open the drop down in the game view all the time? " +
                "This tool will help you and also allows to filter resolutions and change the tool window to integrate best in your layout. " +
                "You can also switch between / simulate your Screen configurations with it.",
                new InfoWithButtonWizardPageElement.ButtonInfo("Open", ResolutionPicker.ShowWindow),
                new InfoWithButtonWizardPageElement.ButtonInfo("Documentation", () => Application.OpenURL("https://documentation.therabytes.de/better-ui/PickResolution.html"))));
        }

        protected override void BeforeGui()
        {
            InfoWithButtonWizardPageElement.ButtonWidth = 160;
        }
    }
}
