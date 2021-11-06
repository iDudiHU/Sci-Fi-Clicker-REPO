using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class Margin
    {
        public float Horizontal { get { return left + right; } }
        public float Vertical { get { return top + bottom; } }

        public int Left { get { return left; } set { left = value; } }
        public int Right { get { return right; } set { right = value; } }
        public int Top { get { return top; } set { top = value; } }
        public int Bottom { get { return bottom; } set { bottom = value; } }

        [SerializeField]
        int left, right, top, bottom;

        public int this[int idx]
        {
            get
            {
                switch (idx)
                {
                    case 0: return left;
                    case 1: return right;
                    case 2: return top;
                    default: return bottom;
                }
            }
            set
            {
                switch (idx)
                {
                    case 0:  left = value;   break;
                    case 1:  right = value;  break;
                    case 2:  top = value;    break;
                    default: bottom = value; break;
                }
            }
        }

        public Margin()
            : this(0, 0, 0, 0)
        { }


        public Margin(Vector4 source)
            : this((int)source.x, (int)source.z, (int)source.y, (int)source.w)
        {
        }

        public Margin(RectOffset source)
            : this(source.left, source.right, source.top, source.bottom)
        {
        }

        public Margin(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public Margin Clone()
        {
            return new Margin(this.left, this.right, this.top, this.bottom);
        }

        public void CopyValuesTo(RectOffset target)
        {
            target.left = this.left;
            target.right = this.right;
            target.top = this.top;
            target.bottom = this.bottom;
        }

        public Vector4 ToVector4()
        {
            return new Vector4(left, top, right, bottom);
        }

        public override string ToString()
        {
            return string.Format("(left: {0}, right: {1}, top: {2}, bottom: {3})", left, right, top, bottom);
        }
    }
}
