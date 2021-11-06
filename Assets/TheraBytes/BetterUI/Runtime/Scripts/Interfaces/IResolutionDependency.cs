using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheraBytes.BetterUi
{
    /// <summary>
    /// Implemnent this interface if your object should do some recalculations on resolution change.
    /// The OnResolutionChanged() method will be automatically called by the ResolutionMonitor.
    /// </summary>
    public interface IResolutionDependency
    {
        void OnResolutionChanged();
    }
}
