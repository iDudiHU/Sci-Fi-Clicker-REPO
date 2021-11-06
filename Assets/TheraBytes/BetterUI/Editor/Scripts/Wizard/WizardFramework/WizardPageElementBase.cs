using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheraBytes.BetterUi.Editor
{
    public abstract class WizardPageElementBase
    {
        protected bool markCompleteImmediately;

        public WizardElementState State { get; protected set; }
        public abstract void DrawGui();

        public WizardPageElementBase Activate()
        {
            if(markCompleteImmediately)
            {
                State = WizardElementState.Complete;
                return this;
            }

            State = WizardElementState.WaitForInput;
            return this;
        }

        public WizardPageElementBase MarkComplete()
        {
            if(State == WizardElementState.Pending)
            {
                markCompleteImmediately = true;
                return this;
            }
            
            State = WizardElementState.Complete;
            return this;
        }
    }

    public enum WizardElementState
    {
        Pending,
        WaitForInput,
        Complete,
    }
}
