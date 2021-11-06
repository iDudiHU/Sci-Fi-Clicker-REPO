using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class VertexMaterialData
    {
        #region Property Types

        [Serializable]
        public abstract class Property<T>
        {
            public string Name;
            public T Value;

            public abstract void SetValue(ref float uvX, ref float uvY, ref float tangentW);
            public abstract Property<T> Clone();
        }

        [Serializable]
        public class FloatProperty : Property<float>
        {
            public enum Mapping
            {
                TexcoordX, TexcoordY, TangentW,
            }

            public Mapping PropertyMap;
            public float Min, Max;

            public bool IsRestricted { get { return Min < Max; } }

            public override void SetValue(ref float uvX, ref float uvY, ref float tangentW)
            {
                switch (PropertyMap)
                {
                    case Mapping.TexcoordX:
                        uvX = Value;
                        break;
                    case Mapping.TexcoordY:
                        uvY = Value;
                        break;
                    //case Mapping.NormalX:
                    //    normal.x = Value;
                    //    break;
                    //case Mapping.NormalY:
                    //    normal.y = Value;
                    //    break;
                    //case Mapping.NormalZ:
                    //    normal.z = Value;
                    //    break;
                    //case Mapping.TangentX:
                    //    tangent.x = Value;
                    //    break;
                    //case Mapping.TangentY:
                    //    tangent.y = Value;
                    //    break;
                    //case Mapping.TangentZ:
                    //    tangent.z = Value;
                    //    break;
                    case Mapping.TangentW:
                        tangentW = Value;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            public override Property<float> Clone()
            {
                return new FloatProperty()
                {
                    Name = this.Name,
                    Value = this.Value,
                    Min = this.Min,
                    Max = this.Max,
                    PropertyMap = this.PropertyMap,
                };
            }
        }

        #region not supported yet
        //[Serializable]
        //public class Vector2Property : Property<Vector2>
        //{
        //    public enum Mapping
        //    {
        //        TexCoord, NormalXY, TangentXY, TangentZW,
        //    }

        //    public Mapping PropertyMap;

        //    public override void SetValue(ref Vector2 texCoord, ref Vector3 normal, ref Vector4 tangent)
        //    {
        //        switch (PropertyMap)
        //        {
        //            case Mapping.TexCoord:
        //                texCoord = Value;
        //                break;
        //            case Mapping.NormalXY:
        //                normal.x = Value.x;
        //                normal.y = Value.y;
        //                break;
        //            case Mapping.TangentXY:
        //                tangent.x = Value.x;
        //                tangent.y = Value.y;
        //                break;
        //            case Mapping.TangentZW:
        //                tangent.z = Value.x;
        //                tangent.w = Value.y;
        //                break;
        //            default: throw new ArgumentException();
        //        }
        //    }


        //    public override Property<Vector2> Clone()
        //    {
        //        return new Vector2Property()
        //        {
        //            Name = this.Name,
        //            Value = this.Value,
        //            PropertyMap = this.PropertyMap,
        //        };
        //    }
        //}

        //[Serializable]
        //public class Vector3Property : Property<Vector3>
        //{
        //    public enum Mapping
        //    {
        //        Normal, TangentXYZ,
        //    }

        //    public Mapping PropertyMap;

        //    public override void SetValue(ref Vector2 texCoord, ref Vector3 normal, ref Vector4 tangent)
        //    {
        //        switch (PropertyMap)
        //        {
        //            case Mapping.Normal:
        //                normal = Value;
        //                break;
        //            case Mapping.TangentXYZ:
        //                tangent.x = Value.x;
        //                tangent.y = Value.y;
        //                tangent.z = Value.z;
        //                break;
        //            default: throw new ArgumentException();
        //        }
        //    }


        //    public override Property<Vector3> Clone()
        //    {
        //        return new Vector3Property()
        //        {
        //            Name = this.Name,
        //            Value = this.Value,
        //            PropertyMap = this.PropertyMap,
        //        };
        //    }
        //}

        //[Serializable]
        //public class Vector4Property : Property<Vector4>
        //{
        //    public override void SetValue(ref Vector2 texCoord, ref Vector3 normal, ref Vector4 tangent)
        //    {
        //        tangent = Value;
        //    }

        //    public override Property<Vector4> Clone()
        //    {
        //        return new Vector4Property()
        //        {
        //            Name = this.Name,
        //            Value = this.Value,
        //        };
        //    }
        //}

        //[Serializable]
        //public class ColorProperty : Property<Color>
        //{
        //    //public enum Mapping
        //    //{
        //    //    Normal, Tangent,
        //    //}

        //    //public Mapping PropertyMap;

        //    public override void SetValue(ref float uvX, ref float uvY, ref float tangentW)
        //    {
        //        uvX = Value.r;
        //        uvY = Value.g;
        //        tangentW = Value.b;
        //        //switch (PropertyMap)
        //        //{
        //        //    case Mapping.Normal:
        //        //        normal.x = Value.r;
        //        //        normal.y = Value.g;
        //        //        normal.z = Value.b;
        //        //        break;
        //        //    case Mapping.Tangent:
        //        //        tangent.x = Value.r;
        //        //        tangent.y = Value.g;
        //        //        tangent.z = Value.b;
        //        //        tangent.w = Value.a;
        //        //        break;
        //        //    default: throw new ArgumentException();
        //        //}
        //    }

        //    public override Property<Color> Clone()
        //    {
        //        return new ColorProperty()
        //        {
        //            Name = this.Name,
        //            Value = this.Value,
        //            //PropertyMap = this.PropertyMap,
        //        };
        //    }
        //}
        #endregion

        #endregion

        public FloatProperty[] FloatProperties;
        //public Vector2Property[] Vector2Properties;
        //public Vector3Property[] Vector3Properties;
        //public Vector4Property[] Vector4Properties;
        //public ColorProperty[] ColorProperties;

        public void Apply(ref float uvX, ref float uvY, ref float tangentW)
        {
            VertexMaterialData.Apply(FloatProperties, ref uvX, ref uvY, ref tangentW);
            //VertexMaterialData.Apply(Vector2Properties, ref texCoord, ref normal, ref tangent);
            //VertexMaterialData.Apply(Vector3Properties, ref texCoord, ref normal, ref tangent);
            //VertexMaterialData.Apply(Vector4Properties, ref texCoord, ref normal, ref tangent);
            //VertexMaterialData.Apply(ColorProperties, ref uvX, ref uvY, ref tangentW);
        }

        private static void Apply<T>(IEnumerable<Property<T>> prop,
            ref float uvX, ref float uvY, ref float tangentW)
        {
            if (prop == null)
                return;

            foreach (var item in prop)
            {
                item.SetValue(ref uvX, ref uvY, ref tangentW);
            }
        }

        public void Clear()
        {
            FloatProperties = new FloatProperty[0];
            //Vector2Properties = new Vector2Property[0];
            //Vector3Properties = new Vector3Property[0];
            //Vector4Properties = new Vector4Property[0];
            //ColorProperties = new ColorProperty[0];
        }

        public void CopyTo(VertexMaterialData target)
        {
            target.FloatProperties = CloneArray<FloatProperty, float>(this.FloatProperties);
            //target.Vector2Properties = CloneArray<Vector2Property, Vector2>(this.Vector2Properties);
            //target.Vector3Properties = CloneArray<Vector3Property, Vector3>(this.Vector3Properties);
            //target.Vector4Properties = CloneArray<Vector4Property, Vector4>(this.Vector4Properties);
            //target.ColorProperties = CloneArray<ColorProperty, Color>(this.ColorProperties);
        }


        public VertexMaterialData Clone()
        {
            VertexMaterialData result = new VertexMaterialData();
            this.CopyTo(result);

            return result;
        }

        static T[] CloneArray<T, TValue>(T[] array)
            where T : Property<TValue>
        {
            T[] result = new T[array.Length];
            for(int i = 0; i < array.Length; i++)
            {
                result[i] = array[i].Clone() as T;
            }

            return result;
        }
    }
}
