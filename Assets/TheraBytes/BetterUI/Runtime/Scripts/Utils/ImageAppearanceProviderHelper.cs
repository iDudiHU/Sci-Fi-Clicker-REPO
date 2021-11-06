using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
    public static class ImageAppearanceProviderHelper
    {

        public static void SetMaterialType(string value, 
            Graphic graphic, VertexMaterialData materialProperties, ref MaterialEffect effect, ref string materialType)
        {
            if (materialType == value && graphic.material != null)
                return;

            materialType = value;
            UpdateMaterial(graphic, materialProperties, ref effect, ref materialType);
        }

        public static void SetMaterialEffect(MaterialEffect value,
            Graphic graphic, VertexMaterialData materialProperties, ref MaterialEffect effect, ref string materialType)
        {
            if (effect == value && graphic.material != null)
                return;
            
            effect = value;
            UpdateMaterial(graphic, materialProperties, ref effect, ref materialType);
        }

        private static void UpdateMaterial(
            Graphic graphic, VertexMaterialData materialProperties, ref MaterialEffect effect, ref string materialType)
        {
            var info = Materials.Instance.GetMaterialInfo(materialType, effect);
            materialProperties.Clear();
            if (info != null)
            {
                graphic.material = info.Material;
                info.Properties.CopyTo(materialProperties);
            }
            else
            {
                graphic.material = null;
            }
        }


        public static void SetMaterialProperty(int propertyIndex, float value,
            Graphic graphic, VertexMaterialData materialProperties,
            ref float materialProperty1, ref float materialProperty2, ref float materialProperty3)
        {
            if (propertyIndex < 0 || propertyIndex >= 3)
                throw new ArgumentException("the propertyIndex can have the value 0, 1 or 2.");

            switch (propertyIndex)
            {
                case 0: materialProperty1 = value; break;
                case 1: materialProperty2 = value; break;
                case 2: materialProperty3 = value; break;
            }

            if (materialProperties.FloatProperties.Length > propertyIndex)
            {
                materialProperties.FloatProperties[propertyIndex].Value = value;
                graphic.SetVerticesDirty();
            }
        }

        public static float GetMaterialPropertyValue(int propertyIndex,
            ref float materialProperty1, ref float materialProperty2, ref float materialProperty3)
        {
            if (propertyIndex < 0 || propertyIndex >= 3)
                throw new ArgumentException("the propertyIndex can have the value 0, 1 or 2.");

            switch (propertyIndex)
            {
                case 0: return materialProperty1;
                case 1: return materialProperty2;
                case 2: return materialProperty3;
                default: return 0;
            }
        }

        public static void AddQuad(
            VertexHelper vertexHelper, Rect bounds,
            Vector2 posMin, Vector2 posMax,
            ColorMode mode, Color colorA, Color colorB,
            Vector2 uvMin, Vector2 uvMax,
            VertexMaterialData materialProperties)
        {
            int cnt = vertexHelper.currentVertCount;

            Color32[] colors = new Color32[4];
            colors[0] = GetColor(mode, colorA, colorB, bounds, posMin.x, posMin.y);
            colors[1] = GetColor(mode, colorA, colorB, bounds, posMin.x, posMax.y);
            colors[2] = GetColor(mode, colorA, colorB, bounds, posMax.x, posMax.y);
            colors[3] = GetColor(mode, colorA, colorB, bounds, posMax.x, posMin.y);

            float uvX = 0, uvY = 0, tangentW = 0;
            materialProperties.Apply(ref uvX, ref uvY, ref tangentW);

            Vector2 uv1 = new Vector2(uvX, uvY);
            Vector3 normal = Vector3.back;
            Vector4 tangent = new Vector4(1, 0, 0, tangentW);

            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), colors[0], new Vector2(uvMin.x, uvMin.y),
                uv1, normal, tangent);
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), colors[1], new Vector2(uvMin.x, uvMax.y),
                uv1, normal, tangent);
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), colors[2], new Vector2(uvMax.x, uvMax.y),
                uv1, normal, tangent);
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), colors[3], new Vector2(uvMax.x, uvMin.y),
                uv1, normal, tangent);

            vertexHelper.AddTriangle(cnt, cnt + 1, cnt + 2);
            vertexHelper.AddTriangle(cnt + 2, cnt + 3, cnt);
        }

        static Color32 GetColor(ColorMode mode, Color a, Color b, Rect bounds, float x, float y)
        {
            switch (mode)
            {
                case ColorMode.Color:
                    return a;

                case ColorMode.HorizontalGradient:
                    {
                        float amount = (x - bounds.xMin) / bounds.size.x;
                        return Color.Lerp(a, b, amount);
                    }

                case ColorMode.VerticalGradient:
                    {
                        float amount = 1 - (y - bounds.yMin) / bounds.size.y;
                        return Color.Lerp(a, b, amount);
                    }

                default: throw new NotImplementedException();
            }
        }
    }
}
