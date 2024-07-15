// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

#include "Shim.hlsl"

#include "../Library/Macros.hlsl"
#include "../Library/Globals.hlsl"
#include "../Library/InputsDriven.hlsl"

#include "../Library/Cascade.hlsl"
#include "Geometry.hlsl"

#include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Utility/Depth.hlsl"

#include "../Library/Texture.hlsl"
#include "../Library/Flow.hlsl"

#include "../Library/Utility/Lighting.hlsl"
#include "Normal.hlsl"
#include "Reflection.hlsl"
#include "Refraction.hlsl"
#include "Caustics.hlsl"
#include "VolumeLighting.hlsl"
#include "Fresnel.hlsl"
#include "Foam.hlsl"
#include "Alpha.hlsl"

#if (CREST_PORTALS != 0)
#include "Packages/com.waveharmonic.crest.portals/Runtime/Shaders/Library/Portals.hlsl"
#endif

#if (CREST_SHADOWS_BUILT_IN_RENDER_PIPELINE != 0)
#if CREST_BIRP
#define SHADOWS_SPLIT_SPHERES 1
#include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Utility/Legacy/Core.hlsl"
#include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Utility/Legacy/Shadows.hlsl"
#endif
#endif

#define m_Properties \
    const float2 i_UndisplacedXZ, \
    const float i_LodAlpha, \
    const half i_WaterLevelOffset, \
    const float2 i_WaterLevelDerivatives, \
    const half2 i_Shadow, \
    const half2 i_Flow, \
    const half3 i_ViewDirectionWS, \
    const bool i_Facing, \
    const half3 i_SceneColor, \
    const float i_SceneDepthRaw, \
    const float4 i_ScreenPosition, \
    const float4 i_ScreenPositionRaw, \
    const float3 i_PositionWS, \
    const float3 i_PositionVS, \
    out half3 o_Albedo, \
    out half3 o_NormalWS, \
    out half3 o_Specular, \
    out half3 o_Emission, \
    out half o_Smoothness, \
    out half o_Occlusion, \
    out half o_Alpha

m_CrestNameSpace

static const TiledTexture _Crest_NormalMapTiledTexture =
    TiledTexture::Make(_Crest_NormalMapTexture, sampler_Crest_NormalMapTexture, _Crest_NormalMapTexture_TexelSize, _Crest_NormalMapScale);

static const TiledTexture _Crest_FoamTiledTexture =
    TiledTexture::Make(_Crest_FoamTexture, sampler_Crest_FoamTexture, _Crest_FoamTexture_TexelSize, _Crest_FoamScale);

static const TiledTexture _Crest_CausticsTiledTexture =
    TiledTexture::Make(_Crest_CausticsTexture, sampler_Crest_CausticsTexture, _Crest_CausticsTexture_TexelSize, _Crest_CausticsTextureScale);
static const TiledTexture _Crest_CausticsDistortionTiledTexture =
    TiledTexture::Make(_Crest_CausticsDistortionTexture, sampler_Crest_CausticsDistortionTexture, _Crest_CausticsDistortionTexture_TexelSize, _Crest_CausticsDistortionScale);

