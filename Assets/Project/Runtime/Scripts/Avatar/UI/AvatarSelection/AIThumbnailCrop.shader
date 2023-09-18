Shader "Com2Verse/UI/AIThumbnailCrop(UI)"
{
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    struct Attributes
    {
        float4 position : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    cbuffer UnityPerMaterial
    {
        half2 _InputResolution;
        half2 _OutputResolution;
    }

    Texture2D<half4> _InputImage;
    SamplerState bilinear_clamp_sampler;

    Varyings Vert(Attributes input)
    {
        const VertexPositionInputs positionInputs = GetVertexPositionInputs(input.position.xyz);

        const float2 d = (_InputResolution.xy - _OutputResolution.xy) / _InputResolution.xy;
        const float2 uv = input.uv.xy * (1.0f - d) + 0.5f * d;

        Varyings output;
        output.positionCS = positionInputs.positionCS;
        output.uv = uv;
        return output;
    }

    half4 Frag(Varyings input) : SV_Target
    {
        return _InputImage.Sample(bilinear_clamp_sampler, input.uv.xy);
    }
    ENDHLSL

    Properties
    {
        _InputImage ("", 2D) = "white" {}
        _InputResolution ("", Vector) = (1, 1, 0, 0)
        _OutputResolution ("", Vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}