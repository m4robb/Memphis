// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

#include "../Library/Macros.hlsl"
#include "../Library/Globals.hlsl"
#include "../Library/InputsDriven.hlsl"

#include "../Library/Cascade.hlsl"
#include "../Surface/Geometry.hlsl"

#include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Utility/Depth.hlsl"

#define m_Properties \
    const float3 i_PositionWS, \
    const float3 i_ObjectPosition, \
    const float3 i_CameraPosition, \
    const float i_Time, \
    out float3 o_PositionWS, \
    out float2 o_UndisplacedXZ, \
    out float o_LodAlpha, \
    out half o_WaterLevelOffset, \
    out float2 o_WaterLevelDerivatives, \
    out half2 o_Shadow, \
    out half2 o_Flow

#if _TRANSPARENT_WRITES_MOTION_VEC
#define m_Slice clamp(_Crest_LodIndex + (isMotionVectors ? g_Crest_LodChange : 0), 0, g_Crest_LodCount)
#define m_Make(slice) Make(slice, isMotionVectors)
#define m_ChunkMeshScaleAlpha isMotionVectors ? _Crest_ChunkMeshScaleAlphaSource : _Crest_ChunkMeshScaleAlpha
#define m_ChunkGeometryGridWidth isMotionVectors ? _Crest_ChunkGeometryGridWidthSource : _Crest_ChunkGeometryGridWidth
#else
#define m_Slice _Crest_LodIndex
#define m_Make(slice) Make(slice)
#define m_ChunkMeshScaleAlpha _Crest_ChunkMeshScaleAlpha
#define m_ChunkGeometryGridWidth _Crest_ChunkGeometryGridWidth
#endif

m_CrestNameSpace

void Vertex(m_Properties)
{
    // This will get called twice.
    // With current and previous time respectively.

    o_UndisplacedXZ = 0.0;
    o_LodAlpha = 0.0;
    o_WaterLevelOffset = 0.0;
    o_WaterLevelDerivatives = 0.0;
    o_Shadow = 0.0;
    o_Flow = 0.0;

    const bool isMotionVectors = i_Time < g_Crest_UnityTime;

    const float slice0 = m_Slice;
    const float slice1 = slice0 + 1;
    const Cascade cascade0 = Cascade::m_Make(slice0);
    const Cascade cascade1 = Cascade::m_Make(slice1);

    o_PositionWS = i_PositionWS;

    // Vertex snapping and LOD transition.
    SnapAndTransitionVertLayout
    (
        m_ChunkMeshScaleAlpha,
        cascade0,
        m_ChunkGeometryGridWidth,
        o_PositionWS,
        o_LodAlpha
    );

    // Fix precision errors at edges.
    {
        // Scale up by small "epsilon" to solve numerical issues. Expand slightly about tile center.
        // :WaterGridPrecisionErrors
        const float2 tileCenterXZ = i_ObjectPosition.xz;
        const float2 cameraPositionXZ = abs(i_CameraPosition.xz);
        // Scale "epsilon" by distance from zero. There is an issue where overlaps can cause SV_IsFrontFace
        // to be flipped (needs to be investigated). Gaps look bad from above surface, and overlaps look bad
        // from below surface. We want to close gaps without introducing overlaps. A fixed "epsilon" will
        // either not solve gaps at large distances or introduce too many overlaps at small distances. Even
        // with scaling, there are still unsolvable overlaps underwater (especially at large distances).
        // 100,000 (0.00001) is the maximum position before Unity warns the user of precision issues.
        o_PositionWS.xz = lerp(tileCenterXZ, o_PositionWS.xz, lerp(1.0, 1.01, max(cameraPositionXZ.x, cameraPositionXZ.y) * 0.00001));
    }

    o_UndisplacedXZ = o_PositionWS.xz;

    // Calculate sample weights. params.z allows shape to be faded out (used on last lod to support pop-less scale transitions).
    const float weight0 = (1.0 - o_LodAlpha) * cascade0._Weight;
    const float weight1 = (1.0 - weight0) * cascade1._Weight;

    // Data that needs to be sampled at the undisplaced position.
    if (weight0 > 0.001)
    {
#if _TRANSPARENT_WRITES_MOTION_VEC
        if (isMotionVectors)
        {
            Cascade::MakeAnimatedWavesSource(slice0).SampleDisplacement(o_UndisplacedXZ, weight0, o_PositionWS, o_WaterLevelDerivatives);
        }
        else
#endif
        {
            Cascade::MakeAnimatedWaves(slice0).SampleDisplacement(o_UndisplacedXZ, weight0, o_PositionWS, o_WaterLevelDerivatives);
        }
    }

    if (weight1 > 0.001)
    {
#if _TRANSPARENT_WRITES_MOTION_VEC
        if (isMotionVectors)
        {
            Cascade::MakeAnimatedWavesSource(slice1).SampleDisplacement(o_UndisplacedXZ, weight1, o_PositionWS, o_WaterLevelDerivatives);
        }
        else
#endif
        {
            Cascade::MakeAnimatedWaves(slice1).SampleDisplacement(o_UndisplacedXZ, weight1, o_PositionWS, o_WaterLevelDerivatives);
        }
    }

    // Data that needs to be sampled at the displaced position.
    if (weight0 > 0.001)
    {
#if CREST_FLOW_ON
        Cascade::MakeFlow(slice0).SampleFlow(o_UndisplacedXZ, weight0, o_Flow);
#endif

        if (_Crest_ShadowsEnabled)
        {
            Cascade::MakeShadow(slice0).SampleShadow(o_PositionWS.xz, weight0, o_Shadow);
        }
    }

    if (weight1 > 0.001)
    {
#if CREST_FLOW_ON
        Cascade::MakeFlow(slice1).SampleFlow(o_UndisplacedXZ, weight1, o_Flow);
#endif

        if (_Crest_ShadowsEnabled)
        {
            Cascade::MakeShadow(slice1).SampleShadow(o_PositionWS.xz, weight1, o_Shadow);
        }
    }

    // Invert so shadows are black as we normally multiply this by lighting.
    o_Shadow = 1.0 - o_Shadow;

#if _TRANSPARENT_WRITES_MOTION_VEC
    if (isMotionVectors)
    {
        o_PositionWS.xz -= g_Crest_WaterCenter.xz;
        o_PositionWS.xz *= g_Crest_WaterScaleChange;
        o_PositionWS.xz += g_Crest_WaterCenter.xz;
        o_PositionWS += g_Crest_WaterCenterDelta;
    }
#endif
}

m_CrestNameSpaceEnd

void Vertex_float(m_Properties)
{
    m_Crest::Vertex
    (
        i_PositionWS,
        i_ObjectPosition,
        i_CameraPosition,
        i_Time,
        o_PositionWS,
        o_UndisplacedXZ,
        o_LodAlpha,
        o_WaterLevelOffset,
        o_WaterLevelDerivatives,
        o_Shadow,
        o_Flow
    );
}

#undef m_Properties
