// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

Shader "Hidden/Crest/Underwater/Water Surface Mask"
{
    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.high-definition"
        }

        Tags { "RenderPipeline"="HDRenderPipeline" }

        Pass
        {
            Name "Water Surface Mask"
            // We always disable culling when rendering water mask, as we only
            // use it for underwater rendering features.
            Cull Off

            Stencil
            {
                Ref [_StencilRef]
                Comp Equal
            }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            // for VFACE
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            #include "Mask.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "Water Horizon Mask"
            Cull Off
            ZWrite Off
            // Horizon must be rendered first or it will overwrite the mask with incorrect values. ZTest not needed.
            ZTest Always

            Stencil
            {
                Ref [_StencilRef]
                Comp Equal
            }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            #include "MaskHorizon.hlsl"
            ENDHLSL
        }
    }

    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal"
        }

        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "Water Surface Mask"
            // We always disable culling when rendering water mask, as we only
            // use it for underwater rendering features.
            Cull Off

            Stencil
            {
                Ref [_StencilRef]
                Comp Equal
            }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            // for VFACE
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #include "Mask.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "Water Horizon Mask"
            Cull Off
            ZWrite Off
            // Horizon must be rendered first or it will overwrite the mask with incorrect values. ZTest not needed.
            ZTest Always

            Stencil
            {
                Ref [_StencilRef]
                Comp Equal
            }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #include "MaskHorizon.hlsl"
            ENDHLSL
        }
    }

    SubShader
    {
        Pass
        {
            Name "Water Surface Mask"
            // We always disable culling when rendering water mask, as we only
            // use it for underwater rendering features.
            Cull Off

            Stencil
            {
                Ref [_StencilRef]
                Comp Equal
            }

            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            // for VFACE
            #pragma target 3.0

            #include "UnityCG.cginc"

            #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Utility/Legacy/Core.hlsl"
            #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Utility/Legacy/InputsDriven.hlsl"

            #include "Mask.hlsl"
            ENDCG
        }

        Pass
        {
            Name "Water Horizon Mask"
            Cull Off
            ZWrite Off
            // Horizon must be rendered first or it will overwrite the mask with incorrect values. ZTest not needed.
            ZTest Always

            Stencil
            {
                Ref [_StencilRef]
                Comp Equal
            }

            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "UnityCG.cginc"

            #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Utility/Legacy/Core.hlsl"
            #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Utility/Legacy/InputsDriven.hlsl"

            #include "MaskHorizon.hlsl"
            ENDCG
        }
    }
}
