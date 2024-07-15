// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Rendering;
using WaveHarmonic.Crest.Editor;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Tags this object as an water depth provider. Renders depth every frame and should only be used for dynamic objects.
    /// For static objects, use a Depth Probe.
    /// </summary>
    [@HelpURL("Manual/ShallowsAndShorelines.html#sea-floor-depth")]
    sealed partial class DepthLodInput : LodInput
#if UNITY_EDITOR
        , IOptionalDepthLod
#endif
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [@Space(10)]

        [@Label("Relative Height")]
        [Tooltip("Whether the data is relative to the input height. Useful for procedural placement.")]
        [@DecoratedField, SerializeField]
        internal bool _Relative;

        [@Label("Copy Signed Distance Field")]
        [Tooltip("Whether the data is relative to the input height. Useful for procedural placement.")]
        [@DecoratedField, SerializeField]
        internal bool _CopySignedDistanceField;

        public static new class ShaderIDs
        {
            public static readonly int s_HeightOffset = Shader.PropertyToID("_Crest_HeightOffset");
            public static readonly int s_SDF = Shader.PropertyToID("_Crest_SDF");
        }

        internal override LodInputMode DefaultMode => LodInputMode.Renderer;

        public override void Draw(Lod simulation, CommandBuffer buffer, RenderTexture target, int pass = -1, float weight = 1f, int slice = -1)
        {
            var wrapper = Data.GetProperties(buffer);
            wrapper.GetBlock();

            wrapper.SetFloat(ShaderIDs.s_HeightOffset, _Relative ? transform.position.y : 0f);

            if (IsCompute)
            {
                wrapper.SetInteger(ShaderIDs.s_SDF, _CopySignedDistanceField ? 1 : 0);
                buffer.SetKeyword(WaterResources.Instance.Compute._DepthTexture, WaterResources.Instance.Keywords.DepthTextureSDF, simulation._Water._DepthLod._EnableSignedDistanceFields);
            }

            wrapper.SetBlock();

            base.Draw(simulation, buffer, target, pass, weight, slice);
        }
    }
}
