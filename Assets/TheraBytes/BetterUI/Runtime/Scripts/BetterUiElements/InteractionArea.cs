using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    [HelpURL("https://documentation.therabytes.de/better-ui/InteractionArea.html")]
    [AddComponentMenu("Better UI/Controls/Interaction Area", 30)]
    public class InteractionArea : Graphic, ICanvasRaycastFilter, IResolutionDependency
    {
        public enum Shape
        {
            Rectangle,
            RoundedRectangle,
            Ellipse,
        }

        public Shape ClickableShape;

        [SerializeField]
        FloatSizeModifier cornerRadiusFallback = new FloatSizeModifier(5, 0, 1000);

        [SerializeField]
        FloatSizeConfigCollection cornerRadiusConfigs = new FloatSizeConfigCollection();

        public float CurrentCornerRadius { get { return cornerRadiusConfigs.GetCurrentItem(cornerRadiusFallback).LastCalculatedSize; } }

        public override void SetMaterialDirty() { /* Do nothing */ }
        public override void SetVerticesDirty() { /* Do nothing */ }
        protected override void OnPopulateMesh(VertexHelper vh) { vh.Clear(); }


        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateCornerRadius();
        }

        public void OnResolutionChanged()
        {
            UpdateCornerRadius();
        }

        public void UpdateCornerRadius()
        {
            cornerRadiusConfigs.GetCurrentItem(cornerRadiusFallback).CalculateSize(this);
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out local);
            Rect rect = rectTransform.rect;

            // Convert to have lower left corner as reference point.
            local.x += rectTransform.pivot.x * rect.width;
            local.y += rectTransform.pivot.y * rect.height;


            switch (ClickableShape)
            {
                case Shape.Rectangle:
                    return true;

                case Shape.RoundedRectangle:
                    {
                        float r = Mathf.Min(CurrentCornerRadius, Mathf.Min(0.5f * rect.width, 0.5f * rect.height));
                        // simple inside check
                        if ((local.x >= r && local.x <= rect.width - r) ||
                            (local.y >= r && local.y <= rect.height - r))
                            return true;

                        float a = 0;
                        float b = 0;

                        if (local.x < r) // left
                        {
                            a = r - local.x;
                        }
                        else if (local.x > rect.width - r) // right
                        {
                            a = local.x - (rect.width - r);
                        }

                        if (local.y < r) // lower
                        {
                            b = r - local.y;
                        }
                        else if (local.y > rect.height - r) // upper
                        {
                            b = local.y - (rect.height - r);
                        }

                        return a * a + b * b <= r * r;
                    }
                case Shape.Ellipse:
                    {
                        float r = 0.5f * Mathf.Max(rect.width, rect.height);
                        float aspect = rect.width / rect.height;
                        float a = local.x - r;
                        float b = local.y * aspect - r;

                        return a * a + b * b <= r * r;
                    }

                default: throw new NotImplementedException();
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = (raycastTarget && this.isActiveAndEnabled)
                ? Color.green 
                : 0.5f * Color.green;

            DrawGizmos();
        }
        void OnDrawGizmos()
        {
            if (!raycastTarget || !this.isActiveAndEnabled)
                return;

            Gizmos.color = Color.gray;
            DrawGizmos();
        }

        void DrawGizmos()
        { 
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Vector3 zero = corners[0];
            Vector3 up = corners[1];
            Vector3 right = corners[3];

            switch (ClickableShape)
            {
                case Shape.Rectangle:
                    {
                        Vector3 a = Transpose(new Vector2(0, 0), zero, up, right);
                        Vector3 b = Transpose(new Vector2(1, 0), zero, up, right);
                        Vector3 c = Transpose(new Vector2(1, 1), zero, up, right);
                        Vector3 d = Transpose(new Vector2(0, 1), zero, up, right);
                        Gizmos.DrawLine(a, b);
                        Gizmos.DrawLine(b, c);
                        Gizmos.DrawLine(c, d);
                        Gizmos.DrawLine(d, a);
                    }
                    break;
                case Shape.RoundedRectangle:
                    {
                        float w = rectTransform.rect.width;
                        float h = rectTransform.rect.height;
                        float radius = Mathf.Min(CurrentCornerRadius, Mathf.Min(0.5f * w, 0.5f * h));
                        float rX = radius / w;
                        float rY = radius / h;

                        Vector3 t1 = Transpose(new Vector2(rX, 0), zero, up, right);
                        Vector3 t2 = Transpose(new Vector2(1 - rX, 0), zero, up, right);
                        Gizmos.DrawLine(t1, t2);

                        Vector3 l1 = Transpose(new Vector2(0, rY), zero, up, right);
                        Vector3 l2 = Transpose(new Vector2(0, 1 - rY), zero, up, right);
                        Gizmos.DrawLine(l1, l2);

                        Vector3 b1 = Transpose(new Vector2(rX, 1), zero, up, right);
                        Vector3 b2 = Transpose(new Vector2(1 - rX, 1), zero, up, right);
                        Gizmos.DrawLine(b1, b2);

                        Vector3 r1 = Transpose(new Vector2(1, rY), zero, up, right);
                        Vector3 r2 = Transpose(new Vector2(1, 1 - rY), zero, up, right);
                        Gizmos.DrawLine(r1, r2);

                        const int segments = 5;
                        float step = (0.5f * Mathf.PI) / (segments );

                        for (int i = 0; i < segments; i++)
                        {

                            float angleA = step * i;
                            float angleB = step * (i + 1);

                            
                            Vector2 a = new Vector2(rX * Mathf.Cos(angleA), rY * Mathf.Sin(angleA));
                            Vector2 b = new Vector2(rX * Mathf.Cos(angleB), rY * Mathf.Sin(angleB));

                            // left bottom
                            Vector2 center = new Vector2(rX, rY);
                            Vector3 start = Transpose(center - a, zero, up, right);
                            Vector3 end = Transpose(center - b, zero, up, right);
                            Gizmos.DrawLine(start, end);

                            // right top
                            center = new Vector2(1 - rX, 1 - rY);
                            start = Transpose(center + a, zero, up, right);
                            end = Transpose(center + b, zero, up, right);
                            Gizmos.DrawLine(start, end);

                            // right bottom
                            center = new Vector2(1 - rX, rY);
                            start = Transpose(new Vector2(center.x + a.x, center.y - a.y), zero, up, right);
                            end = Transpose(new Vector2(center.x + b.x, center.y - b.y), zero, up, right);
                            Gizmos.DrawLine(start, end);

                            // left top
                            center = new Vector2(rX, 1 - rY);
                            start = Transpose(new Vector2(center.x - a.x, center.y + a.y), zero, up, right);
                            end = Transpose(new Vector2(center.x - b.x, center.y + b.y), zero, up, right);
                            Gizmos.DrawLine(start, end);
                        }

                    }
                    break;
                case Shape.Ellipse:
                    {
                        const int segments = 20;
                        float step = (2 * Mathf.PI) / segments;

                        for (int i = 0; i < segments; i++)
                        {
                            float angleA = step * i;
                            float angleB = step * ((i + 1) % segments);

                            Vector2 a = new Vector2(0.5f + 0.5f * Mathf.Cos(angleA), 0.5f + 0.5f * Mathf.Sin(angleA));
                            Vector2 b = new Vector2(0.5f + 0.5f * Mathf.Cos(angleB), 0.5f + 0.5f * Mathf.Sin(angleB));

                            Vector3 start = Transpose(a, zero, up, right);
                            Vector3 end = Transpose(b, zero, up, right);

                            Gizmos.DrawLine(start, end);
                        }
                    }
                    break;
                default: throw new NotImplementedException();
            }

        }

        Vector3 Transpose(Vector2 relativePosition, Vector3 zeroPoint, Vector3 upperPoint, Vector3 rightPoint)
        {
            Vector3 x = relativePosition.x * (rightPoint - zeroPoint);
            Vector3 y = relativePosition.y * (upperPoint - zeroPoint);
            return zeroPoint + x + y;
        }

    }
}