void Fragment(m_Properties)
{
    o_Albedo = 0.0;
    o_NormalWS = half3(0.0, 1.0, 0.0);
    o_Specular = 0.0;
    o_Emission = 0.0;
    o_Smoothness = 0.7;
    o_Occlusion = 1.0;
    o_Alpha = 1.0;

    const bool underwater = IsUnderwater(i_Facing, g_Crest_ForceUnderwater);

    // TODO: Should we use PosToSIs or check for overflow?
    float slice0 = _Crest_LodIndex;
    float slice1 = _Crest_LodIndex + 1;

#ifdef CREST_FLOW_ON
    const Flow flow = Flow::Make(i_Flow, g_Crest_Time);
#endif

    const Cascade cascade0 = Cascade::Make(slice0);
    const Cascade cascade1 = Cascade::Make(slice1);

    float sceneRawZ = i_SceneDepthRaw;

#if (CREST_PORTALS != 0)
#ifndef CREST_SHADOWPASS
#if _ALPHATEST_ON
    if (m_CrestPortal)
    {
        const float pixelRawZ = i_ScreenPositionRaw.z / i_ScreenPositionRaw.w;
        if (OutsideOfPortal(i_ScreenPosition.xy, pixelRawZ, sceneRawZ))
        {
            o_Alpha = 0.0;
            return;
        }
    }
#endif
#endif
#endif

    float sceneZ = Utility::CrestLinearEyeDepth(sceneRawZ);
    float pixelZ = -i_PositionVS.z;

    const float weight0 = (1.0 - i_LodAlpha) * cascade0._Weight;
    const float weight1 = (1.0 - weight0) * cascade1._Weight;

    half foam = 0.0;
    half _determinant = 0.0;
    half4 albedo = 0.0;
    if (weight0 > 0.001)
    {
        Cascade::MakeAnimatedWaves(slice0).SampleNormals(i_UndisplacedXZ, weight0, o_NormalWS.xz, _determinant);

        if (_Crest_FoamEnabled)
        {
            Cascade::MakeFoam(slice0).SampleFoam(i_UndisplacedXZ, weight0, foam);
        }

        if (_Crest_AlbedoEnabled)
        {
            Cascade::MakeAlbedo(slice0).SampleAlbedo(i_UndisplacedXZ, weight0, albedo);
        }
    }

    if (weight1 > 0.001)
    {
        Cascade::MakeAnimatedWaves(slice1).SampleNormals(i_UndisplacedXZ, weight1, o_NormalWS.xz, _determinant);

        if (_Crest_FoamEnabled)
        {
            Cascade::MakeFoam(slice1).SampleFoam(i_UndisplacedXZ, weight1, foam);
        }

        if (_Crest_AlbedoEnabled)
        {
            Cascade::MakeAlbedo(slice1).SampleAlbedo(i_UndisplacedXZ, weight1, albedo);
        }
    }

    // Determinant needs to be one when no waves.
    if (_Crest_LodIndex == (uint)g_Crest_LodCount - 1)
    {
        _determinant += 1.0 - weight0;
    }

    // Normal.
    {
        WaterNormal
        (
            i_WaterLevelDerivatives,
            i_ViewDirectionWS,
            _Crest_MinimumReflectionDirectionY,
            underwater,
            o_NormalWS
        );

        if (_Crest_NormalMapEnabled)
        {
            o_NormalWS.xz += SampleNormalMaps
            (
#ifdef CREST_FLOW_ON
                flow,
#endif
                _Crest_NormalMapTiledTexture,
                _Crest_NormalMapStrength,
                i_UndisplacedXZ,
                i_LodAlpha,
                cascade0
            );
        }

        o_NormalWS = normalize(o_NormalWS);

        o_NormalWS.xz *= _Crest_NormalsStrengthOverall;
        o_NormalWS.y = lerp(1.0, o_NormalWS.y, _Crest_NormalsStrengthOverall);

        if (underwater)
        {
            // Flip when underwater.
            o_NormalWS.xyz *= -1.0;
        }
    }

    // Default for opaque render type.
    float sceneDistance = 1000.0;
    float3 scenePositionWS = 0.0;

    half3 ambientLight = 0.0;
    AmbientLight
    (
        ambientLight
    );

    float3 lightIntensity = 0.0;
    half3 lightDirection = 0.0;

    PrimaryLight
    (
        i_PositionWS,
        lightIntensity,
        lightDirection
    );

    half3 additionalLight = AdditionalLighting(i_PositionWS);

#if (CREST_SHADOWS_BUILT_IN_RENDER_PIPELINE != 0)
#if CREST_BIRP
    // Sample shadow maps.
    float4 shadowCoord = GET_SHADOW_COORDINATES(float4(i_PositionWS, 1.0));
    half shadows = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, shadowCoord);
    shadows = lerp(_LightShadowData.r, 1.0, shadows);
    shadows = min(i_Shadow.y, shadows);
#endif
#endif

#if d_Transparent
    bool caustics;
    RefractedScene
    (
        _Crest_RefractionStrength,
        o_NormalWS,
        i_ScreenPosition.xy,
        pixelZ,
        i_SceneColor,
        sceneZ,
        sceneRawZ,
        underwater,
        o_Emission,
        sceneDistance,
        scenePositionWS,
        caustics
    );
