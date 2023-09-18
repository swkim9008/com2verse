
#ifndef URP_UNLIT_FORWARD_PASS_INCLUDED
#define URP_UNLIT_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Unlit.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;

    #if defined(DEBUG_DISPLAY)
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    #endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float fogCoord : TEXCOORD1;
    float4 positionCS : SV_POSITION;

    #if defined(DEBUG_DISPLAY) || defined(_DEBUG_DISPLAY)
    float3 positionWS : TEXCOORD2;
    float3 normalWS : TEXCOORD3;
    float3 viewDirWS : TEXCOORD4;
    #endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, out InputData inputData)
{
    inputData = (InputData)0;

    #if defined(DEBUG_DISPLAY)
    inputData.positionWS = input.positionWS;
    inputData.normalWS = input.normalWS;
    inputData.viewDirectionWS = input.viewDirWS;
    #else
    inputData.positionWS = float3(0, 0, 0);
    inputData.normalWS = half3(0, 0, 1);
    inputData.viewDirectionWS = half3(0, 0, 1);
    #endif
    inputData.shadowCoord = 0;
    inputData.fogCoord = 0;
    inputData.vertexLighting = half3(0, 0, 0);
    inputData.bakedGI = half3(0, 0, 0);
    inputData.normalizedScreenSpaceUV = 0;
    inputData.shadowMask = half4(1, 1, 1, 1);
}

Varyings UnlitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    output.positionCS = vertexInput.positionCS;
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    #if defined(_FOG_FRAGMENT)
    output.fogCoord = vertexInput.positionVS.z;
    #else
    output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    #if defined(DEBUG_DISPLAY)
    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

    // already normalized from normal transform to WS.
    output.positionWS = vertexInput.positionWS;
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
    #endif

    return output;
}

int _debugChannel = 0;

#define _ALBEDO_CHANNEL 0
#define _NORMAL_TS_CHANNEL 1
#define _NORMAL_WS_CHANNEL 2
#define _OCCLUSION_CHANNEL 3
#define _SPECULAR_CHANNEL 4
#define _SMOOTHNESS_CHANNEL 5
#define _METALLIC_CHANNEL 6
#define _ALPHA_CHANNEL 7
#define _EMISSIVE_CHANNEL 8
#define _SSS_CHANNEL 9
#define _GI_CHANNEL 10
#define _BRDF_ENV_CHANNEL 11
#define _BRDF_DIFFUSE 12
#define _VTX_COLOR_CHANNEL 13
#define _UV_CHANNEL 14
#define _VERT_NORMAL 20
#define _FWD_SHADOW_CHANNEL 23
#define _FWD_SHADOW_MASK_CHANNEL 24
#define _FWD_MAIN_LIGHT_SHADOW_ATT_CHANNEL 25
#define _FWD_REALTIME_SHADOW_CHANNEL 26
#define _FWD_BAKED_SHADOW_CHANNEL 27
#define _DEPTH_CHANNEL 28
#define _REFLECTION_CHANNEL 29

