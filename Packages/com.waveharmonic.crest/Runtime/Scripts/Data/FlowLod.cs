// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using WaveHarmonic.Crest.Editor;
using WaveHarmonic.Crest.Utility;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// A persistent flow simulation that moves around with a displacement LOD. The input is fully combined water surface shape.
    /// </summary>
    [FilterEnum(nameof(_TextureFormatMode), Filtered.Mode.Exclude, (int)LodTextureFormatMode.Automatic)]
    sealed partial class FlowLod : Lod
#if UNITY_EDITOR
        , IOptionalFlowLod
#endif
    {
        const string k_FlowKeyword = "CREST_FLOW_ON_INTERNAL";

        internal static readonly Color s_GizmoColor = new(0f, 0f, 1f, 0.5f);

        internal override string ID => "Flow";
        internal override Color GizmoColor => s_GizmoColor;
        protected override Color ClearColor => Color.black;
        protected override bool NeedToReadWriteTextureData => true;
        internal override bool RunsInHeadless => true;

        protected override GraphicsFormat RequestedTextureFormat => _TextureFormatMode switch
        {
            LodTextureFormatMode.Performance => GraphicsFormat.R16G16_SFloat,
            LodTextureFormatMode.Precision => GraphicsFormat.R32G32_SFloat,
            LodTextureFormatMode.Manual => _TextureFormat,
            _ => throw new System.NotImplementedException(),
        };

        internal FlowLod()
        {
            _Resolution = 128;
            _TextureFormat = GraphicsFormat.R16G16_SFloat;
        }

        public override void Enable()
        {
            base.Enable();

            Shader.EnableKeyword(k_FlowKeyword);
        }

        public override void Disable()
        {
            base.Disable();

            Shader.DisableKeyword(k_FlowKeyword);
        }

        internal IFlowProvider CreateFlowProvider()
        {
            // Flow is GPU only, and can only be queried using the compute path
            if (Enabled)
            {
                return new FlowQuery(_Water);
            }

            return IFlowProvider.None;
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
