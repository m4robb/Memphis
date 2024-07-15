// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

// Based on tutorial: https://connect.unity.com/p/adding-your-own-hlsl-code-to-shader-graph-the-custom-function-node

#ifndef CREST_LIGHTING_H
#define CREST_LIGHTING_H

#include "../Macros.hlsl"
#include "../Globals.hlsl"

TEXTURE2D_X(_Crest_ScreenSpaceShadowTexture);
float4 _Crest_ScreenSpaceShadowTexture_TexelSize;

#if CREST_URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif

#if CREST_HDRP
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

#ifndef SHADERGRAPH_PREVIEW
#if CREST_HDRP_FORWARD_PASS
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/HDShadow.hlsl"
#endif
#endif

#if UNITY_VERSION < 202310
#define GetMeshRenderingLayerMask GetMeshRenderingLightLayer
#endif

#if UNITY_VERSION < 600000
#if PROBE_VOLUMES_L1
#define AMBIENT_PROBE_BUFFER 1
#endif
#endif

m_CrestNameSpace

// TODO: Move
void ApplyIndirectLightingMultiplier
(
    inout half3 io_AmbientLight
)
{
    // Allows control of baked lighting through volume framework.
#ifndef SHADERGRAPH_PREVIEW
    // We could create a BuiltinData struct which would have rendering layers on it, but it seems more complicated.
    io_AmbientLight *= GetIndirectDiffuseMultiplier(GetMeshRenderingLayerMask());
#endif
}
#else // CREST_HDRP
m_CrestNameSpace
#endif

void PrimaryLight
(
    const float3 i_PositionWS,
    out half3 o_Color,
    out half3 o_Direction
)
{
#if CREST_HDRP
    // We could get the main light the same way we get the main light shadows,
    // but most of the data would be missing (including below horizon
    // attenuation) which would require re-running the light loop which is expensive.
    o_Direction = g_Crest_PrimaryLightDirection;
    o_Color = g_Crest_PrimaryLightIntensity;
#elif CREST_URP
    // Actual light data from the pipeline.
    Light light = GetMainLight();
    o_Direction = light.direction;
    o_Color = light.color;
#elif CREST_BIRP
#ifndef USING_DIRECTIONAL_LIGHT
    // Yes. This function wants the world position of the surface.
    o_Direction = normalize(UnityWorldSpaceLightDir(i_PositionWS));
#else
    o_Direction = _WorldSpaceLightPos0.xyz;
#endif
    o_Color = _LightColor0.rgb;
#endif
}

void AmbientLight(out half3 o_AmbientLight)
{
    // Use the constant term (0th order) of SH stuff - this is the average.
    o_AmbientLight =
#if AMBIENT_PROBE_BUFFER
        half3(_AmbientProbeData[0].w, _AmbientProbeData[1].w, _AmbientProbeData[2].w);
#else
        half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
#endif

#if CREST_HDRP
    ApplyIndirectLightingMultiplier(o_AmbientLight);
#endif
}

// Position: SRP = WS / BIRP = SS (z ignored)
half PrimaryLightShadows(const float3 i_Position)
{
    half shadow = 0;

#if CREST_URP
    // We could skip GetMainLight but this is recommended approach which is likely more robust to API changes.
    float4 shadowCoord = TransformWorldToShadowCoord(i_Position);
    Light light = GetMainLight(TransformWorldToShadowCoord(i_Position));
    shadow = light.shadowAttenuation;
#endif

#ifndef SHADERGRAPH_PREVIEW
#if CREST_HDRP_FORWARD_PASS
    DirectionalLightData light = _DirectionalLightDatas[_DirectionalShadowIndex];
    HDShadowContext context = InitShadowContext();
    context.directionalShadowData = _HDDirectionalShadowData[_DirectionalShadowIndex];

    float3 positionWS = GetCameraRelativePositionWS(i_Position);
    // From Unity:
    // > With XR single-pass and camera-relative: offset position to do lighting computations from the combined center view (original camera matrix).
    // > This is required because there is only one list of lights generated on the CPU. Shadows are also generated once and shared between the instanced views.
    ApplyCameraRelativeXR(positionWS);

    // TODO: Pass in screen space position and scene normal.
    shadow = GetDirectionalShadowAttenuation
    (
        context,
        0, // positionSS
        positionWS,
        0, // normalWS
        light.shadowIndex,
        -light.forward
    );

    // Apply shadow strength from main light.
    shadow = LerpWhiteTo(shadow, light.shadowDimmer);
#endif // CREST_HDRP_FORWARD_PASS
#endif // SHADERGRAPH_PREVIEW

#if CREST_BIRP
    shadow = LOAD_TEXTURE2D_X(_Crest_ScreenSpaceShadowTexture,  min(i_Position.xy, _Crest_ScreenSpaceShadowTexture_TexelSize.zw - 1.0)).r;
#endif

    return shadow;
}

half3 AdditionalLighting(float3 i_Position)
{
    half3 color = 0.0;

#if CREST_URP
#if !USE_FORWARD_PLUS
#ifdef _ADDITIONAL_LIGHTS
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        // Last parameter is shadow mask. Since we do not support lightmaps just pass probes. See:
        // Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl
        Light light = GetAdditionalLight(i, i_Position, unity_ProbesOcclusion);
        color += light.color * (light.distanceAttenuation * light.shadowAttenuation);
    }
#endif
#endif
#endif

    return color;
}

m_CrestNameSpaceEnd

#endif
