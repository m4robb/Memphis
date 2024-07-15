// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Rendering;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// A persistent simulation that moves around with a displacement LOD.
    /// </summary>
    abstract class PersistentLod : Lod
    {
        [Tooltip("Frequency to run the simulation, in updates per second. Lower frequencies are more efficient but may lead to visible jitter or slowness.")]
        [@Range(15, 200, order = -1000)]
        [SerializeField]
        protected int _SimulationFrequency = 60;

        public static new class ShaderIDs
        {
            public static readonly int s_SimDeltaTime = Shader.PropertyToID("_Crest_SimDeltaTime");
            public static readonly int s_SimDeltaTimePrev = Shader.PropertyToID("_Crest_SimDeltaTimePrev");
        }

        protected override bool NeedToReadWriteTextureData => true;
        internal override int BufferCount => 2;
        internal int SimulationFrequency => _SimulationFrequency;

        RenderTexture _Sources;
        float _PreviousSubstepDeltaTime = 1f / 60f;

        // Is this the first step since being enabled?
        protected bool _NeedsPrewarmingThisStep = true;

        // This is how far the simulation time is behind Unity's time.
        protected float _TimeToSimulate = 0f;

        internal int LastUpdateSubstepCount { get; private set; }

        protected abstract ComputeShader SimulationShader { get; }
        protected abstract void GetSubstepData(float timeToSimulate, out int substeps, out float delta);

        public override void Enable()
        {
            if (SimulationShader == null)
            {
                _Valid = false;
                return;
            }

            base.Enable();

            _NeedsPrewarmingThisStep = true;
        }

        public override void Disable()
        {
            base.Disable();

            _Sources.Release();
            Helpers.Destroy(_Sources);
        }

        protected override void Allocate()
        {
            base.Allocate();

            var descriptor = new RenderTextureDescriptor(Resolution, Resolution, CompatibleTextureFormat, 0);
            _Sources = CreateLodDataTextures(descriptor, $"{ID}Lod_Temporary", NeedToReadWriteTextureData);
            Clear(_Sources);

            _Targets.RunLambda(buffer => Clear(buffer));
        }

        internal override void ClearLodData()
        {
            base.ClearLodData();
            _Targets.RunLambda(x => Clear(x));
            Clear(_Sources);
        }

        internal override void BuildCommandBuffer(WaterRenderer water, CommandBuffer buffer)
        {
            buffer.BeginSample(Name);

            FlipBuffers();

            var slices = water.LodLevels;

            // How far are we behind.
            _TimeToSimulate += water.DeltaTime;

            // Do a set of substeps to catch up.
            GetSubstepData(_TimeToSimulate, out var substeps, out var delta);

            LastUpdateSubstepCount = substeps;

            // Even if no steps were needed this frame, the simulation still needs to advect to
            // compensate for camera motion / water scale changes, so do a trivial substep.
            // This could be a specialised kernel that only advects, or the simulation shader
            // could have a branch for 0 delta time.
            if (substeps == 0)
            {
                substeps = 1;
                delta = 0f;
            }

            var current = DataTexture;

            for (var substep = 0; substep < substeps; substep++)
            {
                var isFirstStep = substep == 0;
                var frame = isFirstStep ? 1 : 0;
                var wrapper = new PropertyWrapperCompute(buffer, SimulationShader, 0);

                // Record how much we caught up
                _TimeToSimulate -= delta;

                // Buffers are already flipped, but we need to ping-pong for subsequent substeps.
                if (!isFirstStep)
                {
                    // Use temporary target for ping-pong instead of flipping buffer. We do not want
                    // to buffer substeps as they will not match buffered cascade data etc. Each buffer
                    // entry must be for a single frame and substeps are "sub-frame".
                    (_Sources, current) = (current, _Sources);
                }
                else
                {
                    // We only want to handle teleports for the first step.
                    _NeedsPrewarmingThisStep = _NeedsPrewarmingThisStep || _Water._HasTeleportedThisFrame;
                }

                // Both simulation update and input draws need delta time.
                buffer.SetGlobalFloat(ShaderIDs.s_SimDeltaTime, delta);
                buffer.SetGlobalFloat(ShaderIDs.s_SimDeltaTimePrev, _PreviousSubstepDeltaTime);

                wrapper.SetTexture(Crest.ShaderIDs.s_Target, current);
                wrapper.SetTexture(_TextureSourceShaderID, isFirstStep ? _Targets.Previous(1) : _Sources);

                // Compute which LOD data we are sampling source data from. if a scale change has
                // happened this can be any LOD up or down the chain. This is only valid on the
                // first update step, after that the scale source/target data are in the right
                // places.
                wrapper.SetFloat(Lod.ShaderIDs.s_LodChange, isFirstStep ? _Water.ScaleDifferencePower2 : 0);

                wrapper.SetVectorArray(WaterRenderer.ShaderIDs.s_CascadeDataSource, _Water._CascadeData.Previous(frame));
                wrapper.SetVectorArray(_SamplingParametersCascadeSourceShaderID, _SamplingParameters.Previous(frame));

                SetAdditionalSimulationParameters(wrapper);

                var threads = Resolution / k_ThreadGroupSize;
                wrapper.Dispatch(threads, threads, Slices);

                // Only add forces if we did a step.
                if (delta > 0f)
                {
                    SubmitDraws(buffer, Inputs, current);
                }

                // The very first step since being enabled.
                _NeedsPrewarmingThisStep = false;
                _PreviousSubstepDeltaTime = delta;
            }

            _Targets.Current = current;

            // Any post-simulation steps.
            for (var slice = slices - 1; slice >= 0; slice--)
            {
                BuildCommandBufferInternal(slice);
            }

            // Set the target texture as to make sure we catch the 'pong' each frame.
            Shader.SetGlobalTexture(_TextureShaderID, DataTexture);

            buffer.EndSample(Name);
        }

        protected virtual bool BuildCommandBufferInternal(int slice)
        {
            return true;
        }

        /// <summary>
        /// Set any simulation specific shader parameters.
        /// </summary>
        protected virtual void SetAdditionalSimulationParameters<T>(T properties) where T : IPropertyWrapper
        {
        }
    }
}
