// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

// The const keyword for PSSL solves the following:
// > Shader error in '<Shader>': Program '<Program>', member function '<FunctionName>' not viable: 'this' argument has
// > type '<Type> const', but function is not marked const
// This appears to be PSSL only feature as the fix throws a compiler error elsewhere (comprehensive test not done). I
// tried putting const at the beginning of the function signature which compiles but did not solve the problem on PSSL
// so must be different.

#ifndef CREST_TEXTURE_INCLUDED
#define CREST_TEXTURE_INCLUDED

#include "Settings.Crest.hlsl"
#include "Macros.hlsl"
#include "Globals.hlsl"
#include "InputsDriven.hlsl"

#ifdef SHADER_API_PSSL
#define m_ConstantReturn const
#else
#define m_ConstantReturn
#endif

m_CrestNameSpace

struct TiledTexture
{
    Texture2D _texture;
    SamplerState _sampler;
    half _size;
    half _scale;
    float _texel;

    static TiledTexture Make
    (
        in const Texture2D i_texture,
        in const SamplerState i_sampler,
        in const float4 i_size,
        in const half i_scale
    )
    {
        TiledTexture tiledTexture;
        tiledTexture._texture = i_texture;
        tiledTexture._sampler = i_sampler;
        tiledTexture._scale = i_scale;
        // Safely assume a square texture.
        tiledTexture._size = i_size.z;
        tiledTexture._texel = i_size.x;
        return tiledTexture;
    }

    half4 Sample(float2 uv) m_ConstantReturn
    {
        return SAMPLE_TEXTURE2D(_texture, _sampler, uv);
    }

    half4 SampleLevel(float2 uv, float lod) m_ConstantReturn
    {
        return SAMPLE_TEXTURE2D_LOD(_texture, _sampler, uv, lod);
    }
};

m_CrestNameSpaceEnd

#endif // CREST_TEXTURE_INCLUDED
