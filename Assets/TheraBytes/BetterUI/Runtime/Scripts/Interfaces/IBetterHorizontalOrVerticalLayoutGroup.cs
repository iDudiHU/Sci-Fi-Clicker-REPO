using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheraBytes.BetterUi
{
    public interface IBetterHorizontalOrVerticalLayoutGroup
    {
        MarginSizeModifier PaddingSizer { get; }
        FloatSizeModifier SpacingSizer { get; }
    }
}
