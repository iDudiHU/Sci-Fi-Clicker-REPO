using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class RectTransformData : IScreenConfigConnection
    {
        public static readonly RectTransformData Invalid = new RectTransformData();
        public static readonly RectTransformData Identity = new RectTransformData()
        {
            Rotation = Quaternion.identity,
            Scale = Vector3.one,
        };


        public Vector3 LocalPosition;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
        public Vector3 Scale;

        [FormerlySerializedAs("Rotation")]
        [SerializeField]
        Quaternion rotation;

        public Vector3 EulerAngles;

        public Quaternion Rotation
        {
            get
            {
                return (saveRotationAsEuler) ? Quaternion.Euler(EulerAngles) : rotation;
            }
            set
            {
                rotation = value;

                if (saveRotationAsEuler)
                {
                    EulerAngles = rotation.eulerAngles;
                }
            }
        }

        [SerializeField]
        bool saveRotationAsEuler = false;
        public bool SaveRotationAsEuler
        {
            get
            {
                return saveRotationAsEuler;
            }
            set
            {
                if (saveRotationAsEuler == value)
                    return;

                saveRotationAsEuler = value;
            }
        }

        /// <summary>
        ///   <para>The offset of the upper right corner of the rectangle relative to the upper right anchor.</para>
        /// </summary>
        public Vector2 OffsetMax
        {
            get
            {
                return this.AnchoredPosition + Vector2.Scale(this.SizeDelta, Vector2.one - this.Pivot);
            }
            set
            {
                Vector2 v = value - (this.AnchoredPosition + Vector2.Scale(this.SizeDelta, Vector2.one - this.Pivot));
                this.SizeDelta = this.SizeDelta + v;
                this.AnchoredPosition = this.AnchoredPosition + Vector2.Scale(v, this.Pivot);
            }
        }

        /// <summary>
        ///   <para>The offset of the lower left corner of the rectangle relative to the lower left anchor.</para>
        /// </summary>
        public Vector2 OffsetMin
        {
            get
            {
                return this.AnchoredPosition - Vector2.Scale(this.SizeDelta, this.Pivot);
            }
            set
            {
                Vector2 v = value - (this.AnchoredPosition - Vector2.Scale(this.SizeDelta, this.Pivot));
                this.SizeDelta = this.SizeDelta - v;
                this.AnchoredPosition = this.AnchoredPosition + Vector2.Scale(v, Vector2.one - this.Pivot);
            }
        }

        [SerializeField]
        string screenConfigName;
        public string ScreenConfigName { get { return screenConfigName; } set { screenConfigName = value; } }

        public RectTransformData()
        {

        }

        public RectTransformData(RectTransform rectTransform)
        {
            PullFromTransform(rectTransform);
        }

        public static RectTransformData Combine(RectTransformData original, RectTransformData addition)
        {
            RectTransformData result = new RectTransformData();

            result.AnchoredPosition = original.AnchoredPosition + addition.AnchoredPosition;
            result.AnchorMin = original.AnchorMin + addition.AnchorMin;
            result.AnchorMax = original.AnchorMax + addition.AnchorMax;
            result.Pivot = original.Pivot + addition.Pivot;
            result.SizeDelta = original.SizeDelta + addition.SizeDelta;
            result.LocalPosition = original.LocalPosition + addition.LocalPosition;
            result.Scale = new Vector3(
                original.Scale.x * addition.Scale.x,
                original.Scale.y * addition.Scale.y, 
                original.Scale.z * addition.Scale.z);

            result.saveRotationAsEuler = original.saveRotationAsEuler;
            result.Rotation = original.Rotation * addition.Rotation;

            return result;
        }

        public static RectTransformData Separate(RectTransformData original, RectTransformData subtraction)
        {
            RectTransformData result = new RectTransformData();
            result.AnchoredPosition = original.AnchoredPosition - subtraction.AnchoredPosition;
            result.AnchorMin = original.AnchorMin - subtraction.AnchorMin;
            result.AnchorMax = original.AnchorMax - subtraction.AnchorMax;
            result.Pivot = original.Pivot - subtraction.Pivot;
            result.SizeDelta = original.SizeDelta - subtraction.SizeDelta;
            result.LocalPosition = original.LocalPosition - subtraction.LocalPosition;
            result.Scale = new Vector3(
                original.Scale.x / subtraction.Scale.x,
                original.Scale.y / subtraction.Scale.y, 
                original.Scale.z / subtraction.Scale.z);

            result.saveRotationAsEuler = original.saveRotationAsEuler;
            result.Rotation = original.Rotation * Quaternion.Inverse(subtraction.Rotation);

            return result;

        }

        public RectTransformData PullFromData(RectTransformData transformData)
        {
            this.LocalPosition = transformData.LocalPosition;
            this.AnchorMin = transformData.AnchorMin;
            this.AnchorMax = transformData.AnchorMax;
            this.Pivot = transformData.Pivot;
            this.AnchoredPosition = transformData.AnchoredPosition;
            this.SizeDelta = transformData.SizeDelta;
            this.Scale = transformData.Scale;

            this.saveRotationAsEuler = transformData.saveRotationAsEuler;
            this.Rotation = transformData.Rotation;
            this.EulerAngles = transformData.EulerAngles;

            return this;
        }

        public void PullFromTransform(RectTransform transform)
        {
            this.LocalPosition = transform.localPosition;
            this.AnchorMin = transform.anchorMin;
            this.AnchorMax = transform.anchorMax;
            this.Pivot = transform.pivot;
            this.AnchoredPosition = transform.anchoredPosition;
            this.SizeDelta = transform.sizeDelta;
            this.Scale = transform.localScale;

            this.Rotation = transform.localRotation;
            this.EulerAngles = transform.localEulerAngles;
        }


        public void PushToTransform(RectTransform transform)
        {
            transform.localPosition = this.LocalPosition;
            transform.anchorMin = this.AnchorMin;
            transform.anchorMax = this.AnchorMax;
            transform.pivot = this.Pivot;
            transform.anchoredPosition = this.AnchoredPosition;
            transform.sizeDelta = this.SizeDelta;
            transform.localScale = this.Scale;

            if (SaveRotationAsEuler)
            {
                transform.eulerAngles = this.EulerAngles;
            }
            else
            {
                transform.localRotation = this.Rotation;
            }
        }

        public Rect ToRect(Rect parentRect, bool relativeSpace = false)
        {
            float xMin = (AnchorMin.x * parentRect.width) + AnchoredPosition.x - Pivot.x * SizeDelta.x;
            float xMax = (AnchorMax.x * parentRect.width) + AnchoredPosition.x + (1 - Pivot.x) * SizeDelta.x;

            float yMin = (AnchorMin.y * parentRect.height) + AnchoredPosition.y - Pivot.y * SizeDelta.y;
            float yMax = (AnchorMax.y * parentRect.height) + AnchoredPosition.y + (1 - Pivot.y) * SizeDelta.y;

            if(relativeSpace)
            {
                xMin /= parentRect.width;
                xMax /= parentRect.width;
                yMin /= parentRect.height;
                yMax /= parentRect.height;
            }

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public static RectTransformData Lerp(RectTransformData a, RectTransformData b, float amount)
        {
            return Lerp(a, b, amount, a.SaveRotationAsEuler || b.SaveRotationAsEuler);
        }

        public static RectTransformData Lerp(RectTransformData a, RectTransformData b, float amount, bool eulerRotation)
        {
            return new RectTransformData()
            {
                AnchoredPosition = Vector2.Lerp(a.AnchoredPosition, b.AnchoredPosition, amount),
                AnchorMax = Vector2.Lerp(a.AnchorMax, b.AnchorMax, amount),
                AnchorMin = Vector2.Lerp(a.AnchorMin, b.AnchorMin, amount),
                LocalPosition = Vector3.Lerp(a.LocalPosition, b.LocalPosition, amount),
                Pivot = Vector2.Lerp(a.Pivot, b.Pivot, amount),
                Scale = Vector3.Lerp(a.Scale, b.Scale, amount),
                SizeDelta = Vector2.Lerp(a.SizeDelta, b.SizeDelta, amount),
                Rotation = Quaternion.Lerp(a.Rotation, b.Rotation, amount),
                EulerAngles = Vector3.Lerp(a.EulerAngles, b.EulerAngles, amount),
                SaveRotationAsEuler = eulerRotation
            };
        }

        public static RectTransformData LerpUnclamped(RectTransformData a, RectTransformData b, float amount)
        {
            return LerpUnclamped(a, b, amount, a.SaveRotationAsEuler || b.SaveRotationAsEuler);
        }

        public static RectTransformData LerpUnclamped(RectTransformData a, RectTransformData b, float amount, bool eulerRotation)
        {
            return new RectTransformData()
            {
                AnchoredPosition = Vector2.LerpUnclamped(a.AnchoredPosition, b.AnchoredPosition, amount),
                AnchorMax = Vector2.LerpUnclamped(a.AnchorMax, b.AnchorMax, amount),
                AnchorMin = Vector2.LerpUnclamped(a.AnchorMin, b.AnchorMin, amount),
                LocalPosition = Vector3.LerpUnclamped(a.LocalPosition, b.LocalPosition, amount),
                Pivot = Vector2.LerpUnclamped(a.Pivot, b.Pivot, amount),
                Scale = Vector3.LerpUnclamped(a.Scale, b.Scale, amount),
                SizeDelta = Vector2.LerpUnclamped(a.SizeDelta, b.SizeDelta, amount),
                Rotation = Quaternion.LerpUnclamped(a.Rotation, b.Rotation, amount),
                EulerAngles = Vector3.LerpUnclamped(a.EulerAngles, b.EulerAngles, amount),
                SaveRotationAsEuler = eulerRotation
            };
        }

        public override int GetHashCode()
        {
            return this.AnchoredPosition.GetHashCode()
                 ^ this.AnchorMin.GetHashCode()
                 ^ this.AnchorMax.GetHashCode()
                 ^ this.LocalPosition.GetHashCode()
                 ^ this.Pivot.GetHashCode()
                 ^ this.Scale.GetHashCode()
                 ^ this.SizeDelta.GetHashCode()
                 ^ this.rotation.GetHashCode()
                 ^ this.saveRotationAsEuler.GetHashCode()
                 ^ this.EulerAngles.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }

        public static bool operator ==(RectTransformData a, RectTransformData b)
        {
            bool aIsNull = ReferenceEquals(a, null);
            bool bIsNull = ReferenceEquals(b, null);

            if (aIsNull || bIsNull)
                return aIsNull && bIsNull;

            return a.AnchoredPosition   == b.AnchoredPosition
                && a.AnchorMin          == b.AnchorMin
                && a.AnchorMax          == b.AnchorMax
                && a.Pivot              == b.Pivot
                && a.SizeDelta          == b.SizeDelta

                && a.LocalPosition      == b.LocalPosition
                && a.Scale              == b.Scale
                && a.Rotation.Equals(b.Rotation);
        }
        public static bool operator !=(RectTransformData a, RectTransformData b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("RectTransformData: sizeDelta {{{0}, {1}}} - anchoredPosition {{{2}, {3}}}",
                SizeDelta.x, SizeDelta.y, AnchoredPosition.x, AnchoredPosition.y);
        }

    }
}