// private:
half4 _DEBUG_OUTPUT(half4 lit,
                    Varyings input, SurfaceData surfaceData, InputData inputData,
                    half3 sss = half3(0.0f, 0.0f, 0.0f))
{
#if _DEBUG_DISPLAY
    if (_debugChannel == _ALBEDO_CHANNEL) // albedo
    {
        return half4(surfaceData.albedo, surfaceData.alpha);
    }
    if (_debugChannel == _NORMAL_TS_CHANNEL) // normal ts
    {
        return half4(surfaceData.normalTS, 1.0f);
    }
    if (_debugChannel == _NORMAL_WS_CHANNEL) // normal ws
    {
        return half4(normalize(inputData.normalWS * 2.0f - 1.0f), 1.0f);
    }
    if (_debugChannel == _OCCLUSION_CHANNEL) // occlusion
    {
        return half4(surfaceData.occlusion.xxx, surfaceData.alpha);
    }
    if (_debugChannel == _SPECULAR_CHANNEL) // specular
    {
        return half4(surfaceData.specular, surfaceData.alpha);
    } 
    if (_debugChannel == _SMOOTHNESS_CHANNEL) // smoothness
    {
        return half4(surfaceData.smoothness.xxx, surfaceData.alpha);
    }
    if (_debugChannel == _METALLIC_CHANNEL) // metallic
    {
        return half4(surfaceData.metallic.xxx, surfaceData.alpha);
    }
    if (_debugChannel == _ALPHA_CHANNEL) // alpha
    {
        return half4(surfaceData.alpha.xxx, surfaceData.alpha);
    }
    if (_debugChannel == _EMISSIVE_CHANNEL) // emissive
    {
        return half4(surfaceData.emission, surfaceData.alpha);
    }
    if (_debugChannel == _SSS_CHANNEL) // sss
    {
        return half4(sss, surfaceData.alpha);
    }
    if (_debugChannel == _GI_CHANNEL) // gi
    {
        return half4(inputData.bakedGI.xyz, surfaceData.alpha);
    }
    if (_debugChannel == _BRDF_ENV_CHANNEL) // brdf environment
    {
        const half oneMinusReflectivity = OneMinusReflectivityMetallic(surfaceData.metallic);
        const half reflectivity = 1.0h - oneMinusReflectivity;
        
        #ifdef _SPECULAR_SETUP
        half3 brdfDiffuse = surfaceData.albedo * (half3(1.0h, 1.0h, 1.0h) - surfaceData.specular);
        #else
        half3 brdfDiffuse = surfaceData.albedo * oneMinusReflectivity;
        #endif

        half3 irradiance;
        const half3 v = inputData.viewDirectionWS;
        const half3 n = inputData.normalWS;
        half3 reflectVector = reflect(-v, n);
        const half ndv = dot(n, v);
        const half perceptualRoughness = 1.0h - surfaceData.smoothness;

        #ifdef _REFLECTION_PROBE_BLENDING
        irradiance = CalculateIrradianceFromReflectionProbes(reflectVector, inputData.positionWS, perceptualRoughness);
        #else
        #ifdef _REFLECTION_PROBE_BOX_PROJECTION
        reflectVector = BoxProjectedCubemapDirection(reflectVector, inputData.positionWS, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
        #endif
        const half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);

        const half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
        irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
        #endif
    
        const half3 indirectSpecular = irradiance * surfaceData.occlusion;
        const half r = 1.0h - surfaceData.smoothness;
        const float surfaceReduction = 1.0h / (r * r + 1.0h);
        const half3 envBrdfSpec = half3(surfaceReduction * lerp(surfaceData.specular, surfaceData.smoothness + reflectivity, Pow4(1.0h - ndv)));
    
        half3 c = brdfDiffuse * indirectSpecular * envBrdfSpec * surfaceData.occlusion;
        return half4(c.xyz, surfaceData.alpha);
    }
    if (_debugChannel == _BRDF_DIFFUSE) // brdf diffuse
    {
        const half oneMinusReflectivity = OneMinusReflectivityMetallic(surfaceData.metallic);
        
        #ifdef _SPECULAR_SETUP
        half3 brdfDiffuse = surfaceData.albedo * (half3(1.0h, 1.0h, 1.0h) - surfaceData.specular);
        #else
        half3 brdfDiffuse = surfaceData.albedo * oneMinusReflectivity;
        #endif

        brdfDiffuse *= surfaceData.occlusion * inputData.bakedGI;
        return half4(brdfDiffuse.xyz, surfaceData.alpha);
    }
    if (_debugChannel == _VTX_COLOR_CHANNEL) // vertex color
    {
        return half4(0.0h, 0.0h, 0.0h, surfaceData.alpha);
    }
    if (_debugChannel == _UV_CHANNEL + 0) // vertex uv
    {
        return half4(input.uv.xy, (1.0f - dot(input.uv.xy, input.uv.xy)), surfaceData.alpha);
    }
    if (_debugChannel == _UV_CHANNEL + 1) // vertex uv 2
    {
        return half4(input.uv.xy, (1.0f - dot(input.uv.xy, input.uv.xy)), surfaceData.alpha);
    }
    if (_debugChannel == _UV_CHANNEL + 2) // vertex uv 3
    {
        return half4(input.uv.xy, (1.0f - dot(input.uv.xy, input.uv.xy)), surfaceData.alpha);
    }
    if (_debugChannel == _UV_CHANNEL + 3) // vertex uv 4
    {
        return half4(input.uv.xy, (1.0f - dot(input.uv.xy, input.uv.xy)), surfaceData.alpha);
    }
    if (_debugChannel == _UV_CHANNEL + 4) // vertex uv 5
    {
        return half4(input.uv.xy, (1.0f - dot(input.uv.xy, input.uv.xy)), surfaceData.alpha);
    }
    if (_debugChannel == _UV_CHANNEL + 5) // vertex uv 5
    {
        return half4(input.uv.xy, (1.0f - dot(input.uv.xy, input.uv.xy)), surfaceData.alpha);
    }
    if (_debugChannel == _VERT_NORMAL + 0) // vertex normal
    {
        return half4(input.normalWS.xyz * 2.0f - 1.0f, surfaceData.alpha);
    }
    if (_debugChannel == _VERT_NORMAL + 1) // vertex tangent
    {
        #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
        return half4(input.tangentWS.xyz * 2.0f - 1.0f, surfaceData.alpha);
        #else
        return half4(input.normalWS.xyz * 2.0f - 1.0f, surfaceData.alpha);
        #endif
    }
    if (_debugChannel == _VERT_NORMAL + 2) // vertex bitangent
    {
        #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
        const half3 n = input.normalWS.xyz;
        const half3 t = input.tangentWS.xyz;
        const half3 b = cross(n, t);
        return half4(b * 2.0f - 1.0f, surfaceData.alpha);
        #else
        return half4(input.normalWS.xyz * 2.0f - 1.0f, surfaceData.alpha);
    #endif
    }
    if (_debugChannel == _FWD_SHADOW_CHANNEL) // shadow
    {
        const half4 shadowMask = CalculateShadowMask(inputData);
        const AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
        Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
        const float ndl = saturate(dot(mainLight.direction.xyz, inputData.normalWS.xyz));
        return half4(ndl * mainLight.distanceAttenuation.xxx * mainLight.shadowAttenuation.xxx, surfaceData.alpha);
    }
    if (_debugChannel == _FWD_SHADOW_MASK_CHANNEL) // shadow
    {
        const half4 shadowMask = CalculateShadowMask(inputData);
        return shadowMask;
    }
    if (_debugChannel == _FWD_MAIN_LIGHT_SHADOW_ATT_CHANNEL) // shadow
    {
        const half4 shadowMask = CalculateShadowMask(inputData);
        const AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
        Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
        return half4(mainLight.shadowAttenuation.xxx, surfaceData.alpha);
    }
    if (_debugChannel == _FWD_REALTIME_SHADOW_CHANNEL) // shadow
    {
        half realtimeShadow = MainLightRealtimeShadow(inputData.shadowCoord);
        return half4(realtimeShadow, realtimeShadow, realtimeShadow, surfaceData.alpha);
    }
    if (_debugChannel == _FWD_BAKED_SHADOW_CHANNEL) // shadow
    {
        const half4 shadowMask = CalculateShadowMask(inputData);
        
        #ifdef CALCULATE_BAKED_SHADOWS
        half bakedShadow = BakedShadow(shadowMask, _MainLightOcclusionProbes);
        #else
        half bakedShadow = half(1.0);
        #endif
        
        return half4(bakedShadow, bakedShadow, bakedShadow, surfaceData.alpha);
    }
    if (_debugChannel == _DEPTH_CHANNEL) // depth
    {
        float pixelDepth = input.positionCS.z / input.positionCS.w;
        return half4(pixelDepth.xxx * 1000.0h, surfaceData.alpha);
    }
    if (_debugChannel == _REFLECTION_CHANNEL) // reflection
    {
        const half3 reflectVector = reflect(-inputData.viewDirectionWS.xyz, inputData.normalWS.xyz);
        const float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surfaceData.smoothness);
        const half3 reflection = GlossyEnvironmentReflection(reflectVector, input.positionWS.xyz, perceptualRoughness, 1.0h);
        return half4(reflection, 1.0h);
    }
    clip(-1);
