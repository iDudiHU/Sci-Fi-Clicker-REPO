using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterImage)), CanEditMultipleObjects]
    public class BetterImageEditor : GraphicEditor
    {
        private SerializedProperty m_FillMethod;
        private SerializedProperty m_FillOrigin;
        private SerializedProperty m_FillAmount;
        private SerializedProperty m_FillClockwise;
        private SerializedProperty m_Type;
        private SerializedProperty m_FillCenter;
        private SerializedProperty m_Sprite;
        private SerializedProperty m_PreserveAspect;
        private SerializedProperty m_useSpriteMesh;
        private SerializedProperty m_pixelPerUnitMultiplyer;
        private SerializedProperty m_maskable;

        private SerializedProperty spriteSetingsFallbackProp;
        private SerializedProperty spriteSetingsCollectionProp;

        private GUIContent m_SpriteContent;
        private GUIContent m_SpriteTypeContent;
        private GUIContent m_ClockwiseContent;

        private AnimBool m_ShowSlicedOrTiled;
        private AnimBool m_ShowSliced;
        private AnimBool m_ShowFilled;
        private AnimBool m_ShowType;
        
        ImageAppearanceProviderEditorHelper materialDrawer;
        BetterImage image;

        public override bool HasPreviewGUI()
        {
            image = this.target as BetterImage;
            if (image == null)
            {
                return false;
            }
            return image.sprite != null;
        }

        public override string GetInfoString()
        {
            image = this.target as BetterImage;
            Rect rect = image.rectTransform.rect;
            object num = Mathf.RoundToInt(Mathf.Abs(rect.width));
            Rect rect1 = image.rectTransform.rect;
            return string.Format("Image Size: {0}x{1}", num, Mathf.RoundToInt(Mathf.Abs(rect1.height)));
        }

        protected override void OnEnable()
        {
            image = target as BetterImage;
            this.materialDrawer = new ImageAppearanceProviderEditorHelper(base.serializedObject, image);

            base.OnEnable();
            this.m_SpriteContent = new GUIContent("Source Image");
            this.m_SpriteTypeContent = new GUIContent("Image Type");
            this.m_ClockwiseContent = new GUIContent("Clockwise");
            this.m_Sprite = base.serializedObject.FindProperty("m_Sprite");
            this.m_Type = base.serializedObject.FindProperty("m_Type");
            this.m_FillCenter = base.serializedObject.FindProperty("m_FillCenter");
            this.m_FillMethod = base.serializedObject.FindProperty("m_FillMethod");
            this.m_FillOrigin = base.serializedObject.FindProperty("m_FillOrigin");
            this.m_FillClockwise = base.serializedObject.FindProperty("m_FillClockwise");
            this.m_FillAmount = base.serializedObject.FindProperty("m_FillAmount");
            this.m_PreserveAspect = base.serializedObject.FindProperty("m_PreserveAspect");
            this.m_useSpriteMesh = base.serializedObject.FindProperty("m_UseSpriteMesh");
            this.m_pixelPerUnitMultiplyer = base.serializedObject.FindProperty("m_PixelsPerUnitMultiplier");
            this.m_maskable = base.serializedObject.FindProperty("m_Maskable");

            this.spriteSetingsFallbackProp = base.serializedObject.FindProperty("fallbackSpriteSettings");
            this.spriteSetingsCollectionProp = base.serializedObject.FindProperty("customSpriteSettings");


            this.m_ShowType = new AnimBool(this.m_Sprite.objectReferenceValue != null);
            this.m_ShowType.valueChanged.AddListener(new UnityAction(this.Repaint));
            Image.Type mType = (Image.Type)this.m_Type.enumValueIndex;
            this.m_ShowSlicedOrTiled = new AnimBool((this.m_Type.hasMultipleDifferentValues ? false : mType == Image.Type.Sliced));
            this.m_ShowSliced = new AnimBool((this.m_Type.hasMultipleDifferentValues ? false : mType == Image.Type.Sliced));
            this.m_ShowFilled = new AnimBool((this.m_Type.hasMultipleDifferentValues ? false : mType == Image.Type.Filled));
            this.m_ShowSlicedOrTiled.valueChanged.AddListener(new UnityAction(this.Repaint));
            this.m_ShowSliced.valueChanged.AddListener(new UnityAction(this.Repaint));
            this.m_ShowFilled.valueChanged.AddListener(new UnityAction(this.Repaint));
            this.SetShowNativeSize(true);
            
        }

        private void SetShowNativeSize(bool instant)
        {
            Image.Type mType = (Image.Type)this.m_Type.enumValueIndex;
            base.SetShowNativeSize((mType == Image.Type.Simple ? true : mType == Image.Type.Filled), instant);
        }


        public override void OnInspectorGUI()
        {
            base.serializedObject.Update();

            ScreenConfigConnectionHelper.DrawGui("Sprite Settings", spriteSetingsCollectionProp, ref spriteSetingsFallbackProp, DrawSpriteSettings);
            

            EditorGUILayout.Separator();
            if (image.type == Image.Type.Filled)
            {
                // materials not (yet) supported for filled images
                EditorGUILayout.PropertyField(this.m_Material);
            }
            else
            {
                // draw color and material
                materialDrawer.DrawMaterialGui(base.m_Material);
            }
            EditorGUILayout.Separator();


            base.RaycastControlsGUI();
            this.m_ShowType.target = this.m_Sprite.objectReferenceValue != null;
            if (EditorGUILayout.BeginFadeGroup(this.m_ShowType.faded))
            {
                this.TypeGUI();
            }
            EditorGUILayout.EndFadeGroup();
            this.SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(this.m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
                if (m_useSpriteMesh != null)
                {
                    EditorGUILayout.PropertyField(this.m_useSpriteMesh, new GUILayoutOption[0]);
                }
                EditorGUILayout.PropertyField(this.m_PreserveAspect, new GUILayoutOption[0]);
                EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
            }
            EditorGUILayout.EndFadeGroup();
            base.NativeSizeButtonGUI();
            base.serializedObject.ApplyModifiedProperties();

            if (image.type == UnityEngine.UI.Image.Type.Sliced)
            {

                var prop = serializedObject.FindProperty("keepBorderAspectRatio");
                EditorGUILayout.PropertyField(prop);
                serializedObject.ApplyModifiedProperties();
            }

            if(image.type == Image.Type.Sliced || image.type == Image.Type.Tiled)
            {
                var prop = serializedObject.FindProperty("spriteBorderScaleFallback");
                var collection = serializedObject.FindProperty("customBorderScales");
                //EditorGUILayout.PropertyField(prop);

                ScreenConfigConnectionHelper.DrawSizerGui("Border Scale", collection, ref prop);
                serializedObject.ApplyModifiedProperties();
            }

            if (m_maskable != null)
            {
                EditorGUILayout.PropertyField(m_maskable);
                base.serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawSpriteSettings(string configName, SerializedProperty property)
        {
            SerializedProperty spriteProp = property.FindPropertyRelative("Sprite");
            SerializedProperty primeColorProp = property.FindPropertyRelative("PrimaryColor");

            SpriteGUI(spriteProp);

            // coloring not supported for Filled yet.
            if (image.type != Image.Type.Filled)
            {
                SerializedProperty colorModeProp = property.FindPropertyRelative("ColorMode");
                SerializedProperty secondColorProp = property.FindPropertyRelative("SecondaryColor");
                ImageAppearanceProviderEditorHelper.DrawColorGui(colorModeProp, primeColorProp, secondColorProp);
            }
            else
            {
                EditorGUILayout.PropertyField(primeColorProp);
            }
        }

        /// <summary>
        ///   <para>Custom preview for Image component.</para>
        /// </summary>
        /// <param name="rect">Rectangle in which to draw the preview.</param>
        /// <param name="background">Background image.</param>
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Image image = this.target as Image;
                if (image == null)
                {
                    return;
                }
                Sprite sprite = image.sprite;
                if (sprite == null)
                {
                    return;
                }
                
                Texture2D preview = AssetPreview.GetAssetPreview(sprite);
                EditorGUI.DrawTextureTransparent(rect, preview, ScaleMode.ScaleToFit, 1f);
            }
        }
        

        /// <summary>
        ///   <para>GUI for showing the Sprite property.</para>
        /// </summary>
        protected void SpriteGUI(SerializedProperty spriteProp)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(spriteProp, this.m_SpriteContent, new GUILayoutOption[0]);
            if (EditorGUI.EndChangeCheck())
            {
                Sprite mSprite = spriteProp.objectReferenceValue as Sprite;
                if (mSprite)
                {
                    Image.Type mType = (Image.Type)this.m_Type.enumValueIndex;
                    if (mSprite.border.SqrMagnitude() > 0f)
                    {
                        this.m_Type.enumValueIndex = 1;
                    }
                    else if (mType == Image.Type.Sliced)
                    {
                        this.m_Type.enumValueIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        ///   <para>GUI for showing the image type and associated settings.</para>
        /// </summary>
        protected void TypeGUI()
        {
            bool flag;
            EditorGUILayout.PropertyField(this.m_Type, this.m_SpriteTypeContent, new GUILayoutOption[0]);
            EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
            Image.Type mType = (Image.Type)this.m_Type.enumValueIndex;
            if (this.m_Type.hasMultipleDifferentValues)
            {
                flag = false;
            }
            else
            {
                flag = (mType == Image.Type.Sliced ? true : mType == Image.Type.Tiled);
            }
            bool flag1 = flag;
            if (flag1 && (int)base.targets.Length > 1)
            {
                flag1 = (
                    from obj in (IEnumerable<UnityEngine.Object>)base.targets
                    select obj as Image).All<Image>((Image img) => img.hasBorder);
            }
            this.m_ShowSlicedOrTiled.target = flag1;
            this.m_ShowSliced.target = (!flag1 || this.m_Type.hasMultipleDifferentValues ? false : mType == Image.Type.Sliced);
            this.m_ShowFilled.target = (this.m_Type.hasMultipleDifferentValues ? false : mType == Image.Type.Filled);
            Image image = this.target as Image;
            if (EditorGUILayout.BeginFadeGroup(this.m_ShowSlicedOrTiled.faded) && image.hasBorder)
            {
                EditorGUILayout.PropertyField(this.m_FillCenter, new GUILayoutOption[0]);

                if (m_pixelPerUnitMultiplyer != null)
                {
                    EditorGUILayout.PropertyField(this.m_pixelPerUnitMultiplyer, new GUILayoutOption[0]);
                }
            }
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(this.m_ShowSliced.faded) && image.sprite != null && !image.hasBorder)
            {
                EditorGUILayout.HelpBox("This Image doesn't have a border.", MessageType.Warning);
            }
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(this.m_ShowFilled.faded))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(this.m_FillMethod, new GUILayoutOption[0]);
                if (EditorGUI.EndChangeCheck())
                {
                    this.m_FillOrigin.intValue = 0;
                }
                switch (this.m_FillMethod.enumValueIndex)
                {
                    case 0:
                        {
                            this.m_FillOrigin.intValue = (int)((Image.OriginHorizontal)EditorGUILayout.EnumPopup("Fill Origin", (Image.OriginHorizontal)this.m_FillOrigin.intValue, new GUILayoutOption[0]));
                            break;
                        }
                    case 1:
                        {
                            this.m_FillOrigin.intValue = (int)((Image.OriginVertical)EditorGUILayout.EnumPopup("Fill Origin", (Image.OriginVertical)this.m_FillOrigin.intValue, new GUILayoutOption[0]));
                            break;
                        }
                    case 2:
                        {
                            this.m_FillOrigin.intValue = (int)((Image.Origin90)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin90)this.m_FillOrigin.intValue, new GUILayoutOption[0]));
                            break;
                        }
                    case 3:
                        {
                            this.m_FillOrigin.intValue = (int)((Image.Origin180)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin180)this.m_FillOrigin.intValue, new GUILayoutOption[0]));
                            break;
                        }
                    case 4:
                        {
                            this.m_FillOrigin.intValue = (int)((Image.Origin360)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin360)this.m_FillOrigin.intValue, new GUILayoutOption[0]));
                            break;
                        }
                }
                EditorGUILayout.PropertyField(this.m_FillAmount, new GUILayoutOption[0]);
                if (this.m_FillMethod.enumValueIndex > 1)
                {
                    EditorGUILayout.PropertyField(this.m_FillClockwise, this.m_ClockwiseContent, new GUILayoutOption[0]);
                }
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
        }

        [MenuItem("CONTEXT/Image/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            Image img = command.context as Image;
            var newImg = Betterizer.MakeBetter<Image, BetterImage>(img);
            var sprite = img.sprite;
            var col = img.color;
            
            if(newImg != null)
            {
                newImg.SetAllDirty();

                BetterImage better = newImg as BetterImage;
                if(better != null)
                {
                    // set border scale both to height as default to make default scale uniform.
                    better.SpriteBorderScale.ModX.SizeModifiers[0].Mode = ImpactMode.PixelHeight;
                    better.SpriteBorderScale.ModY.SizeModifiers[0].Mode = ImpactMode.PixelHeight;

                    better.CurrentSpriteSettings.Sprite = sprite;
                    better.CurrentSpriteSettings.PrimaryColor = col;

#if !UNITY_4 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3 && !UNITY_5_4 && !UNITY_5_5  // from UNITY 5.6 on
                    // ensure shader channels in canvas
                    Canvas canvas = better.transform.GetComponentInParent<Canvas>();
                    canvas.additionalShaderChannels = canvas.additionalShaderChannels 
                        | AdditionalCanvasShaderChannels.TexCoord1 
                        | AdditionalCanvasShaderChannels.Tangent;
#endif
                }

            }
        }

    }
}
