using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    public interface ISizeConfigCollection
    {
        string GetCurrentConfigName();
    }

    [Serializable]
    public class SizeConfigCollection<T> : ISizeConfigCollection
        where T : class, IScreenConfigConnection
    {
        [SerializeField]
        List<T> items = new List<T>();
        
        public List<T> Items { get { return items; } }

        bool sorted = false;

        public void Sort()
        {
            List<string> order = ResolutionMonitor.Instance.OptimizedScreens.Select(o => o.Name).ToList();
            items.Sort((a, b) => order.IndexOf(a.ScreenConfigName).CompareTo(order.IndexOf(b.ScreenConfigName)));

            sorted = true;
        }

        public string GetCurrentConfigName()
        {
            T result = GetCurrentItem(null);

            if (result != null)
                return result.ScreenConfigName;

            return null;
        }

        public T GetCurrentItem(T fallback)
        {
            // if there is no config matching the screen
            if (ResolutionMonitor.CurrentScreenConfiguration == null)
                return fallback;

            if (!(sorted))
            {
                Sort();
            }


#if UNITY_EDITOR
            
            // simulation
            var config = ResolutionMonitor.SimulatedScreenConfig;
            if (config != null)
            {
                if (Items.Any(o => o.ScreenConfigName == config.Name))
                {
                    return Items.First(o => o.ScreenConfigName == config.Name);
                }
            }
#endif

            // search for screen config
            foreach (T item in items)
            {
                if (string.IsNullOrEmpty(item.ScreenConfigName))
                    return fallback;

                var c = ResolutionMonitor.GetConfig(item.ScreenConfigName);
                if(c != null && c.IsActive)
                {
                    return item;
                }
            }
            
            // fallback logic
            foreach (var conf in ResolutionMonitor.GetCurrentScreenConfigurations())
            {
                foreach (var c in conf.Fallbacks)
                {
                    var matchingItem = items.FirstOrDefault(o => o.ScreenConfigName == c);
                    if (matchingItem != null)
                        return matchingItem;
                }
            }

            // final fallback
            return fallback;
        }
        
    }
}
