using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public abstract class WizardPage
    {
        public abstract string NameId { get; }

        protected List<WizardPageElementBase> elements = new List<WizardPageElementBase>();
        Vector2 scrollPosition;

        protected virtual string NextButtonText { get { return "Next"; } }
        protected IWizard wizard;

        public WizardPage(IWizard wizard)
        {
            this.wizard = wizard;
        }

        public void Initialize()
        {
            OnInitialize();
            Load();
        }

        protected abstract void OnInitialize();

        public void Add(WizardPageElementBase element)
        {
            elements.Add(element);
            if(elements.Count == 1)
            {
                element.Activate();
            }
        }

        public void DrawGui()
        {
            BeforeGui();
            this.scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            bool disableNext = false;
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];

                switch (element.State)
                {
                    case WizardElementState.Complete:
                        element.DrawGui();
                        ActivateIfPending(i + 1);
                        break;

                    case WizardElementState.WaitForInput:
                        element.DrawGui();
                        disableNext = true;
                        break;

                    case WizardElementState.Pending:
                        disableNext = true;
                        break;

                    default: throw new NotImplementedException(); ;
                }
            }

            EditorGUILayout.EndScrollView();


            EditorGUI.BeginDisabledGroup(disableNext);
            if(GUILayout.Button(NextButtonText, GUILayout.Height(40)))
            {
                NextButtonClicked();
            }
            EditorGUI.EndDisabledGroup();

            AfterGui();
        }

        protected virtual void NextButtonClicked()
        {
            Save();
            wizard.PageFinished(this);
        }

        protected virtual void BeforeGui() { }
        protected virtual void AfterGui()
        {
            string page = string.Format("Page {0} / {1}     ", wizard.CurrentPageNumber + 1, wizard.TotalPageCount);
            for(int i = 0; i < wizard.TotalPageCount; i++)
            {
                page += (i == wizard.CurrentPageNumber) ? "♠ " : "○ ";
            }

            EditorGUILayout.LabelField(page);
        }

        void ActivateIfPending(int index)
        {
            if (index >= elements.Count)
                return;

            if (elements[index].State != WizardElementState.Pending)
                return;

            elements[index].Activate();
        }

        void Save()
        {
            foreach(var element in elements)
            {
                var dataElement = element as IWizardDataElement;
                if (dataElement != null)
                {
                    string key = dataElement.SerializationKey;
                    string value = dataElement.GetValueAsString();

                    wizard.PersistentData.RegisterValue(key, value);
                }
            }

            wizard.PersistentData.Save();
        }

        private void Load()
        {
            // apply saved data to elements until there is data missing
            int lastCompleteIndex = -1;
            for(int i = 0; i < elements.Count; i++)
            {
                var dataElement = elements[i] as IWizardDataElement;
                if (dataElement != null)
                {
                    string stringValue;
                    if(wizard.PersistentData.TryGetValue(dataElement.SerializationKey, out stringValue))
                    {
                        dataElement.TrySetValue(stringValue);
                        lastCompleteIndex = i;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // mark all elements complete until the last one which could be loaded successfully
            for(int i = 0; i <= lastCompleteIndex; i++)
            {
                elements[i].MarkComplete();
            }

            // activate all upcoming elements until user interaction is required.
            for(int i = lastCompleteIndex + 1; i < elements.Count; i++)
            {
                elements[i].Activate();

                if (elements[i].State == WizardElementState.WaitForInput)
                    break;
            }
        }
    }
}
