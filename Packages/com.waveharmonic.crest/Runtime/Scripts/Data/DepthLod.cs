﻿// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using WaveHarmonic.Crest.Editor;
using WaveHarmonic.Crest.Utility;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Data that gives depth of the water (height of sea level above water floor).
    /// </summary>
    [FilterEnum(nameof(_TextureFormatMode), Filtered.Mode.Exclude, (int)LodTextureFormatMode.Automatic)]
    sealed partial class DepthLod : Lod
#if UNITY_EDITOR
        , IOptionalDepthLod
#endif
    {
        [Tooltip("Support signed distance field data generated from the depth probes. Requires a two component texture format.")]
        [@DecoratedField, SerializeField]
        internal bool _EnableSignedDistanceFields = true;

        // NOTE: Must match CREST_WATER_DEPTH_BASELINE in Constants.hlsl.
        internal const float k_DepthBaseline = float.MaxValue;
        internal static readonly Color s_GizmoColor = new(1f, 0f, 0f, 0.5f);
        // We want the clear color to be the mininimum terrain height (-1000m).
        // Mathf.Infinity can cause problems for distance.
        static readonly Color s_NullColor = new(-k_DepthBaseline, k_DepthBaseline, 0, 0);

        internal override string ID => "Depth";
        internal override string Name => "Water Depth";
        internal override Color GizmoColor => s_GizmoColor;
        protected override Color ClearColor => s_NullColor;
        protected override bool NeedToReadWriteTextureData => true;
        internal override bool RunsInHeadless => true;

        protected override GraphicsFormat RequestedTextureFormat => _TextureFormatMode switch
        {
            LodTextureFormatMode.Automatic or
            LodTextureFormatMode.Performance => _EnableSignedDistanceFields ? GraphicsFormat.R16G16_SFloat : GraphicsFormat.R16_SFloat,
            LodTextureFormatMode.Precision => _EnableSignedDistanceFields ? GraphicsFormat.R32G32_SFloat : GraphicsFormat.R32_SFloat,
            LodTextureFormatMode.Manual => _TextureFormat,
            _ => throw new System.NotImplementedException(),
        };

        Texture2DArray _NullTexture;
        protected override Texture2DArray NullTexture
        {
            get
            {
                if (_NullTexture == null)
                {
                    var texture = TextureArrayHelpers.CreateTexture2D(s_NullColor, TextureFormat.RFloat);
                    texture.name = $"_Crest_{ID}LodTemporaryDefaultTexture";
                    _NullTexture = TextureArrayHelpers.CreateTexture2DArray(texture, k_MaximumSlices);
                    _NullTexture.name = $"_Crest_{ID}LodDefaultTexture";
                    Helpers.Destroy(texture);
                }

                return _NullTexture;
            }
        }

        internal DepthLod()
        {
            _Enabled = true;
            _TextureFormat = GraphicsFormat.R16G16_SFloat;
        }

        public override void AddToHash(ref int hash)
        {
            base.AddToHash(ref hash);
            Hash.AddBool(_EnableSignedDistanceFields, ref hash);
        }

        internal static readonly SortedList<int, ILodInput> s_Inputs = new(Helpers.DuplicateComparison);
        private protected override SortedList<int, ILodInput> Inputs => s_Inputs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnLoad()
        {
            s_Inputs.Clear();
        }
    }
}
