using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheraBytes.BetterUi
{ 
    public interface IBetterTransitionUiElement
    {
        List<Transitions> BetterTransitions { get; }
    }
}
