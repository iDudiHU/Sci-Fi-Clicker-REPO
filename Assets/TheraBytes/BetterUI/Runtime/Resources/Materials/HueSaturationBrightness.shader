// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BetterUI/HueSaturationBrightness"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
		
		SrcBlendMode ("SrcBlendMode", Float) = 5
		DstBlendMode ("DstBlendMode", Float) = 10
		
		[Toggle(COMBINE_ALPHA)] CombineAlpha("Combine Alpha", Float) = 0
		[Toggle(FORCE_CLIP)] ForceClip("Force Clip", Float) = 0
		ClipThreshold("Alpha Clip Threshold", Float) = 0.5
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}

		Stencil
	{
		Ref[_Stencil]
		Comp[_StencilComp]
		Pass[_StencilOp]
		ReadMask[_StencilReadMask]
		WriteMask[_StencilWriteMask]
	}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend [SrcBlendMode] [DstBlendMode]
		ColorMask[_ColorMask]

		Pass
	{
		Name "HueSaturationBrightness"
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0

#include "UnityCG.cginc"
#include "UnityUI.cginc"

#pragma multi_compile __ UNITY_UI_ALPHACLIP
#pragma multi_compile __ COMBINE_ALPHA
#pragma multi_compile __ FORCE_CLIP

	struct appdata_t
	{
		float4 vertex   : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
		float2 uv1		: TEXCOORD1;
		float4 tangent	: TANGENT;

#if UNITY_VERSION >= 550
		UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
	};

	struct v2f
	{
		float4 vertex			: SV_POSITION;
		fixed4 color			: COLOR;
		float2 texcoord			: TEXCOORD0;
		float4 worldPosition	: TEXCOORD1;
		float3 hsb				: TEXCOORD2;

#if UNITY_VERSION >= 550
		UNITY_VERTEX_OUTPUT_STEREO
#endif
	};

	fixed4 _TextureSampleAdd;
	float4 _ClipRect;

#if UNITY_VERSION < 550
	bool _UseClipRect;
	bool _UseAlphaClip;
	bool CombineAlpha;
	bool ForceClip;
#endif

	// VERTEX SHADER
	v2f vert(appdata_t IN)
	{
		v2f OUT;
#if UNITY_VERSION >= 550
		UNITY_SETUP_INSTANCE_ID(IN);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
#endif
		OUT.worldPosition = IN.vertex;

#if UNITY_VERSION >= 550
		OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
#else
		OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

		#ifdef UNITY_HALF_TEXEL_OFFSET
		OUT.vertex.xy += (_ScreenParams.zw - 1.0)*float2(-1, 1);
		#endif
#endif
		OUT.texcoord = IN.texcoord;

		OUT.color = IN.color;
		OUT.hsb = float3(IN.uv1.xy, IN.tangent.w);
		return OUT;
	}

	sampler2D _MainTex;
	float ClipThreshold;

	// RGB -> HSB
	float3 rgb2hsb(float3 c : COLOR)
	{
		float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
		float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
		float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

		float d = q.x - min(q.w, q.y);
		float e = 1.0e-10;

		return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
	}

	// HSB -> RGB
	float3 hsb2rgb(float3 c : COLOR)
	{
		float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
		float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);

		return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
	}

	// FRAGMENT SHADER
	fixed4 frag(v2f IN) : SV_Target
	{
		half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

#if UNITY_VERSION >= 550
#ifdef COMBINE_ALPHA
		color.rgb *= color.a;
#endif
#ifdef FORCE_CLIP
		clip(color.a - ClipThreshold);
#endif
#else
		if (CombineAlpha)
			color.rgb *= color.a;
			
		if (ForceClip)
			clip(color.a - ClipThreshold);
#endif

		float3 hsb = rgb2hsb(color.rgb);
		hsb.x = (hsb.x + IN.hsb.x) % 1.0;
		hsb.y *= IN.hsb.y;
		hsb.z *= IN.hsb.z;


		color.rgb = hsb2rgb(hsb);

		
#if UNITY_VERSION >= 550
		color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
#ifdef UNITY_UI_ALPHACLIP
		clip(color.a - 0.001);
#endif
#else
		if (_UseClipRect)
			color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

		if (_UseAlphaClip)
			clip(color.a - 0.001);
#endif

		return color;
	}


		ENDCG
	}
	}
}
