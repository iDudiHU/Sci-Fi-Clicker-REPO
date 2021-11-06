using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheraBytes.BetterUi
{ 
    public interface IScreenTypeCheck : IIsActive
    {
        bool IsScreenType();
    }
}
