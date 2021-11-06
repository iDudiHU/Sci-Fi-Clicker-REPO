using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    [HelpURL("https://documentation.therabytes.de/better-ui/BetterLocator.html")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Better UI/Layout/Better Locator", 30)]
    public class BetterLocator : MonoBehaviour, IResolutionDependency
    {

        [Serializable]
        public class RectTransformDataConfigCollection : SizeConfigCollection<RectTransformData> { }

        [SerializeField]
        RectTransformData transformFallback;

        [SerializeField]
        RectTransformDataConfigCollection transformConfigs = new RectTransformDataConfigCollection();

        public RectTransformData CurrentTransformData { get { return transformConfigs.GetCurrentItem(transformFallback); } }

        RectTransform rectTransform { get { return this.transform as RectTransform; } }

        void OnEnable()
        {
            if (transformFallback == null) // happens when added in editor during play mode
            {
                InitTransformFallback();
            }

            CurrentTransformData.PushToTransform(rectTransform);
        }

        public void OnResolutionChanged()
        {
            CurrentTransformData.PushToTransform(rectTransform);
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            bool isUnInitialized = transformFallback == null
                || (transformConfigs.Items.Count == 0
                && transformFallback.Scale == Vector3.zero
                && transformFallback.Rotation.eulerAngles == Vector3.zero
                && transformFallback.AnchoredPosition == Vector2.zero
                && transformFallback.AnchorMin == Vector2.zero
                && transformFallback.AnchorMax == Vector2.zero
                && transformFallback.Pivot == Vector2.zero
                && transformFallback.SizeDelta == Vector2.zero
                && transformFallback.LocalPosition == Vector3.zero);

            if (isUnInitialized)
            {
                InitTransformFallback();
            }
        }
#endif

        void InitTransformFallback()
        {
            if (transformFallback == null)
            {
                transformFallback = new RectTransformData();
            }

            transformFallback.PullFromTransform(rectTransform);
        }
    }
}
