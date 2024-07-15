// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using WaveHarmonic.Crest.Editor;
using WaveHarmonic.Crest.Utility;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// A dynamic shape simulation that moves around with a displacement LOD.
    /// </summary>
    sealed partial class DynamicWavesLod : PersistentLod
#if UNITY_EDITOR
        , IOptionalDynamicWavesLod
#endif
    {
        [Tooltip("How much waves are dampened in shallow water.")]
        [@Range(0f, 1f)]
        [SerializeField]
        internal float _AttenuationInShallows = 1f;

        [@Embedded]
        [SerializeField]
        internal DynamicWavesLodSettings _Settings;

        const string k_DynamicWavesKeyword = "CREST_DYNAMIC_WAVE_SIM_ON_INTERNAL";

        static new class ShaderIDs
        {
            public static readonly int s_HorizontalDisplace = Shader.PropertyToID("_Crest_HorizontalDisplace");
            public static readonly int s_DisplaceClamp = Shader.PropertyToID("_Crest_DisplaceClamp");
            public static readonly int s_Damping = Shader.PropertyToID("_Crest_Damping");
            public static readonly int s_Gravity = Shader.PropertyToID("_Crest_Gravity");
            public static readonly int s_CourantNumber = Shader.PropertyToID("_Crest_CourantNumber");
        }

        internal static readonly Color s_GizmoColor = new(0f, 1f, 0f, 0.5f);

        internal override string ID => "DynamicWaves";
        internal override string Name => "Dynamic Waves";
        internal override Color GizmoColor => s_GizmoColor;
        protected override Color ClearColor => Color.black;
        internal override bool RunsInHeadless => true;
        protected override ComputeShader SimulationShader => WaterResources.Instance.Compute._UpdateDynamicWaves;
        protected override GraphicsFormat RequestedTextureFormat => _TextureFormatMode switch
        {
            // Try and match Animated Waves format as we copy this simulation into it.
            LodTextureFormatMode.Automatic => Water == null ? GraphicsFormat.None : Water.AnimatedWavesLod.TextureFormatMode switch
            {
                LodTextureFormatMode.Precision => GraphicsFormat.R32G32_SFloat,
                _ => GraphicsFormat.R16G16_SFloat,
            },
            LodTextureFormatMode.Performance => GraphicsFormat.R16G16_SFloat,
            LodTextureFormatMode.Precision => GraphicsFormat.R32G32_SFloat,
            LodTextureFormatMode.Manual => _TextureFormat,
            _ => throw new System.NotImplementedException(),
        };


        bool[] _Active;
        bool Active(int slice) => _Active[slice];

        internal float TimeLeftToSimulate => _TimeToSimulate;

        internal DynamicWavesLod()
        {
            _OverrideResolution = false;
            _Resolution = 512;
            _TextureFormatMode = LodTextureFormatMode.Automatic;
            _TextureFormat = GraphicsFormat.R16G16_SFloat;
        }

        public override void Enable()
        {
            base.Enable();

            Shader.EnableKeyword(k_DynamicWavesKeyword);
        }

        public override void Disable()
        {
            base.Disable();

            Shader.DisableKeyword(k_DynamicWavesKeyword);
        }

        protected override void Allocate()
        {
            base.Allocate();

            _Active = new bool[Slices];
            for (var i = 0; i < _Active.Length; i++) _Active[i] = true;
        }

        protected override bool BuildCommandBufferInternal(int slice)
        {
            if (!base.BuildCommandBufferInternal(slice)) return false;

            // Check if the simulation should be running.
            var texel = _SamplingParameters.Current[slice].z;
            _Active[slice] = texel >= Settings._MinimumGridSize && (texel <= Settings._MaximumGridSize || Settings._MaximumGridSize == 0f);

            return true;
        }

        internal override void Bind<T>(T target)
        {
            base.Bind(target);
            target.SetFloat(ShaderIDs.s_HorizontalDisplace, Settings._HorizontalDisplace);
            target.SetFloat(ShaderIDs.s_DisplaceClamp, Settings._DisplaceClamp);
        }

        protected override void SetAdditionalSimulationParameters<T>(T simMaterial)
        {
            base.SetAdditionalSimulationParameters(simMaterial);

            simMaterial.SetFloat(ShaderIDs.s_Damping, Settings._Damping);
            simMaterial.SetFloat(ShaderIDs.s_Gravity, _Water.Gravity * Settings._GravityMultiplier);
            simMaterial.SetFloat(ShaderIDs.s_CourantNumber, Settings._CourantNumber);
            simMaterial.SetFloat(AnimatedWavesLod.ShaderIDs.s_AttenuationInShallows, _AttenuationInShallows);
        }

        protected override void GetSubstepData(float timeToSimulate, out int substeps, out float delta)
        {
            substeps = Mathf.FloorToInt(timeToSimulate * _SimulationFrequency);
            delta = substeps > 0 ? (1f / _SimulationFrequency) : 0f;
        }

        public void CountWaveSims(int from, out int present, out int active)
        {
            present = Slices;
            active = 0;
            for (var i = 0; i < present; i++)
            {
                if (i < from) continue;
                if (!Active(i)) continue;

                active++;
            }
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
