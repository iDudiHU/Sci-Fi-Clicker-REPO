using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheraBytes.BetterUi.Editor
{
    public interface IWizardDataElement
    {
        string SerializationKey { get; }
        string GetValueAsString();
        bool TrySetValue(string input);
    }
}
