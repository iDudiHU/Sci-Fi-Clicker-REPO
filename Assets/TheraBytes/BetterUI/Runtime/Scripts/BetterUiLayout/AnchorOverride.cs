using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable 0649 // never assigned warning

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif

    [HelpURL("https://documentation.therabytes.de/better-ui/AnchorOverride.html")]
    [AddComponentMenu("Better UI/Layout/Anchor Override", 30)]
    public class AnchorOverride : UIBehaviour, IResolutionDependency
    {
        [Serializable]
        public class AnchorReference
        {
            public enum ReferenceLocation
            {
                Disabled,
                Center,
                Pivot,
                LowerLeft,
                UpperRight,
            }

            [SerializeField] RectTransform reference;
            [SerializeField] ReferenceLocation minX;
            [SerializeField] ReferenceLocation maxX;
            [SerializeField] ReferenceLocation minY;
            [SerializeField] ReferenceLocation maxY;

            public RectTransform Reference { get { return reference; } set { reference = value; } }
            public ReferenceLocation MinX { get { return minX; } }
            public ReferenceLocation MaxX { get { return maxX; } }
            public ReferenceLocation MinY { get { return minY; } }
            public ReferenceLocation MaxY { get { return maxY; } }


        }

        [Serializable]
        public class AnchorReferenceCollection : IScreenConfigConnection
        {
            [SerializeField] List<AnchorReference> elements = new List<AnchorReference>();

            public List<AnchorReference> Elements { get { return elements; } }

            [SerializeField]
            string screenConfigName;
            public string ScreenConfigName { get { return screenConfigName; } set { screenConfigName = value; } }
        }

        [Serializable]
        public class AnchorReferenceCollectionConfigCollection : SizeConfigCollection<AnchorReferenceCollection> { }



        [SerializeField]
        AnchorReferenceCollection anchorsFallback = new AnchorReferenceCollection();

        [SerializeField]
        AnchorReferenceCollectionConfigCollection anchorsConfigs = new AnchorReferenceCollectionConfigCollection();

        [SerializeField] bool isAnimated;
        [SerializeField] float acceleration = 1;
        [SerializeField] float maxMoveSpeed = 0.05f;
        [SerializeField] float snapThreshold = 0.002f;


        AnchorReferenceCollection currentAnchors;

        public AnchorReferenceCollection CurrentAnchors
        {
            get
            {
                if(currentAnchors == null)
                {
                    currentAnchors = anchorsConfigs.GetCurrentItem(anchorsFallback);
                }

                return currentAnchors;
            }
        }

        Canvas canvas;

        RectTransform RectTransform { get { return this.transform as RectTransform; } }

        DrivenRectTransformTracker rectTransformTracker = new DrivenRectTransformTracker();

        float currentVelocity = 0;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            bool instantUpdate =
#if UNITY_EDITOR
                !Application.isPlaying || currentAnchors == null;
#else
                false;
#endif

            currentAnchors = anchorsConfigs.GetCurrentItem(anchorsFallback);

            UpdateAnchors(instantUpdate);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            rectTransformTracker.Clear();
        }

        public void OnResolutionChanged()
        {
            currentAnchors = anchorsConfigs.GetCurrentItem(anchorsFallback);
            UpdateAnchors(true);
        }

        private void Update()
        {
            bool instantUpdate =
#if UNITY_EDITOR
            !Application.isPlaying;
#else
                false;
#endif
            UpdateAnchors(instantUpdate);
        }

        public void UpdateAnchors(bool forceInstant)
        {
            if (!enabled)
                return;

            if (currentAnchors == null)
            {
                currentAnchors = anchorsConfigs.GetCurrentItem(anchorsFallback);
            }

            Vector2 anchorMin = RectTransform.anchorMin;
            Vector2 anchorMax = RectTransform.anchorMax;

            rectTransformTracker.Clear();

            foreach (AnchorReference a in currentAnchors.Elements)
            {
                Rect rect;
                if (TryGetAnchor(a, out rect))
                {
                    if (a.MinX != AnchorReference.ReferenceLocation.Disabled)
                    {
                        anchorMin.x = GetAnchorPosition(a, rect, a.MinX).x;
                        rectTransformTracker.Add(this, this.RectTransform, DrivenTransformProperties.AnchorMinX);
                    }

                    if (a.MaxX != AnchorReference.ReferenceLocation.Disabled)
                    {
                        anchorMax.x = GetAnchorPosition(a, rect, a.MaxX).x;
                        rectTransformTracker.Add(this, this.RectTransform, DrivenTransformProperties.AnchorMaxX);
                    }

                    if (a.MinY != AnchorReference.ReferenceLocation.Disabled)
                    {
                        anchorMin.y = GetAnchorPosition(a, rect, a.MinY).y;
                        rectTransformTracker.Add(this, this.RectTransform, DrivenTransformProperties.AnchorMinY);
                    }

                    if (a.MaxY != AnchorReference.ReferenceLocation.Disabled)
                    {
                        anchorMax.y = GetAnchorPosition(a, rect, a.MaxY).y;
                        rectTransformTracker.Add(this, this.RectTransform, DrivenTransformProperties.AnchorMaxY);
                    }
                }
            }

            if(isAnimated && !forceInstant)
            {
                float distMinX = Mathf.Abs(RectTransform.anchorMin.x - anchorMin.x);
                float distMinY = Mathf.Abs(RectTransform.anchorMin.y - anchorMin.y);
                float distMaxX = Mathf.Abs(RectTransform.anchorMax.x - anchorMax.x);
                float distMaxY = Mathf.Abs(RectTransform.anchorMax.y - anchorMax.y);

                float maxDist = Mathf.Max(distMinX, distMinY, distMaxX, distMaxY);

                if (maxDist <= snapThreshold)
                {
                    currentVelocity = 0;
                    RectTransform.anchorMin = anchorMin;
                    RectTransform.anchorMax = anchorMax;
                    return;
                }

                currentVelocity = Mathf.Clamp01(currentVelocity + acceleration * Time.unscaledDeltaTime);

                float maxMove = currentVelocity * maxDist / 2f;
                float scale = Mathf.Clamp01(maxMoveSpeed / maxMove);
                float amount = 0.5f * scale * currentVelocity;

                float minX = Mathf.Lerp(RectTransform.anchorMin.x, anchorMin.x, amount);
                float minY = Mathf.Lerp(RectTransform.anchorMin.y, anchorMin.y, amount);
                float maxX = Mathf.Lerp(RectTransform.anchorMax.x, anchorMax.x, amount);
                float maxY = Mathf.Lerp(RectTransform.anchorMax.y, anchorMax.y, amount);

                RectTransform.anchorMin = new Vector2(minX, minY);
                RectTransform.anchorMax = new Vector2(maxX, maxY);
            }
            else
            {
                RectTransform.anchorMin = anchorMin;
                RectTransform.anchorMax = anchorMax;
            }
        }


        private static Vector2 GetAnchorPosition(AnchorReference a, Rect rect, AnchorReference.ReferenceLocation location)
        {
            Vector2 localPos = new Vector2();
            switch (location)
            {
                case AnchorReference.ReferenceLocation.Center:
                    localPos = rect.center;
                    break;

                case AnchorReference.ReferenceLocation.Pivot:
                    localPos = rect.min + new Vector2(a.Reference.pivot.x * rect.width, a.Reference.pivot.y * rect.height);
                    break;

                case AnchorReference.ReferenceLocation.LowerLeft:
                    localPos = rect.min;
                    break;

                case AnchorReference.ReferenceLocation.UpperRight:
                    localPos = rect.max;
                    break;

                default:
                    throw new NotImplementedException();
            }

            return localPos;
        }

        bool TryGetAnchor(AnchorReference anchorRef, out Rect anchorObject)
        {
            anchorObject = new Rect();
            if (anchorRef.Reference == null)
                return false;

#if UNITY_EDITOR
            if (IsParentOf(anchorRef.Reference))
            {
                Debug.LogError("Anchor Override: referenced object cannot be a child. Reference is removed.");
                anchorRef.Reference = null;
                return false;
            }
#endif
            Camera cam = null;
            if (canvas == null)
            {
                canvas = this.transform.GetComponentInParent<Canvas>();
            }

            if (canvas != null)
            {
                cam = canvas.worldCamera;
            }

            Rect screenRect = anchorRef.Reference.ToScreenRect(true, canvas);
            Vector2 min = screenRect.min;
            Vector2 max = screenRect.max;

            RectTransform parentRectTransform = this.transform.parent as RectTransform;
            Vector2 localPosMin, localPosMax;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, min, cam, out localPosMin))
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, max, cam, out localPosMax))
                {
                    Vector2 size = parentRectTransform.rect.size;

                    if (size.x == 0 || size.y == 0) // preven division by zero
                        return false;

                    Vector2 pp = parentRectTransform.pivot;
                    localPosMin = new Vector2(pp.x + localPosMin.x / size.x, pp.y + localPosMin.y / size.y);
                    localPosMax = new Vector2(pp.x + localPosMax.x / size.x, pp.y + localPosMax.y / size.y);

                    anchorObject.min = localPosMin;
                    anchorObject.size = localPosMax - localPosMin;

                    return true;
                }
            }

            return false;
        }

        bool IsParentOf(Transform transform)
        {
            if (transform.parent == this.transform)
                return true;

            if (transform.parent == null)
                return false;

            return IsParentOf(transform.parent);
        }


#if UNITY_EDITOR && UNITY_2017_2_OR_NEWER
        void OnDrawGizmos()
        {
            // Ensure continuous Update calls.
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
        }
#endif

    }

}

#pragma warning restore 0649