#endif
    return lit;
}

half4 DEBUG_RETURN(
    half4 lit,
    Varyings input, SurfaceData surfaceData, InputData inputData,
    half3 sss = half3(0.0f, 0.0f, 0.0f)
)
{
    #ifdef _DEBUG_DISPLAY
    return _DEBUG_OUTPUT(lit, input, surfaceData, inputData, sss);
    #endif

    return lit;
}

half4 UnlitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half2 uv = input.uv;
    half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    half3 color = texColor.rgb * _BaseColor.rgb;
    half alpha = texColor.a * _BaseColor.a;

    AlphaDiscard(alpha, _Cutoff);

    #if defined(_ALPHAPREMULTIPLY_ON)
    color *= alpha;
    #endif

    InputData inputData;
    InitializeInputData(input, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

#ifdef _DBUFFER
    ApplyDecalToBaseColor(input.positionCS, color);
#endif

    #if defined(_FOG_FRAGMENT)
        #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
        float viewZ = -input.fogCoord;
        float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
        half fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
        #else
        half fogFactor = 0;
        #endif
    #else
    half fogFactor = input.fogCoord;
    #endif
    half4 finalColor = UniversalFragmentUnlit(inputData, color, alpha);

#if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
    float2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUV);
    finalColor.rgb *= aoFactor.directAmbientOcclusion;
#endif

    finalColor.rgb = MixFog(finalColor.rgb, fogFactor);

    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.emission = color.xyz;
    surfaceData.albedo = half3(0.0h, 0.0h, 0.0h);
    surfaceData.normalTS = half3(0.0h, 0.0h, 1.0h);
    surfaceData.smoothness = 0.0h;
    surfaceData.specular = half3(0.0f, 0.0f, 0.0f);
    surfaceData.occlusion = 1.0h;
    surfaceData.metallic = 0.0h;
    inputData.bakedGI = half3(0.0f, 0.0f, 0.0f);
    inputData.vertexLighting = half3(0.0f, 0.0f, 0.0f);
    
    return DEBUG_RETURN(finalColor, input, surfaceData, inputData);
}

#endif
