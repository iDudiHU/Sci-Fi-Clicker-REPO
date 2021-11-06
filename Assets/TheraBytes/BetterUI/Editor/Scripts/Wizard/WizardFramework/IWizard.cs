using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheraBytes.BetterUi.Editor
{
    public interface IWizard
    {
        int CurrentPageNumber { get; }
        int TotalPageCount { get; }

        PersistentWizardData PersistentData { get; }
        void PageFinished(WizardPage page);
        void DoReloadOperation(WizardPage page, Action operation);
        void JumpToPage<T>() where T : WizardPage;
        void Close();
    }
}