#endif

    float refractedSeaLevel = 0;
    float3 refractedSurfacePosition = 0;
    if (!underwater)
    {
        // Sample larger slice to avoid the first slice.
        float4 displacement = Cascade::MakeAnimatedWaves(slice1).Sample(scenePositionWS.xz);
        refractedSeaLevel = g_Crest_WaterCenter.y + displacement.w;
        refractedSurfacePosition = displacement.xyz;
        refractedSurfacePosition.y += refractedSeaLevel;
    }

    // Out-scattering.
    if (!underwater)
    {
        // Account for average extinction of light as it travels down through volume. Assume flat water as anything else would be expensive.
        half3 extinction = _Crest_Absorption + _Crest_Scattering.xyz;
        o_Emission *= exp(-extinction * max(0.0, refractedSeaLevel - scenePositionWS.y));
    }

#if d_Transparent
    // Caustics
    if (_Crest_CausticsEnabled && !underwater && caustics)
    {
        float3 position = scenePositionWS;
#if CREST_BIRP
        position = float3(i_ScreenPosition.xy * _ScreenSize.xy, 0);
#endif

        half lightOcclusion = PrimaryLightShadows(position);

        half blur = 0.0;
#ifdef CREST_FLOW_ON
        blur = _Crest_CausticsMotionBlur;
#endif

        o_Emission *= Caustics
        (
#ifdef CREST_FLOW_ON
            flow,
#endif
            scenePositionWS,
            refractedSurfacePosition.y,
            lightIntensity,
            lightDirection,
            lightOcclusion,
            sceneDistance,
            _Crest_CausticsTiledTexture,
            _Crest_CausticsTextureAverage,
            _Crest_CausticsStrength,
            _Crest_CausticsFocalDepth,
            _Crest_CausticsDepthOfField,
            _Crest_CausticsDistortionTiledTexture,
            _Crest_CausticsDistortionStrength,
            blur,
            underwater
        );
    }
