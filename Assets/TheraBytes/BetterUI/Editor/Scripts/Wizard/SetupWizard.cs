using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheraBytes.BetterUi;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    [InitializeOnLoad]
    public class SetupWizard : EditorWindow, IWizard
    {
        // static >
        const string PageToLoadKey = "page_to_load_key";
        
        static PersistentWizardData spwd; // "static persistent wizard data" - don't access directly.
        static PersistentWizardData StaticPersistentWizardData
        {
            get
            {
                if(spwd == null)
                {
                    string filePath = string.Format("{0}/TheraBytes/BetterUI/Editor/setup~.wizard", Application.dataPath);
                    spwd = new PersistentWizardData(filePath);
                }

                return spwd;
            }
        }

        [MenuItem("Tools/Better UI/Settings/Setup Wizard", false, -100)]
        public static void ShowWindow()
        {
            EditorApplication.delayCall -= ShowWindow;

            var wnd = EditorWindow.GetWindow(typeof(SetupWizard), true, "Better UI - Setup Wizard");
            wnd.minSize = new Vector2(524, 460);
            wnd.maxSize = new Vector2(524, 2000);
        }

        [InitializeOnLoadMethod]
        static void Init()
        {
            if (StaticPersistentWizardData.FileExists())
                return;

            EditorApplication.delayCall += ShowWindow;
        }


        // member >


        public int CurrentPageNumber { get { return TotalPageCount - pages.Count; } }
        public int TotalPageCount { get; private set; }

        Queue<WizardPage> pages;
        public PersistentWizardData PersistentData { get { return StaticPersistentWizardData; } }

        void OnEnable()
        {
            pages = new Queue<WizardPage>();
            pages.Enqueue(new WelcomePage(this));
            pages.Enqueue(new ResolutionMonitorPage(this));
            pages.Enqueue(new ThirdPartySupportPage(this));

#if UNITY_2017_3_OR_NEWER
            // Assembly definitions were introduced in Unity 2017.3
            pages.Enqueue(new AssemblyDefinitionsPage(this));
#endif
            pages.Enqueue(new ExampleScenesPage(this));
            pages.Enqueue(new ToolsPage(this));
            pages.Enqueue(new FinalPage(this));

            TotalPageCount = pages.Count;

            string pageToLoad;
            if(PersistentData.TryGetValue(PageToLoadKey, out pageToLoad))
            {
                while(pages.Count > 0 && pages.Peek().NameId != pageToLoad)
                {
                    pages.Dequeue();
                }

                PersistentData.RemoveEntry(PageToLoadKey);
                PersistentData.Save();
            }

            pages.Peek().Initialize();

            Focus();
        }

        void OnGUI()
        {
            pages.Peek().DrawGui();
        }

        public void PageFinished(WizardPage page)
        {
            if (pages.Count > 1)
            {
                pages.Dequeue();
                pages.Peek().Initialize();
            }
            else
            {
                Close();
            }
        }

        public void JumpToPage<T>()
            where T : WizardPage
        {
            while (pages.Count > 0 && pages.Peek().GetType() != typeof(T))
            {
                pages.Dequeue();
            }

            Debug.Assert(pages.Count > 0);
            pages.Peek().Initialize();
        }

        public void DoReloadOperation(WizardPage page, Action operation)
        {
            if(operation != null)
            {
                PersistentData.RegisterValue(PageToLoadKey, page.NameId);
                PersistentData.Save();

                pages.Clear();
                pages.Enqueue(new RefreshingPage(this));
                pages.Peek().Initialize();

                operation();
                Focus();
            }
        }
    }
}
