using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    public interface ILayoutChildDependency
    {
        void ChildSizeChanged(Transform child);
        void ChildAddedOrEnabled(Transform child);
        void ChildRemovedOrDisabled(Transform child);
    }
}