#endif

    half3 sss = 0.0;

    if (_Crest_SSSEnabled)
    {
        sss = PinchSSS
        (
            _determinant,
            _Crest_SSSPinchMinimum,
            _Crest_SSSPinchMaximum,
            _Crest_SSSPinchFalloff,
            _Crest_SSSIntensity,
            lightDirection,
            _Crest_SSSDirectionalFalloff,
            i_ViewDirectionWS
        );
    }

    // Volume Lighting
    half3 volumeLight = 0.0;
    half3 volumeOpacity = 0.0;
    VolumeLighting
    (
        _Crest_Absorption,
        _Crest_Scattering.xyz,
        _Crest_Anisotropy,
        i_Shadow.x,
        i_ViewDirectionWS,
        ambientLight,
        lightDirection,
        lightIntensity,
        additionalLight,
        _Crest_AmbientTerm,
        _Crest_DirectTerm,
        sceneDistance,
        sss,
        volumeLight,
        volumeOpacity
    );

    // Fresnel
    float reflected = 0.0;
    float transmitted = 0.0;
    {
        ApplyFresnel
        (
            i_ViewDirectionWS,
            o_NormalWS,
            underwater,
            1.0, // air
            _Crest_RefractiveIndexOfWater,
            _Crest_TotalInternalReflectionIntensity,
            transmitted,
            reflected
        );

        if (underwater)
        {
            o_Emission *= transmitted;
            o_Emission += volumeLight * reflected;
        }
        else
        {
            o_Emission *= 1.0 - volumeOpacity;
            o_Emission += volumeLight * volumeOpacity;
            o_Emission *= transmitted;
        }
    }

    // Specular
    {
        o_Specular = _Crest_Specular * reflected * i_Shadow.y;
    }

    // Smoothness
    {
        // Vary smoothness by distance.
        o_Smoothness = lerp(_Crest_Smoothness, _Crest_SmoothnessFar, pow(saturate(pixelZ / _Crest_SmoothnessFarDistance), _Crest_SmoothnessFalloff));
    }

    // Occlusion
    {
        o_Occlusion = underwater ? _Crest_OcclusionUnderwater : _Crest_Occlusion;
    }

    // Planar Reflections
    if (_Crest_PlanarReflectionsEnabled)
    {
        half4 reflection = PlanarReflection
        (
            _Crest_ReflectionTexture,
            sampler_Crest_ReflectionTexture,
            _Crest_PlanarReflectionsIntensity,
            o_Smoothness,
            o_NormalWS,
            _Crest_PlanarReflectionsDistortion,
            i_ViewDirectionWS,
            i_ScreenPosition.xy,
            underwater
        );

        half alpha = reflection.a;
        o_Emission = lerp(o_Emission, reflection.rgb, alpha * reflected * o_Occlusion);
        // Override reflections with planar reflections.
        // Results are darker than Unity's.
        o_Occlusion *= 1.0 - alpha;
    }

    // Foam
    if (_Crest_FoamEnabled)
    {
        half albedo = MultiScaleFoamAlbedo
        (
#ifdef CREST_FLOW_ON
            flow,
#endif
            _Crest_FoamTiledTexture,
            _Crest_FoamFeather,
            foam,
            cascade0,
            cascade1,
            i_LodAlpha,
            i_UndisplacedXZ
        );

        half2 normal = MultiScaleFoamNormal
        (
#ifdef CREST_FLOW_ON
            flow,
#endif
            _Crest_FoamTiledTexture,
            _Crest_FoamFeather,
            _Crest_FoamNormalStrength,
            foam,
            albedo,
            cascade0,
            cascade1,
            i_LodAlpha,
            i_UndisplacedXZ,
            pixelZ
        );

        half3 intensity = _Crest_FoamIntensityAlbedo;

#if (CREST_SHADOWS_BUILT_IN_RENDER_PIPELINE != 0)
#if CREST_BIRP
        // @HACK: Scale intensity as BIRP does not support shadows for transparent objects.
        intensity = max(_Crest_FoamIntensityAlbedo * saturate(ShadeSH9(float4(o_NormalWS, 1.0))), _Crest_FoamIntensityAlbedo * shadows);
#endif
#endif

        ApplyFoamToSurface
        (
            albedo,
            normal,
            intensity,
            _Crest_Occlusion,
            _Crest_FoamSmoothness,
            _Crest_Specular,
            underwater,
            o_Albedo,
            o_NormalWS,
            o_Emission,
            o_Occlusion,
            o_Smoothness,
            o_Specular
        );

        // We will use this for shadow casting.
        foam = albedo;
    }

    // Albedo
    if (_Crest_AlbedoEnabled)
    {
        o_Albedo = lerp(o_Albedo, albedo.rgb, albedo.a);
        o_Emission *= 1.0 - albedo.a;
    }

    // Alpha
    {
#ifndef CREST_SHADOWPASS
#if d_Transparent
        // Feather at intersection. Cannot be used for shadows since depth is not available.
        o_Alpha = saturate((sceneZ - pixelZ) / 0.2);
#endif
#endif

        // This keyword works for all RPs despite BIRP having prefixes in serialised data.
#if _ALPHATEST_ON
#if CREST_SHADOWPASS
        o_Alpha = max(foam, albedo.a) - _Crest_ShadowCasterThreshold;
#endif

        // Add 0.5 bias for LOD blending and texel resolution correction. This will help to
        // tighten and smooth clipped edges.
        o_Alpha -= ClipSurface(i_PositionWS.xz) > 0.5 ? 2.0 : 0.0;
#endif // _ALPHATEST_ON

        // Specular in HDRP is still affected outside the 0-1 alpha range.
        o_Alpha = min(o_Alpha, 1.0);
    }

#if (CREST_SHADOWS_BUILT_IN_RENDER_PIPELINE != 0)
#if CREST_BIRP
    // @HACK: Dull highlights as BIRP does not support shadows for transparent objects.
    o_Smoothness *= lerp(1, shadows, foam * 100);
    // @FIXME: 0.2 to difference when high. Likely incorrect shadow sampling.
    o_Specular = shadows < 0.2 ? 1.0 - max(shadows, 0.3) : o_Specular;
#endif
#endif
}

m_CrestNameSpaceEnd

void Fragment_float(m_Properties)
{
    m_Crest::Fragment
    (
        i_UndisplacedXZ,
        i_LodAlpha,
        i_WaterLevelOffset,
        i_WaterLevelDerivatives,
        i_Shadow,
        i_Flow,
        i_ViewDirectionWS,
        i_Facing,
        i_SceneColor,
        i_SceneDepthRaw,
        i_ScreenPosition,
        i_ScreenPositionRaw,
        i_PositionWS,
        i_PositionVS,
        o_Albedo,
        o_NormalWS,
        o_Specular,
        o_Emission,
        o_Smoothness,
        o_Occlusion,
        o_Alpha
    );
}

#undef m_Properties
