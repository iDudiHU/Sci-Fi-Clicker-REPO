using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterRawImage)), CanEditMultipleObjects]
    public class BetterRawImageEditor : GraphicEditor
    {
        private SerializedProperty m_Texture;
        private GUIContent m_UVRectContent;

        private SerializedProperty textureSetingsFallbackProp;
        private SerializedProperty textureSetingsCollectionProp;
        private SerializedProperty maskable;

        ImageAppearanceProviderEditorHelper materialDrawer;

        /// <summary>
        ///   <para>A string cointaining the Image details to be used as a overlay on the component Preview.</para>
        /// </summary>
        /// <returns>
        ///   <para>The RawImage details.</para>
        /// </returns>
        public override string GetInfoString()
        {
            BetterRawImage rawImage = this.target as BetterRawImage;
            Rect rect = rawImage.rectTransform.rect;
            object num = Mathf.RoundToInt(Mathf.Abs(rect.width));
            Rect rect1 = rawImage.rectTransform.rect;
            string str = string.Format("RawImage Size: {0}x{1}", num, Mathf.RoundToInt(Mathf.Abs(rect1.height)));
            return str;
        }



        /// <summary>
        ///   <para>Can this component be Previewed in its current state?</para>
        /// </summary>
        /// <returns>
        ///   <para>True if this component can be Previewed in its current state.</para>
        /// </returns>
        public override bool HasPreviewGUI()
        {
            BetterRawImage rawImage = this.target as BetterRawImage;
            if (rawImage == null)
            {
                return false;
            }
            Rect rect = BetterRawImageEditor.Outer(rawImage);
            return (rect.width <= 0f ? false : rect.height > 0f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            BetterRawImage img = this.target as BetterRawImage;
            this.materialDrawer = new ImageAppearanceProviderEditorHelper(base.serializedObject, img);

            this.m_UVRectContent = new GUIContent("UV Rect");
            this.m_Texture = base.serializedObject.FindProperty("m_Texture");

            this.textureSetingsFallbackProp = base.serializedObject.FindProperty("fallbackTextureSettings");
            this.textureSetingsCollectionProp = base.serializedObject.FindProperty("customTextureSettings");
            this.maskable = base.serializedObject.FindProperty("m_Maskable");

            this.SetShowNativeSize(true);
        }

        /// <summary>
        ///   <para>Implement specific RawImage inspector GUI code here. If you want to simply extend the existing editor call the base OnInspectorGUI () before doing any custom GUI code.</para>
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.serializedObject.Update();

            ScreenConfigConnectionHelper.DrawGui("Texture Settings", textureSetingsCollectionProp, ref textureSetingsFallbackProp, DrawTextureSettings);

            //base.AppearanceControlsGUI();
            materialDrawer.DrawMaterialGui(base.m_Material);

            base.RaycastControlsGUI();
            this.SetShowNativeSize(false);
            base.NativeSizeButtonGUI();

            if (maskable != null)
            {
                EditorGUILayout.PropertyField(maskable);
            }

            base.serializedObject.ApplyModifiedProperties();
        }

        private void DrawTextureSettings(string configName, SerializedProperty property)
        {
            SerializedProperty textureProp = property.FindPropertyRelative("Texture");

            EditorGUILayout.PropertyField(textureProp);

            SerializedProperty primeColorProp = property.FindPropertyRelative("PrimaryColor");
            SerializedProperty colorModeProp = property.FindPropertyRelative("ColorMode");
            SerializedProperty secondColorProp = property.FindPropertyRelative("SecondaryColor");
            
            ImageAppearanceProviderEditorHelper.DrawColorGui(colorModeProp, primeColorProp, secondColorProp);

            SerializedProperty uvProp = property.FindPropertyRelative("UvRect");

            EditorGUILayout.PropertyField(uvProp, m_UVRectContent);
        }

        private struct IntRect
        {
            public int x, y, width, height;

            public override string ToString()
            {
                return "x: " + x + "\ty: " + y + "\twidth: " + width + "\theight: " + height;
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
                BetterRawImage betterRawImage = this.target as BetterRawImage;
                if (betterRawImage == null || betterRawImage.texture == null)
                {
                    return;
                }

                Texture rawTexture = betterRawImage.mainTexture;
                Rect uvRect = betterRawImage.uvRect;

                // Convert texture into readable texture2D
                RenderTexture previousRenderTexture = RenderTexture.active;
                
                RenderTexture tmp = RenderTexture.GetTemporary(rawTexture.width, rawTexture.height);
                Graphics.Blit(rawTexture, tmp);
                
                Texture2D rawTexture2D = new Texture2D(rawTexture.width, rawTexture.height);
                rawTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                rawTexture2D.Apply();

                RenderTexture.active = previousRenderTexture;
                tmp.Release();

                // Fun calculations
                IntRect baseRect;
                baseRect.width = Mathf.Clamp((int) (rawTexture.width * uvRect.width), 0, rawTexture.width);
                baseRect.height = Mathf.Clamp((int) (rawTexture.height * uvRect.height), 0, rawTexture.height);
                baseRect.x = (int)(rawTexture.width * uvRect.x);
                baseRect.y = (int)(rawTexture.height * uvRect.y);

                IntRect textureCoordinates;
                textureCoordinates.width = Mathf.Clamp(baseRect.width, 0, rawTexture2D.width - baseRect.x);
                textureCoordinates.height = Mathf.Clamp(baseRect.height, 0, rawTexture2D.height - baseRect.y);
                textureCoordinates.x = Mathf.Clamp(baseRect.x, 0, rawTexture2D.width);
                textureCoordinates.y = Mathf.Clamp(baseRect.y, 0, rawTexture2D.height);

                int previewWidth = (int) Mathf.Abs(rawTexture.width * uvRect.width);
                int previewHeight = (int) Mathf.Abs(rawTexture.height * uvRect.height);

                IntRect drawCoordinates;
                drawCoordinates.x = uvRect.x > 0 ? 0 : (int)(textureCoordinates.width * Mathf.Abs(uvRect.x));
                drawCoordinates.y = uvRect.y > 0 ? 0 : (int)(textureCoordinates.height * Mathf.Abs(uvRect.y));
                drawCoordinates.width = Mathf.Clamp(textureCoordinates.width, 0, previewWidth - drawCoordinates.x);
                drawCoordinates.height = Mathf.Clamp(textureCoordinates.height, 0, previewHeight - drawCoordinates.y);

                // fix cutoffs
                if (textureCoordinates.width > drawCoordinates.width)
                {
                    textureCoordinates.width = drawCoordinates.width;
                }
                if (textureCoordinates.height > drawCoordinates.height)
                {
                    textureCoordinates.height = drawCoordinates.height;
                }

                // Create actual Preview
                Texture2D preview = new Texture2D(previewWidth, previewHeight);
                if (drawCoordinates.width > 0 && drawCoordinates.height > 0)
                {
                    Color[] pixels = rawTexture2D.GetPixels(textureCoordinates.x, textureCoordinates.y, textureCoordinates.width, textureCoordinates.height);
                    preview.SetPixels(drawCoordinates.x, drawCoordinates.y, drawCoordinates.width, drawCoordinates.height, pixels);
                    preview.Apply(true, false);
                }

                EditorGUI.DrawTextureTransparent(rect, preview, ScaleMode.ScaleToFit);
            }
        }

        private static Rect Outer(BetterRawImage rawImage)
        {
            Rect rect = rawImage.uvRect;
            rect.xMin = rect.xMin * rawImage.rectTransform.rect.width;
            rect.xMax = rect.xMax * rawImage.rectTransform.rect.width;
            rect.yMin = rect.yMin * rawImage.rectTransform.rect.height;
            rect.yMax = rect.yMax * rawImage.rectTransform.rect.height;
            return rect;
        }

        private void SetShowNativeSize(bool instant)
        {
            base.SetShowNativeSize(this.m_Texture.objectReferenceValue != null, instant);
        }

        [MenuItem("CONTEXT/RawImage/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            RawImage img = command.context as RawImage;
            var texture = img.texture;
            var col = img.color;
            var uv = img.uvRect;

            RawImage newImg = Betterizer.MakeBetter<RawImage, BetterRawImage>(img);
            BetterRawImage better = newImg as BetterRawImage;

            if (better != null)
            {
                better.CurrentTextureSettings.Texture = texture;
                better.CurrentTextureSettings.PrimaryColor = col;
                better.CurrentTextureSettings.UvRect = uv;

#if !UNITY_4 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3 && !UNITY_5_4 && !UNITY_5_5 // from UNITY 5.6 on
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
