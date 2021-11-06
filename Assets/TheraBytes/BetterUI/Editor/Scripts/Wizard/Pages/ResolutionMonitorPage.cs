using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class ResolutionMonitorPage : WizardPage
    {
        const int SMALL_SCREEN = 1 << 0;
        const int BIG_SCREEN = 1 << 1;

        ValueWizardPageElement<Vector2> optimizedResolutionElement;
        ValueWizardPageElement<bool> isFallbackLandscapeElement;
        ValueWizardPageElement<bool> supportBothOrientationsElement;
        ValueWizardPageElement<int> responsiveDesignElement;

        public override string NameId { get { return "ResolutionMonitorPage"; } }

        public ResolutionMonitorPage(IWizard wizard)
            : base(wizard)
        {
        }

        protected override void OnInitialize()
        {
            Add(new InfoWizardPageElement("Resolution Monitor Setup", InfoType.Header));
            Add(new InfoWizardPageElement(
                "The Resolution Monitor is responsible for applying the correct sizes to your Better UI elements as well as detecting screen configurations.\n" +
                "This wizard page will help you setting it up correctly",
                InfoType.InfoBox));

            // optimized resolution
            Add(new SeparatorWizardPageElement());
            Add(new InfoWizardPageElement("♠ Which is the resolution your game is optimized for?"));
            Add(new InfoWizardPageElement("The 'Optimized Resolution' is used as reference for resizing UI variables (like font size).\n" +
                "If the artists also create their UI graphics for that resolution it will be easier to make the UI looking right.",
                InfoType.InfoBox));

            optimizedResolutionElement = new ValueWizardPageElement<Vector2>("optimizedResolution",
                (o, v) =>
                {
                    if (v == Vector2.zero)
                    {
                        v = new Vector2(1920, 1080);
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Optimized Resolution");
                    v = EditorGUILayout.Vector2Field("", v);
                    if (GUILayout.Button("Full HD")) v = new Vector2(1920, 1080);
                    if (GUILayout.Button("UHD-4k")) v = new Vector2(3840, 2160);
                    EditorGUILayout.EndHorizontal();

                    DrawContinueButton(o);
                    return v;
                });

            Add(optimizedResolutionElement);

            // main orientation
            Add(new SeparatorWizardPageElement());
            Add(new InfoWizardPageElement("♠ Which is the orientation your game is played (or played mainly)?"));
            isFallbackLandscapeElement = new ValueWizardPageElement<bool>("isFallbackLandscape",
                (o, v) =>
                {
                    EditorGUILayout.BeginHorizontal();
                    v = GUILayout.Toggle(v, "Landscape", EditorStyles.miniButtonLeft);
                    v = !GUILayout.Toggle(!v, "Portrait", EditorStyles.miniButtonRight);
                    EditorGUILayout.EndHorizontal();

                    DrawContinueButton(o);
                    return v;
                });
            Add(isFallbackLandscapeElement);

            // both orientations
            Add(new SeparatorWizardPageElement());
            Add(new InfoWizardPageElement("♠ Do you support both, Landscape and Portrait orientation?"));
            supportBothOrientationsElement = new ValueWizardPageElement<bool>("supportBothOrientations",
                (o, v) =>
                {
                    EditorGUILayout.BeginHorizontal();
                    v = GUILayout.Toggle(v, "Yes", EditorStyles.miniButtonLeft);
                    v = !GUILayout.Toggle(!v, "No", EditorStyles.miniButtonRight);
                    EditorGUILayout.EndHorizontal();

                    DrawContinueButton(o);
                    return v;
                });
            Add(supportBothOrientationsElement);

            // Responsive Design
            Add(new SeparatorWizardPageElement());
            Add(new InfoWizardPageElement("♠ Do you support Responsive Design for different screen sizes?"));
            Add(new InfoWizardPageElement("With responsive design you can place certain elements at different locations or hide them " +
                "depending on the screen size / orientation (or other parameters).\n" +
                "You can also configure other variables differently for different screens (e.g. controlling the sizes of layout parameters differently)",
                InfoType.InfoBox));

            responsiveDesignElement = new ValueWizardPageElement<int>("responsiveDesign",
                (o, v) =>
                {
                    bool containsSmall = (v & SMALL_SCREEN) == SMALL_SCREEN;
                    bool containsBig = (v & BIG_SCREEN) == BIG_SCREEN;

                    EditorGUILayout.LabelField("In addition to the default screen size, support differnt layouts for ... (multi-select allowed)");
                    EditorGUILayout.BeginHorizontal();
                    containsSmall = GUILayout.Toggle(containsSmall, "Smaller Screens", EditorStyles.miniButtonLeft);
                    containsBig = GUILayout.Toggle(containsBig, "Larger Screens", EditorStyles.miniButtonRight);
                    EditorGUILayout.EndHorizontal();

                    v = (containsSmall ? SMALL_SCREEN : 0) | (containsBig ? BIG_SCREEN : 0);

                    DrawContinueButton(o);
                    return v;
                });
            Add(responsiveDesignElement);


            // Generate!
            Add(new SeparatorWizardPageElement());
            Add(new InfoWizardPageElement("Generate Resolution Monitor", InfoType.Header));
            Add(new InfoWizardPageElement("When you feel comfortable with the settings you made, go on and click the button below."));
            var generateElement = new CustomWizardPageElement((o) =>
            {
                string suffix = (ResolutionMonitor.ScriptableObjectFileExists) ? " (overwrite existing)" : "";
                if (GUILayout.Button("Generate Resolution Monitor!" + suffix, GUILayout.Height(50)))
                {
                    if (!ResolutionMonitor.ScriptableObjectFileExists
                        || EditorUtility.DisplayDialog("Are you sure?", 
                            "Overwriting the current Resolution Monitor settings could break your UI! (some screen configuration references may get lost)", 
                            "Overwrite!", "Cancel"))
                    {
                        GenerateResolutionMonitor();
                        o.MarkComplete();
                    } 
                }

            });
            Add(generateElement);

            if(ResolutionMonitor.ScriptableObjectFileExists)
            {
                generateElement.MarkComplete();
            }
            else
            {
                // display only if resolution monitor was not present before.
                Add(new InfoWizardPageElement("Resolution Monitor has been generated."));
            }


            // Extra info
            Add(new SeparatorWizardPageElement());
            Add(new CustomWizardPageElement((o) =>
            {
                EditorGUILayout.HelpBox("You have configured your Resolution Monitor. " +
                    "You can even add more configuratons and also trigger your own layouts via code. " +
                    "This would require manual configuration of the Resolution Monitor and some deeper knowledge of the system.", MessageType.Info);
                
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Open Documentation"))
                {
                    Application.OpenURL("https://documentation.therabytes.de/better-ui/ScreenConfigurations.html");
                }

                if (GUILayout.Button("Edit Resolution Monitor"))
                {
                    Selection.activeObject = ResolutionMonitor.Instance;
                }

                EditorGUILayout.EndHorizontal();

            }).MarkComplete());
        }

        private void GenerateResolutionMonitor()
        {
            ResolutionMonitor.EnsureInstance();
            var rm = ResolutionMonitor.Instance;
            Vector2 lRes = optimizedResolutionElement.Value;
            if(lRes.x < lRes.y)
            {
                lRes = new Vector2(lRes.y, lRes.x);
            }

            Vector2 pRes = new Vector2(lRes.y, lRes.x);

            // fallback
            bool isLandscape = isFallbackLandscapeElement.Value;
            if (isLandscape)
            {
                rm.FallbackName = "Landscape";
                rm.SetOptimizedResolutionFallback(lRes);
            }
            else
            {
                rm.FallbackName = "Portrait";
                rm.SetOptimizedResolutionFallback(pRes);
            }

            rm.OptimizedScreens.Clear();

            // both orientations
            ScreenTypeConditions alt = null;
            bool supportBoth = supportBothOrientationsElement.Value;
            if (supportBoth)
            {
                alt = CreateScreenTypeCondition(!isLandscape, "", lRes, pRes);
                // added later
            }

            // Responsive Design
            bool containsSmall = (responsiveDesignElement.Value & SMALL_SCREEN) == SMALL_SCREEN;
            bool containsBig = (responsiveDesignElement.Value & BIG_SCREEN) == BIG_SCREEN;

            var smallChecker = new IsScreenOfCertainSize(0, IsScreenOfCertainSize.DEFAULT_SMALL_THRESHOLD);
            var bigChecker = new IsScreenOfCertainSize(IsScreenOfCertainSize.DEFAULT_LARGE_THRESHOLD, 9999999);


            ScreenTypeConditions smallMain = null;
            ScreenTypeConditions smallAlt = null;
            ScreenTypeConditions bigMain = null;
            ScreenTypeConditions bigAlt = null;
            if (supportBoth)
            {
                if (containsSmall)
                {
                    smallAlt = CreateScreenTypeCondition(!isLandscape, " Small", lRes, pRes, smallChecker);

                    smallAlt.Fallbacks.Add(alt.Name);
                    alt.Fallbacks.Add(smallAlt.Name);

                    rm.OptimizedScreens.Add(smallAlt);
                }

                if(containsBig)
                {
                    bigAlt = CreateScreenTypeCondition(!isLandscape, " Large", lRes, pRes, bigChecker);

                    bigAlt.Fallbacks.Add(alt.Name);
                    alt.Fallbacks.Add(bigAlt.Name);

                    if (containsSmall)
                    {
                        bigAlt.Fallbacks.Add(smallAlt.Name);
                        smallAlt.Fallbacks.Add(bigAlt.Name);
                    }

                    rm.OptimizedScreens.Add(bigAlt);
                }

                
                rm.OptimizedScreens.Add(alt);
            }

            if (containsSmall)
            {
                smallMain = CreateScreenTypeCondition(isLandscape, " Small", lRes, pRes, smallChecker);
                rm.OptimizedScreens.Add(smallMain);

                if(smallAlt != null)
                {
                    smallAlt.Fallbacks.Add(smallMain.Name);
                }
            }

            if (containsBig)
            {
                bigMain = CreateScreenTypeCondition(isLandscape, " Large", lRes, pRes, bigChecker);
                rm.OptimizedScreens.Add(bigMain);

                if(bigAlt != null)
                {
                    bigAlt.Fallbacks.Add(bigMain.Name);
                }
            }

            Debug.Log("Resolution Monitor has been generated successfully.");
        }

        private ScreenTypeConditions CreateScreenTypeCondition(bool isLandscape, string nameSuffix, Vector2 lRes, Vector2 pRes,
            IsScreenOfCertainSize sizeInfo = null)
        {
            string name = (isLandscape)
                ? "Landscape" : "Portrait";

            name += nameSuffix;

            var sct = new ScreenTypeConditions(name, typeof(IsCertainScreenOrientation));

            sct.OptimizedScreenInfo.Resolution = (isLandscape)
                ? lRes : pRes;

            sct.CheckOrientation.ExpectedOrientation = (isLandscape)
                ? IsCertainScreenOrientation.Orientation.Landscape
                : IsCertainScreenOrientation.Orientation.Portrait;

            sct.CheckScreenSize.IsActive = sizeInfo != null;
            if(sizeInfo != null)
            {
                sct.CheckScreenSize.Units = sizeInfo.Units;
                sct.CheckScreenSize.MeasureType = sizeInfo.MeasureType;
                sct.CheckScreenSize.MinSize = sizeInfo.MinSize;
                sct.CheckScreenSize.MaxSize = sizeInfo.MaxSize;
            }

            return sct;
        }

        private static void DrawContinueButton(WizardPageElementBase o)
        {
            if (o.State == WizardElementState.WaitForInput && GUILayout.Button("Continue..."))
            {
                o.MarkComplete();
            }
        }
    }
}