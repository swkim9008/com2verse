// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Editor/UnityAssetTool/MR/UI/Default Cutout"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        _MaskTex("Mask Texture", 2D) = "white" {}
        _Range("Mask Range", Range(-1, 1)) = 0
        _ClipRange("Clip Range", Range(0, 1)) = 0.5

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
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
            Blend Off
            ColorMask[_ColorMask]

            Pass
            {
                Name "Default"
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"
                #include "UnityUI.cginc"

                #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
                #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                    float2 texcoord1 : TEXCOORD1;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord  : TEXCOORD0;
                    float2 texcoord1  : TEXCOORD1;
                    float4 worldPosition : TEXCOORD2;
                    
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                CBUFFER_START(UnityPerMaterial)	//SRP Batcher를 위해 변수에 CBUFFER관련 옵션 추가
                sampler2D _MainTex;
                fixed4 _Color;
                
                float4 _ClipRect;
                float4 _MainTex_ST;
                
                sampler2D _MaskTex;
                float4  _MaskTex_ST;
                float _Range;
                float _ClipRange;
                CBUFFER_END

                fixed4 _TextureSampleAdd;


                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    OUT.worldPosition = v.vertex;
                    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    OUT.texcoord1 = TRANSFORM_TEX(v.texcoord1, _MaskTex);

                    OUT.color = v.color * _Color;
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                    color.a = tex2D(_MaskTex, IN.texcoord1).r + _Range;
                    clip(color.a - _ClipRange);
                    
                    // 블렌드를 하려면 아래와 같이 Alpha 값을 복구해야된다.
                    // (Blend Off 일 경우에는 없어도 됨)
                    color.a -= _Range;

                    return color;
                }
            ENDCG
            }
        }
}
