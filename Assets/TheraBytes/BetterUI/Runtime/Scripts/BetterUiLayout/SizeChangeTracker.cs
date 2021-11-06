using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
    [HelpURL("https://documentation.therabytes.de/better-ui/SizeChangeTracker.html")]
    [AddComponentMenu("Better UI/Helpers/Size Change Tracker", 30)]
    public class SizeChangeTracker : UIBehaviour, ILayoutSelfController
    {
        public RectTransform[] AffectedObjects;
        
        bool isInRecursion = false;

        protected override void OnEnable()
        {
            base.OnEnable();

            CallForAffectedObjects((dp) => dp.ChildAddedOrEnabled(this.transform));
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            CallForAffectedObjects((dp) => dp.ChildRemovedOrDisabled(this.transform));
        }

        private void CallForAffectedObjects(Action<ILayoutChildDependency> function)
        {
            if (isInRecursion)
                return;

            if (function == null)
                throw new ArgumentNullException("function must not be null");

            isInRecursion = true;

            try
            {
                foreach (RectTransform rt in AffectedObjects)
                {
                    if (rt == null)
                        continue;

                    foreach (ILayoutChildDependency dp in rt.GetComponents<MonoBehaviour>()
                        .Where(o => o is ILayoutChildDependency)
                        .Select(o => o as ILayoutChildDependency))
                    {
                        if (dp == null)
                            continue;

                        function(dp);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                isInRecursion = false;
            }

        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            CallForAffectedObjects((dp) => dp.ChildSizeChanged(this.transform));
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            CallForAffectedObjects((dp) => dp.ChildRemovedOrDisabled(this.transform));
        }

        public void SetLayoutHorizontal()
        {
            CallForAffectedObjects((dp) => dp.ChildSizeChanged(this.transform));
        }

        public void SetLayoutVertical()
        {
            CallForAffectedObjects((dp) => dp.ChildSizeChanged(this.transform));
        }

    }
}
