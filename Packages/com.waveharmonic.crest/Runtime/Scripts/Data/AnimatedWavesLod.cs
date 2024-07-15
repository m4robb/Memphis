// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace WaveHarmonic.Crest
{
    abstract class BakedWaveData : ScriptableObject
    {
        public abstract ICollisionProvider CreateCollisionProvider();
        public abstract float WindSpeed { get; }
    }

    /// <summary>
    /// Captures waves/shape that is drawn kinematically - there is no frame-to-frame
    /// state.
    ///
    ///  * A combine pass is done which combines downwards from low detail LODs down into
    ///    the high detail LODs.
    ///  * The textures from this LodData are passed to the water material when the surface
    ///    is drawn.
    ///  * LodDataDynamicWaves adds its results into this LodData. The dynamic waves piggy
    ///    back off the combine pass and subsequent assignment to the water material.
    ///
    /// The RGB channels are the XYZ displacement from a rest plane at water level to
    /// the corresponding displaced position on the surface.
    /// </summary>
    [@FilterEnum(nameof(_TextureFormatMode), Filtered.Mode.Exclude, (int)LodTextureFormatMode.Automatic)]
    sealed partial class AnimatedWavesLod : Lod
    {
        [@Space(10)]

        [Tooltip("PREVIEW: Set this to 2 to improve wave quality. In some cases like flowing rivers this can make a substantial difference to visual stability.\n\nWe recommend doubling the Lod Data Resolution on the WaterRenderer component to preserve detail after making this change, but note that this will consume 4x more video memory until we are able to optimise data usage further, so apply this change with caution.")]
        [@Range(1f, 4f)]
        [SerializeField]
        internal float _WaveResolutionMultiplier = 1f;

        [Tooltip("How much waves are dampened in shallow water.")]
        [@Range(0f, 1f)]
        [SerializeField]
        float _AttenuationInShallows = 0.95f;

        [Tooltip("Any water deeper than this will receive full wave strength. The lower the value, the less effective the depth cache will be at attenuating very large waves. Set to the maximum value (1,000) to disable.")]
        [@Range(1f, 1000f)]
        [SerializeField]
        float _ShallowsMaximumDepth = 1000f;


        [@Heading("Collisions")]

        [Tooltip("Where to obtain water shape on CPU for physics / gameplay.")]
        [@DecoratedField, SerializeField]
        CollisionSources _CollisionSource = CollisionSources.GPU;

        [Tooltip("Maximum number of wave queries that can be performed when using GPU queries.")]
        [@Predicated(nameof(_CollisionSource), true, nameof(CollisionSources.GPU))]
        [@DecoratedField, SerializeField]
        int _MaximumQueryCount = QueryBase.k_DefaultMaximumQueryCount;

        [@Predicated(nameof(_CollisionSource), true, nameof(CollisionSources.CPU))]
        [@DecoratedField, SerializeField]
        internal BakedWaveData _BakedWaveData;


        internal static new partial class ShaderIDs
        {
            public static readonly int s_WaveBuffer = Shader.PropertyToID("_Crest_WaveBuffer");
            public static readonly int s_AttenuationInShallows = Shader.PropertyToID("_Crest_AttenuationInShallows");
        }

        internal static readonly Color s_GizmoColor = new(0f, 1f, 0f, 0.5f);

        /// <summary>
        /// Turn shape combine pass on/off. Debug only - stripped in builds.
        /// </summary>
        public static bool s_Combine = true;

        internal const int k_PassPreCombine = 0;
        internal const int k_PassPostCombine = 1;

        internal override string ID => "AnimatedWaves";
        internal override string Name => "Animated Waves";
        internal override Color GizmoColor => s_GizmoColor;
        protected override bool NeedToReadWriteTextureData => true;
        protected override Color ClearColor => Color.black;
        internal override int BufferCount => _BufferCount;
        internal override bool RunsInHeadless => true;

        // NOTE: Tried RGB111110Float but errors becomes visible. One option would be to use a UNORM setup.
        protected override GraphicsFormat RequestedTextureFormat => _TextureFormatMode switch
        {
            LodTextureFormatMode.Performance => GraphicsFormat.R16G16B16A16_SFloat,
            LodTextureFormatMode.Precision => GraphicsFormat.R32G32B32A32_SFloat,
            LodTextureFormatMode.Manual => _TextureFormat,
            _ => throw new System.NotImplementedException(),
        };

        ComputeShader _CombineShader;
        internal RenderTexture _WaveBuffers;
        int _BufferCount = 1;

        int _KernalShapeCombine = -1;
        int _KernalShapeCombine_DISABLE_COMBINE = -1;
        int _KernalShapeCombine_FLOW_ON = -1;
        int _KernalShapeCombine_FLOW_ON_DISABLE_COMBINE = -1;
        int _KernalShapeCombine_DYNAMIC_WAVE_SIM_ON = -1;
        int _KernalShapeCombine_DYNAMIC_WAVE_SIM_ON_DISABLE_COMBINE = -1;
        int _KernalShapeCombine_FLOW_ON_DYNAMIC_WAVE_SIM_ON = -1;
        int _KernalShapeCombine_FLOW_ON_DYNAMIC_WAVE_SIM_ON_DISABLE_COMBINE = -1;

        public float WaveResolutionMultiplier => _WaveResolutionMultiplier;
        public float AttenuationInShallows => _AttenuationInShallows;
        public float MaximumAttenuationDepth => _ShallowsMaximumDepth;
        public CollisionSources CollisionSource { get => _CollisionSource; set => _CollisionSource = value; }
        public int MaxQueryCount => _MaximumQueryCount;

        internal AnimatedWavesLod()
        {
            _Enabled = true;
            _OverrideResolution = false;
            _TextureFormat = GraphicsFormat.R16G16B16A16_SFloat;
        }

        public override void AddToHash(ref int hash)
        {
            base.AddToHash(ref hash);
            Utility.Hash.AddBool(Helpers.IsMotionVectorsEnabled(), ref hash);
            Utility.Hash.AddInt((int)_CollisionSource, ref hash);
        }


        public override void Enable()
        {
            _CombineShader = WaterResources.Instance.Compute._ShapeCombine;
            if (_CombineShader == null)
            {
                _Valid = false;
                return;
            }

            base.Enable();
        }

        public override void Disable()
        {
            base.Disable();
            _WaveBuffers.Release();
            Helpers.Destroy(_WaveBuffers);
        }

        protected override void Allocate()
        {
            _BufferCount = Helpers.IsMotionVectorsEnabled() ? 2 : 1;

            base.Allocate();

            // Setup the RenderTexture and compute shader for combining different Animated Wave
            // LODs. As we use a single texture array for all LODs, we employ a compute shader
            // as only they can read and write to the same texture.
            var descriptor = new RenderTextureDescriptor(Resolution, Resolution, CompatibleTextureFormat, 0);
            _WaveBuffers = CreateLodDataTextures(descriptor, "AnimatedWavesWaveBuffer", true);

            _KernalShapeCombine = _CombineShader.FindKernel("ShapeCombine");
            _KernalShapeCombine_DISABLE_COMBINE = _CombineShader.FindKernel("ShapeCombine_DISABLE_COMBINE");
            _KernalShapeCombine_FLOW_ON = _CombineShader.FindKernel("ShapeCombine_FLOW_ON");
            _KernalShapeCombine_FLOW_ON_DISABLE_COMBINE = _CombineShader.FindKernel("ShapeCombine_FLOW_ON_DISABLE_COMBINE");
            _KernalShapeCombine_DYNAMIC_WAVE_SIM_ON = _CombineShader.FindKernel("ShapeCombine_DYNAMIC_WAVE_SIM_ON");
            _KernalShapeCombine_DYNAMIC_WAVE_SIM_ON_DISABLE_COMBINE = _CombineShader.FindKernel("ShapeCombine_DYNAMIC_WAVE_SIM_ON_DISABLE_COMBINE");
            _KernalShapeCombine_FLOW_ON_DYNAMIC_WAVE_SIM_ON = _CombineShader.FindKernel("ShapeCombine_FLOW_ON_DYNAMIC_WAVE_SIM_ON");
            _KernalShapeCombine_FLOW_ON_DYNAMIC_WAVE_SIM_ON_DISABLE_COMBINE = _CombineShader.FindKernel("ShapeCombine_FLOW_ON_DYNAMIC_WAVE_SIM_ON_DISABLE_COMBINE");
        }

        internal override void BuildCommandBuffer(WaterRenderer water, CommandBuffer buffer)
        {
            buffer.BeginSample(Name);

            FlipBuffers();

            Shader.SetGlobalFloat(ShaderIDs.s_AttenuationInShallows, _AttenuationInShallows);

            foreach (var updatable in s_Updatables)
            {
                if (!updatable.Enabled) continue;
                updatable.UpdateShape(_Water, buffer);
            }

            // Clear target.
            buffer.SetRenderTarget(_WaveBuffers, 0, CubemapFace.Unknown, -1);
            buffer.ClearRenderTarget(false, true, ClearColor);

            // LOD dependent data.
            // Write to per-octave _WaveBuffers. Not the same as _AnimatedWaves.
            // Draw any data with lod preference.
            SubmitDraws(buffer, s_Inputs, _WaveBuffers, k_PassPreCombine, filter: true);

            buffer.BeginSample("Combine");

            // Combine the LODs - copy results from biggest LOD down to LOD 0
            Combine(buffer);

            buffer.EndSample("Combine");

            // LOD independent data.
            // Draw any data that did not express a preference for one lod or another.
            var drawn = SubmitDraws(buffer, s_Inputs, DataTexture, k_PassPostCombine);

            // Alpha channel is cleared in combine step, but if any inputs draw in post-combine
            // step then alpha may have data.
            var clear = WaterResources.Instance.Compute._Clear;
            if (drawn && clear != null)
            {
                buffer.SetComputeTextureParam(clear, 0, Crest.ShaderIDs.s_Target, DataTexture);
                buffer.SetComputeVectorParam(clear, Crest.ShaderIDs.s_ClearMask, Color.black);
                buffer.SetComputeVectorParam(clear, Crest.ShaderIDs.s_ClearColor, Color.clear);
                buffer.DispatchCompute
                (
                    clear,
                    0,
                    Resolution / k_ThreadGroupSizeX,
                    Resolution / k_ThreadGroupSizeY,
                    Slices
                );
            }

            // Pack height data into alpha channel.
            // We do not add height to displacement directly for better precision and layering.
            var heightShader = WaterResources.Instance.Compute._PackLevel;
            if (_Water._LevelLod.Enabled && heightShader != null)
            {
                buffer.SetComputeTextureParam(heightShader, 0, Crest.ShaderIDs.s_Target, DataTexture);
                buffer.DispatchCompute
                (
                    heightShader,
                    0,
                    Resolution / k_ThreadGroupSizeX,
                    Resolution / k_ThreadGroupSizeY,
                    Slices
                );
            }

            if (BufferCount > 1)
            {
                // Update current and previous. Latter for MVs and/or VFX.
                Shader.SetGlobalTexture(_TextureSourceShaderID, _Targets.Previous(1));
                Shader.SetGlobalTexture(_TextureShaderID, DataTexture);
            }

            buffer.EndSample(Name);
        }

        void Combine(CommandBuffer buffer)
        {
            var slices = Slices;

            var combineShaderKernel = _KernalShapeCombine;
            var combineShaderKernel_lastLOD = _KernalShapeCombine_DISABLE_COMBINE;
            {
                var isFlowOn = _Water._FlowLod.Enabled;
                var isDynWavesOn = _Water._DynamicWavesLod.Enabled;
                // set the shader kernels that we will use.
                if (isFlowOn && isDynWavesOn)
                {
                    combineShaderKernel = _KernalShapeCombine_FLOW_ON_DYNAMIC_WAVE_SIM_ON;
                    combineShaderKernel_lastLOD = _KernalShapeCombine_FLOW_ON_DYNAMIC_WAVE_SIM_ON_DISABLE_COMBINE;
                }
                else if (isFlowOn)
                {
                    combineShaderKernel = _KernalShapeCombine_FLOW_ON;
                    combineShaderKernel_lastLOD = _KernalShapeCombine_FLOW_ON_DISABLE_COMBINE;
                }
                else if (isDynWavesOn)
                {
                    combineShaderKernel = _KernalShapeCombine_DYNAMIC_WAVE_SIM_ON;
                    combineShaderKernel_lastLOD = _KernalShapeCombine_DYNAMIC_WAVE_SIM_ON_DISABLE_COMBINE;
                }
            }

            // Combine waves.
            for (var slice = slices - 1; slice >= 0; slice--)
            {
                var kernel = slice < slices - 1 && s_Combine
                    ? combineShaderKernel : combineShaderKernel_lastLOD;

                var wrapper = new PropertyWrapperCompute(buffer, _CombineShader, kernel);

                // The per-octave wave buffers we read from.
                wrapper.SetTexture(ShaderIDs.s_WaveBuffer, _WaveBuffers);

                if (_Water._DynamicWavesLod.Enabled) _Water._DynamicWavesLod.Bind(wrapper);

                // Set the animated waves texture where we read/write to combine the results. Use
                // compute suffix to avoid collision as a file already uses the normal name.
                wrapper.SetTexture(Crest.ShaderIDs.s_Target, DataTexture);
                wrapper.SetInteger(Lod.ShaderIDs.s_LodIndex, slice);

                buffer.DispatchCompute
                (
                    _CombineShader,
                    kernel,
                    Resolution / k_ThreadGroupSizeX,
                    Resolution / k_ThreadGroupSizeY,
                    1
                );
            }
        }


        //
        // Collision
        //

        public enum CollisionSources
        {
            None = 0,
            // GerstnerWavesCPU = 1,
            GPU = 2,
            CPU = 3,
        }

        /// <summary>
        /// Provides water shape to CPU.
        /// </summary>
        internal ICollisionProvider CreateCollisionProvider()
        {
            ICollisionProvider result = null;

            switch (_CollisionSource)
            {
                case CollisionSources.None:
                    result = ICollisionProvider.None;
                    break;
                case CollisionSources.GPU:
                    if (!WaterRenderer.RunningWithoutGraphics)
                    {
                        result = new CollisionQuery(_Water);
                    }
                    else
                    {
                        Debug.LogError($"Crest: GPU queries not supported in headless/batch mode. To resolve, assign an Animated Wave Settings asset to the {nameof(WaterRenderer)} component and set the Collision Source to be a CPU option.");
                    }
                    break;
                case CollisionSources.CPU:
                    if (_BakedWaveData != null)
                    {
                        result = _BakedWaveData.CreateCollisionProvider();
                    }
                    break;
            }

            if (result == null)
            {
                // This should not be hit, but can be if compute shaders aren't loaded correctly.
                // They will print out appropriate errors. Don't just return null and have null reference
                // exceptions spamming the logs.
                return ICollisionProvider.None;
            }

            return result;
        }


        //
        // ShapeUpdatable
        //

        public interface IShapeUpdatable
        {
            bool Enabled { get; }

            // Mainly used for reporting displacements.
            void UpdateShape(WaterRenderer water, CommandBuffer buffer);
        }

        static readonly List<IShapeUpdatable> s_Updatables = new();

        public static void Attach(IShapeUpdatable updatable)
        {
            s_Updatables.Remove(updatable);
            s_Updatables.Add(updatable);
        }

        public static void Detach(IShapeUpdatable updatable)
        {
            s_Updatables.Remove(updatable);
        }


        //
        // DrawFilter
        //

        internal readonly struct WavelengthFilter
        {
            public readonly float _Minimum;
            public readonly float _Maximum;
            public readonly float _TransitionThreshold;
            public readonly float _ViewerAltitudeLevelAlpha;
            public readonly int _Slice;
            public readonly int _Slices;

            public WavelengthFilter(WaterRenderer water, int slice)
            {
                _Slice = slice;
                _Slices = water.LodLevels;
                _Maximum = water.MaximumWavelength(slice);
                _Minimum = _Maximum * 0.5f;
                _TransitionThreshold = water.MaximumWavelength(_Slices - 1) * 0.5f;
                _ViewerAltitudeLevelAlpha = water.ViewerAltitudeLevelAlpha;
            }
        }

        internal static float FilterByWavelength(WavelengthFilter filter, float wavelength)
        {
            // No wavelength preference - don't draw per-lod
            if (wavelength == 0f)
            {
                return 0f;
            }

            // Too small for this lod
            if (wavelength < filter._Minimum)
            {
                return 0f;
            }

            // If approaching end of lod chain, start smoothly transitioning any large wavelengths across last two lods
            if (wavelength >= filter._TransitionThreshold)
            {
                if (filter._Slice == filter._Slices - 2)
                {
                    return 1f - filter._ViewerAltitudeLevelAlpha;
                }

                if (filter._Slice == filter._Slices - 1)
                {
                    return filter._ViewerAltitudeLevelAlpha;
                }
            }
            else if (wavelength < filter._Maximum)
            {
                // Fits in this lod
                return 1f;
            }

            return 0f;
        }

        internal static float FilterByWavelength(WaterRenderer water, int slice, float wavelength)
        {
            return FilterByWavelength(new(water, slice), wavelength);
        }


        //
        // Inputs
        //

        internal static readonly Utility.SortedList<int, ILodInput> s_Inputs = new(Helpers.DuplicateComparison);
        private protected override Utility.SortedList<int, ILodInput> Inputs => s_Inputs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnLoad()
        {
            s_Inputs.Clear();
        }
    }
}
