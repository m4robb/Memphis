// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

#if UNITY_EDITOR

namespace WaveHarmonic.Crest.Editor
{
    interface IOptionalLod
    {
        Lod GetLod(WaterRenderer water);

        // FeatureProperty
        string PropertyName { get; }
        // FeatureLabel
        string PropertyLabel { get; }

        // Optional. Not all simulations will have a corresponding keyword.
        bool HasMaterialToggle => !string.IsNullOrEmpty(MaterialProperty);

        // Needed as clip surface material toggle is Alpha Clipping.
        string MaterialPropertyLabel => $"{PropertyLabel} > Enabled";

        string MaterialProperty => null;
        string MaterialKeyword => $"{MaterialProperty}_ON";
    }

    interface IOptionalAlbedoLod : IOptionalLod
    {
        Lod IOptionalLod.GetLod(WaterRenderer water) => water.AlbedoLod;
        string IOptionalLod.PropertyLabel => "Albedo";
        string IOptionalLod.PropertyName => nameof(WaterRenderer._AlbedoLod);
        string IOptionalLod.MaterialProperty => "_Crest_AlbedoEnabled";
    }

    interface IOptionalClipLod : IOptionalLod
    {
        Lod IOptionalLod.GetLod(WaterRenderer water) => water.ClipLod;
        string IOptionalLod.PropertyLabel => "Clip Surface";
        string IOptionalLod.PropertyName => nameof(WaterRenderer._ClipLod);

        string IOptionalLod.MaterialPropertyLabel => "Alpha Clipping";

        // BIRP SG has prefixes for Unity properties but other RPs do not. These prefixes are for serialisation only and
        // are not used in the shader.
        string IOptionalLod.MaterialProperty => (RenderPipelineHelper.IsLegacy ? "_BUILTIN" : "") + "_AlphaClip";
        string IOptionalLod.MaterialKeyword => (RenderPipelineHelper.IsLegacy ? "_BUILTIN" : "") + "_ALPHATEST_ON";
    }

    interface IOptionalDynamicWavesLod : IOptionalLod
    {
        Lod IOptionalLod.GetLod(WaterRenderer water) => water.DynamicWavesLod;
        string IOptionalLod.PropertyLabel => "Dynamic Waves";
        string IOptionalLod.PropertyName => nameof(WaterRenderer._DynamicWavesLod);
    }

    interface IOptionalFlowLod : IOptionalLod
    {
        Lod IOptionalLod.GetLod(WaterRenderer water) => water.FlowLod;
        string IOptionalLod.PropertyLabel => "Flow";
        string IOptionalLod.PropertyName => nameof(WaterRenderer._FlowLod);
        string IOptionalLod.MaterialProperty => "CREST_FLOW";
    }

    interface IOptionalFoamLod : IOptionalLod
    {
        Lod IOptionalLod.GetLod(WaterRenderer water) => water.FoamLod;
        string IOptionalLod.PropertyLabel => "Foam";
        string IOptionalLod.PropertyName => nameof(WaterRenderer._FoamLod);
        string IOptionalLod.MaterialProperty => "_Crest_FoamEnabled";
    }

    interface IOptionalDepthLod : IOptionalLod
    {
        Lod IOptionalLod.GetLod(WaterRenderer water) => water.DepthLod;
        string IOptionalLod.PropertyLabel => "Water Depth";
        string IOptionalLod.PropertyName => nameof(WaterRenderer._DepthLod);
    }

    interface IOptionalShadowLod : IOptionalLod
    {
        Lod IOptionalLod.GetLod(WaterRenderer water) => water.ShadowLod;
        string IOptionalLod.PropertyLabel => "Shadow";
        string IOptionalLod.PropertyName => nameof(WaterRenderer._ShadowLod);
        string IOptionalLod.MaterialProperty => "_Crest_ShadowsEnabled";
    }

    interface IOptionalLevelLod : IOptionalLod
    {
        Lod IOptionalLod.GetLod(WaterRenderer water) => water.LevelLod;
        string IOptionalLod.PropertyLabel => "Level";
        string IOptionalLod.PropertyName => nameof(WaterRenderer._LevelLod);
    }
}

#endif
