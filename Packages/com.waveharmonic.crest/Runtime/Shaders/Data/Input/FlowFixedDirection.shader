// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

Shader "Crest/Inputs/Flow/Fixed Direction"
{
    Properties
    {
        _Crest_Speed("Speed", Range(0.0, 30.0)) = 1.0
        _Crest_Direction("Direction", Range(0.0, 1.0)) = 0.0

        [Toggle(d_Feather)]
        _Crest_Feather("Feather At UV Extents", Float) = 0
        _Crest_FeatherWidth("Feather Width", Range(0.001, 0.5)) = 0.1

        [HideInInspector]
        _Crest_Version("Version", Integer) = 0
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma shader_feature_local d_Feather

            #include "UnityCG.cginc"

            #include "../../Library/Globals.hlsl"
            #include "../../Library/InputsDriven.hlsl"
            #include "../../Library/Helpers.hlsl"

            CBUFFER_START(CrestPerWaterInput)
            float _Crest_Speed;
            float _Crest_Direction;
            float3 _Crest_DisplacementAtInputPosition;
            half _Crest_FeatherWidth;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
#if d_Feather
                float2 uv : TEXCOORD0;
#endif
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 vel : TEXCOORD0;
#if d_Feather
                float2 uv : TEXCOORD1;
#endif
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;

                float3 worldPos = mul(unity_ObjectToWorld, float4(input.positionOS, 1.0)).xyz;
                // Correct for displacement
                worldPos.xz -= _Crest_DisplacementAtInputPosition.xz;
                o.positionCS = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));

                o.vel = _Crest_Speed * float2(cos(_Crest_Direction * 6.283185), sin(_Crest_Direction * 6.283185));

#if d_Feather
                o.uv = input.uv;
#endif

                return o;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 flow = input.vel;

#if d_Feather
                flow *= FeatherWeightFromUV(input.uv, _Crest_FeatherWidth);
#endif
                return float4(flow, 0.0, 0.0);
            }
            ENDCG
        }
    }
    CustomEditor "WaveHarmonic.Crest.Editor.CustomShaderGUI"
}
