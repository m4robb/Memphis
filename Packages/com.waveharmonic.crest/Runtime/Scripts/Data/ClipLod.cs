// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using WaveHarmonic.Crest.Editor;
using WaveHarmonic.Crest.Utility;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Drives water surface clipping (carving holes). 0-1 values, surface clipped when > 0.5.
    /// </summary>
    [FilterEnum(nameof(_TextureFormatMode), Filtered.Mode.Exclude, (int)LodTextureFormatMode.Automatic)]
    sealed partial class ClipLod : Lod
#if UNITY_EDITOR
        , IOptionalClipLod
#endif
    {
        [Tooltip("Whether to clip nothing by default (and clip inputs remove patches of surface), or to clip everything by default (and clip inputs add patches of surface).")]
        [@DecoratedField, SerializeField]
        internal DefaultClippingState _DefaultClippingState = DefaultClippingState.NothingClipped;

        static new class ShaderIDs
        {
            public static readonly int s_ClipByDefault = Shader.PropertyToID("g_Crest_ClipByDefault");
        }

        internal enum DefaultClippingState
        {
            NothingClipped,
            EverythingClipped,
        }

        internal static readonly Color s_GizmoColor = new(0f, 1f, 1f, 0.5f);

        internal override string ID => "Clip";
        internal override string Name => "Clip Surface";
        internal override Color GizmoColor => s_GizmoColor;
        protected override Color ClearColor => _DefaultClippingState == DefaultClippingState.EverythingClipped ? Color.white : Color.black;
        protected override bool NeedToReadWriteTextureData => true;

        protected override GraphicsFormat RequestedTextureFormat => _TextureFormatMode switch
        {
            // The clip values only really need 8bits (unless using signed distance).
            LodTextureFormatMode.Performance => GraphicsFormat.R8_UNorm,
            LodTextureFormatMode.Precision => GraphicsFormat.R16_UNorm,
            LodTextureFormatMode.Manual => _TextureFormat,
            _ => throw new System.NotImplementedException(),
        };

        internal ClipLod()
        {
            _TextureFormat = GraphicsFormat.R8_UNorm;
        }

        public override void Enable()
        {
            base.Enable();

            if (!_Enabled)
            {
                Shader.SetGlobalFloat(ShaderIDs.s_ClipByDefault, (float)DefaultClippingState.NothingClipped);
            }
        }

        public override void Disable()
        {
            base.Disable();
            Shader.SetGlobalFloat(ShaderIDs.s_ClipByDefault, (float)DefaultClippingState.NothingClipped);
        }

        internal override void BuildCommandBuffer(WaterRenderer water, CommandBuffer buffer)
        {
            base.BuildCommandBuffer(water, buffer);
            Shader.SetGlobalFloat(ShaderIDs.s_ClipByDefault, (float)_DefaultClippingState);
        }

        internal static readonly SortedList<int, ILodInput> s_Inputs = new(Helpers.DuplicateComparison);
        private protected override SortedList<int, ILodInput> Inputs => s_Inputs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnLoad()
        {
            s_Inputs.Clear();
        }

#if UNITY_EDITOR
        [@OnChange]
        void OnChange(string path, object previous)
        {
            // Change default clipping state.
            _TargetsToClear = Mathf.Max(1, _TargetsToClear);
        }
#endif
    }
}
