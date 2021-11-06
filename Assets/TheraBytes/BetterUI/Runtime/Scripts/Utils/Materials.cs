using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheraBytes.BetterUi
{
    public enum MaterialEffect
    {
        Normal,
        Additive,
        LinearDodge,
        Multiply,
        Overlay,
    }
    
    [HelpURL("https://documentation.therabytes.de/better-ui/AboutMaterials.html")]
    public class Materials : SingletonScriptableObject<Materials>
    {
        const string STANDARD = "Standard";
        const string GRAYSCALE = "Grayscale";
        const string HUE_SATURATION_BRIGHTNESS = "Hue Saturation Brightness";

        static string FilePath { get { return "TheraBytes/Resources/Materials"; } }

        static readonly List<string> materialOrder = new List<string>() { STANDARD, GRAYSCALE, HUE_SATURATION_BRIGHTNESS };

        [Serializable]
        public class MaterialInfo
        {
            public string Name;
            public Material Material;
            public VertexMaterialData Properties = new VertexMaterialData();
            public MaterialEffect Effect;

            public override string ToString()
            {
                return string.Format("{0} ({1})", Name, Effect);
            }
        }

        [SerializeField]
        List<MaterialInfo> materials = new List<MaterialInfo>();

        private void OnEnable()
        {
            EnsurePredefinedMaterials();
        }

        void EnsurePredefinedMaterials()
        {
            if (materials.Count > 0)
                return;

            // Add predefined materials

            AddIfNotPresent("Standard",
                (e) => new MaterialInfo()
                {
                    Material = Resources.Load<Material>("Materials/Standard_" + e.ToString()),
                });

            AddIfNotPresent("Grayscale",
                (e) => new MaterialInfo()
                {
                    Material = Resources.Load<Material>("Materials/Grayscale_" + e.ToString()),
                    Properties = new VertexMaterialData()
                    {
                        FloatProperties = new VertexMaterialData.FloatProperty[]
                        {
                                        new VertexMaterialData.FloatProperty()
                                        {
                                            Name = "Amount",
                                            Min = 0,
                                            Max = 1,
                                            Value = 1,
                                            PropertyMap = VertexMaterialData.FloatProperty.Mapping.TexcoordX
                                        }
                        }
                    },
                });

            AddIfNotPresent("Hue Saturation Brightness",
                (e) => new MaterialInfo()
                {
                    Material = Resources.Load<Material>("Materials/HueSaturationBrightness_" + e.ToString()),
                    Properties = new VertexMaterialData()
                    {
                        FloatProperties = new VertexMaterialData.FloatProperty[]
                        {
                            new VertexMaterialData.FloatProperty()
                            {
                                Name = "Hue",
                                Min = 0,
                                Max = 1,
                                Value = 0,
                                PropertyMap = VertexMaterialData.FloatProperty.Mapping.TexcoordX,
                            },
                            new VertexMaterialData.FloatProperty()
                            {
                                Name = "Saturation",
                                Value = 1,
                                PropertyMap = VertexMaterialData.FloatProperty.Mapping.TexcoordY,
                            },
                            new VertexMaterialData.FloatProperty()
                            {
                                Name = "Brightness",
                                Value = 1,
                                PropertyMap = VertexMaterialData.FloatProperty.Mapping.TangentW,
                            },
                        }
                    }
                });
            
            AddIfNotPresent("Color Overlay", 
                (e) => new MaterialInfo()
                {
                    Material = Resources.Load<Material>("Materials/ColorOverlay_" + e.ToString()),
                    Properties = new VertexMaterialData()
                    {
                        FloatProperties = new VertexMaterialData.FloatProperty[]
                        {
                            new VertexMaterialData.FloatProperty()
                            {
                                Name = "Opacity",
                                Min = 0,
                                Max = 1,
                                Value = 0,
                                PropertyMap = VertexMaterialData.FloatProperty.Mapping.TexcoordX
                            }
                        }
                    }
                });
        }

        void AddIfNotPresent(string name, Func<MaterialEffect, MaterialInfo> CreateMaterial, params MaterialEffect[] preservedLayerEffects)
        {
            foreach (var e in Enum.GetValues(typeof(MaterialEffect)))
            {
                var effect = (MaterialEffect)e;
                var info = GetMaterialInfo(name, effect);
                if (info == null)
                {
                    info = CreateMaterial(effect);

                    if (info.Material == null)
                    {
                        // on import it is too early to load materials so they are null.
                        // skip it in such case. It will be done when accessed next time.
                        continue;
                    }

                    info.Name = name;
                    info.Effect = effect;

                    materials.Add(info);
                }

                BlendMode srcMode, dstMode;
                float clipThreshold = 0.001f;
                bool clip = false;
                bool combineAlpha = false;

                switch (effect)
                {
                    case MaterialEffect.Normal:
                        srcMode = BlendMode.SrcAlpha;
                        dstMode = BlendMode.OneMinusSrcAlpha;
                        break;
                    case MaterialEffect.Additive:
                        srcMode = BlendMode.OneMinusDstColor;
                        dstMode = BlendMode.One;
                        combineAlpha = true;
                        break;
                    case MaterialEffect.LinearDodge:
                        srcMode = BlendMode.SrcAlpha | BlendMode.One;
                        dstMode = BlendMode.One | BlendMode.Zero;
                        combineAlpha = true;
                        clip = true;
                        break;
                    case MaterialEffect.Multiply:
                        srcMode = BlendMode.DstColor;
                        dstMode = BlendMode.Zero;
                        clip = true;
                        clipThreshold = 0.5f;
                        break;
                    case MaterialEffect.Overlay:
                        srcMode = BlendMode.DstAlpha;
                        dstMode = BlendMode.OneMinusDstColor;
                        clip = true;
                        clipThreshold = 0.5f;
                        break;
                    default: throw new ArgumentException();
                }

                info.Material.SetInt("SrcBlendMode", (int)srcMode);
                info.Material.SetInt("DstBlendMode", (int)dstMode);

                info.Material.SetFloat("ClipThreshold", clipThreshold);


#if (UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3) || UNITY_5_4
                // Unity 5.4+ is supported. Use older releases on your own risk.
                info.Material.SetInt("CombineAlpha", combineAlpha ? 1 : 0);
                info.Material.SetInt("ForceClip", clip ? 1 : 0);
#else // Unity 5.5+

                if (combineAlpha)
                    info.Material.EnableKeyword("COMBINE_ALPHA");
                else
                    info.Material.DisableKeyword("COMBINE_ALPHA");
                    
                if (clip)
                    info.Material.EnableKeyword("FORCE_CLIP");
                else
                    info.Material.DisableKeyword("FORCE_CLIP");
#endif

            }
        }

        IEnumerator SetTogglePropertyDelayed(Material material, string toggleName, bool toggle)
        {
            yield return null;
            material.SetInt(toggleName, (toggle) ? 1 : 0);
        }

        public MaterialInfo GetMaterialInfo(string name, MaterialEffect e)
        {
            MaterialInfo result = materials.FirstOrDefault((o) => o.Name == name && o.Effect == e);
            return result;
        }

        public Material GetMaterial(string name)
        {
            var m = materials.FirstOrDefault((o) => o.Name == name);

            if(m != null)
            {
                return m.Material;
            }

            return null;
        }

        public List<string> GetAllMaterialNames()
        {
            EnsurePredefinedMaterials();

            var list = new HashSet<string>(materials
                .Select(o => o.Name)).ToList();

            list.Sort((a, b) =>
            {
                if (materialOrder.Contains(a))
                {
                    if (materialOrder.Contains(b))
                        return materialOrder.IndexOf(a).CompareTo(materialOrder.IndexOf(b));
                    else
                        return -1;
                }
                else
                {
                    return 1;
                }
            });

            return list;
        }

        public HashSet<MaterialEffect> GetAllMaterialEffects(string name)
        {
            return new HashSet<MaterialEffect>(materials
                .Where(o => o.Name == name)
                .Select(o => o.Effect));
        }

#if UNITY_EDITOR
        public int GetMaterialInfoIndex(string name, MaterialEffect effect)
        {

            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].Name == name && materials[i].Effect == effect)
                    return i;
            }

            return -1;
        }
#endif
    }
}
