// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

// This sets base water height to Y value of geometry.

Shader "Crest/Inputs/Level/Water Level From Geometry"
{
    Properties
    {
        [HideInInspector]
        _Crest_Version("Version", Integer) = 0
    }

    SubShader
    {
        Pass
        {
            Blend Off

            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"
            #include "../../Library/Globals.hlsl"

            CBUFFER_START(CrestPerWaterInput)
            float3 _Crest_DisplacementAtInputPosition;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;

                o.worldPos = mul(unity_ObjectToWorld, float4(input.positionOS, 1.0)).xyz;
                // Correct for displacement
                o.worldPos.xz -= _Crest_DisplacementAtInputPosition.xz;

                o.positionCS = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.0));

                return o;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Write displacement to get from sea level of water to the y value of this geometry
                const float heightOffset = input.worldPos.y - g_Crest_WaterCenter.y;
                return heightOffset;
            }
            ENDCG
        }
    }
    CustomEditor "WaveHarmonic.Crest.Editor.CustomShaderGUI"
}
