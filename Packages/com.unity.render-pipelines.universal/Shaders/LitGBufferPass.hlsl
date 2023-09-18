#ifndef UNIVERSAL_LIT_GBUFFER_PASS_INCLUDED
#define UNIVERSAL_LIT_GBUFFER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

// TODO: Currently we support viewDirTS caclulated in vertex shader and in fragments shader.
// As both solutions have their advantages and disadvantages (etc. shader target 2.0 has only 8 interpolators).
// We need to find out if we can stick to one solution, which we needs testing.
// So keeping this until I get manaul QA pass.
#if defined(_PARALLAXMAP) && (SHADER_TARGET >= 30)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

// keep this file in sync with LitForwardPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1;
    float2 dynamicLightmapUV  : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD1;
#endif

    half3 normalWS                  : TEXCOORD2;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    half4 tangentWS                 : TEXCOORD3;    // xyz: tangent, w: sign
#endif
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half3 vertexLighting            : TEXCOORD4;    // xyz: vertex lighting
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD5;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS                 : TEXCOORD6;
#endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);
#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV       : TEXCOORD8; // Dynamic lightmap UVs
#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        inputData.positionWS = input.positionWS;
    #endif

    inputData.positionCS = input.positionCS;
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    #if defined(_NORMALMAP) || defined(_DETAIL)
        float sgn = input.tangentWS.w;      // should be either +1 or -1
        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
        inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
    #else
        inputData.normalWS = input.normalWS;
    #endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
        inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    inputData.fogCoord = 0.0; // we don't apply fog in the guffer pass

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        inputData.vertexLighting = input.vertexLighting.xyz;
    #else
        inputData.vertexLighting = half3(0, 0, 0);
    #endif

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
#endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitGBufferPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;

    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        real sign = input.tangentOS.w * GetOddNegativeScale();
        half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
    #endif

    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
        output.tangentWS = tangentWS;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
        half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
        output.viewDirTS = viewDirTS;
    #endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
        output.vertexLighting = vertexLight;
    #endif

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        output.positionWS = vertexInput.positionWS;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

    output.positionCS = vertexInput.positionCS;

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
#define _FINAL_GI_CHANNEL 30
#define _LIGHTING 31
#define _MAIN_LIGHTING 32
#define _ADDITIONAL_LIGHTING 33

// private:
half4 _DEBUG_OUTPUT(half4 lit,
                    Varyings input, SurfaceData surfaceData, InputData inputData,
                    half3 sss = half3(0.0f, 0.0f, 0.0f))
{
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
    if (_debugChannel == _FINAL_GI_CHANNEL)
    {
        BRDFData brdfData;
        InitializeBRDFData(surfaceData, brdfData);

        Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
        MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);

        const half3 gi = GlobalIllumination(
            brdfData, inputData.bakedGI, surfaceData.occlusion,
            inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS
        );

        return half4(gi, surfaceData.alpha);
    }
    if (_debugChannel == _LIGHTING)
    {
        surfaceData.albedo = half3(0.5h, 0.5h, 0.5h);
        surfaceData.smoothness = 0.0h;
        
        return UniversalFragmentPBR(inputData, surfaceData);
    }
    if (_debugChannel == _MAIN_LIGHTING)
    {
        surfaceData.albedo = half3(0.5h, 0.5h, 0.5h);
        surfaceData.smoothness = 0.0h;
        
        BRDFData brdfData = (BRDFData)0;
        InitializeBRDFData(surfaceData, brdfData);

        half4 shadowMask = CalculateShadowMask(inputData);
        AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);

        #if UNITY_VERSION >= 202200
        uint meshRenderingLayers = GetMeshRenderingLayer();
        #elif UNITY_VERSION >= 202100
        uint meshRenderingLayers = GetMeshRenderingLightLayer();
        #else
        uint meshRenderingLayers = GetMeshRenderingLightLayer();
        #endif
        
        Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
        LightingData lightingData = CreateLightingData(inputData, surfaceData);

        if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
        {
            const half attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
            lightingData.mainLightColor = LightingPhysicallyBased(
                brdfData, mainLight.color, mainLight.direction,
                attenuation, inputData.normalWS, inputData.viewDirectionWS);
        }

        return CalculateFinalColor(lightingData, surfaceData.alpha);
    }
    if (_debugChannel == _ADDITIONAL_LIGHTING)
    {
        surfaceData.albedo = half3(0.5h, 0.5h, 0.5h);
        surfaceData.smoothness = 0.0h;
        
        BRDFData brdfData = (BRDFData)0;
        InitializeBRDFData(surfaceData, brdfData);

        #if UNITY_VERSION >= 202200
        uint meshRenderingLayers = GetMeshRenderingLayer();
        #elif UNITY_VERSION >= 202100
        uint meshRenderingLayers = GetMeshRenderingLightLayer();
        #else
        uint meshRenderingLayers = GetMeshRenderingLightLayer();
        #endif
        
        LightingData lightingData = CreateLightingData(inputData, surfaceData);

        #if defined(_ADDITIONAL_LIGHTS)
        half4 shadowMask = CalculateShadowMask(inputData);
        AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
        uint pixelLightCount = GetAdditionalLightsCount();

        LIGHT_LOOP_BEGIN(pixelLightCount)
            Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            {
                lightingData.additionalLightsColor += LightingPhysicallyBased(
                    brdfData,
                    light.color, light.direction,
                    light.distanceAttenuation * light.shadowAttenuation,
                    inputData.normalWS, inputData.viewDirectionWS);
            }
        LIGHT_LOOP_END
        #endif

        #if defined(_ADDITIONAL_LIGHTS_VERTEX)
        lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
        #endif

        return CalculateFinalColor(lightingData, surfaceData.alpha);
    }
    clip(-1);
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

void _DebugEmission(Varyings input, inout SurfaceData surfaceData, InputData inputData)
{
    #if _DEBUG_DISPLAY
    surfaceData.emission = DEBUG_RETURN(half4(0.0f, 0.0f, 0.0f, 1.0f), input, surfaceData, inputData).xyz;
    surfaceData.albedo = half3(0.0h, 0.0h, 0.0h);
    surfaceData.normalTS = half3(0.0h, 0.0h, 1.0h);
    surfaceData.smoothness = 0.0h;
    surfaceData.specular = half3(0.0f, 0.0f, 0.0f);
    surfaceData.occlusion = 1.0h;
    surfaceData.metallic = 0.0h;
    inputData.bakedGI = half3(0.0f, 0.0f, 0.0f);
    inputData.vertexLighting = half3(0.0f, 0.0f, 0.0f);
    #endif
}

// Used in Standard (Physically Based) shader
FragmentOutput LitGBufferPassFragment(Varyings input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if defined(_PARALLAXMAP)
#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = input.viewDirTS;
#else
    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, input.viewDirWS);
#endif
    ApplyPerPixelDisplacement(viewDirTS, input.uv);
#endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

    _DebugEmission(input, surfaceData, inputData);

    // Stripped down version of UniversalFragmentPBR().

    // in LitForwardPass GlobalIllumination (and temporarily LightingPhysicallyBased) are called inside UniversalFragmentPBR
    // in Deferred rendering we store the sum of these values (and of emission as well) in the GBuffer
    BRDFData brdfData;
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
    half3 color = GlobalIllumination(brdfData, inputData.bakedGI, surfaceData.occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);

    return BRDFDataToGbuffer(brdfData, inputData, surfaceData.smoothness, surfaceData.emission + color, surfaceData.occlusion);
}

#endif
