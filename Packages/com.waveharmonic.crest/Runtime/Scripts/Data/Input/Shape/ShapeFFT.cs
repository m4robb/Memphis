// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Rendering;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// FFT water wave shape
    /// </summary>
    [AddComponentMenu(Constants.k_MenuPrefixInputs + "Shape FFT")]
    sealed partial class ShapeFFT : ShapeWaves
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414


        // Waves

        [Tooltip("How turbulent/chaotic the waves are.")]
        [@Range(0, 1, order = -3)]
        public float _WindTurbulence = 0.145f;

        [Tooltip("How aligned waves are with wind.")]
        [@Range(0, 1, order = -4)]
        public float _WindAlignment;


        // Generation

        [Tooltip("FFT waves will loop with a period of this many seconds.")]
        [@Range(4f, 128f, Range.Clamp.Minimum)]
        [SerializeField]
        float _TimeLoopLength = Mathf.Infinity;


        [Header("Culling")]

        [Tooltip("Maximum amount surface will be displaced vertically from sea level. Increase this if gaps appear at bottom of screen.")]
        [SerializeField]
        float _MaximumVerticalDisplacement = 10f;

        [Tooltip("Maximum amount a point on the surface will be displaced horizontally by waves from its rest position. Increase this if gaps appear at sides of screen.")]
        [SerializeField]
        float _MaximumHorizontalDisplacement = 15f;


        [@Heading("Collision Data Baking")]

        [Tooltip("Enable running this FFT with baked data. This makes the FFT periodic (repeating in time).")]
        [@Predicated(nameof(_Mode), inverted: true, nameof(LodInputMode.Global), hide: true)]
        [@DecoratedField, SerializeField]
        internal bool _EnableBakedCollision = false;

        [Tooltip("Frames per second of baked data. Larger values may help the collision track the surface closely at the cost of more frames and increase baked data size.")]
        [@Predicated(nameof(_EnableBakedCollision))]
        [@Predicated(nameof(_Mode), inverted: true, nameof(LodInputMode.Global), hide: true)]
        [@DecoratedField, SerializeField]
        internal int _TimeResolution = 4;

        [Tooltip("Smallest wavelength required in collision. To preview the effect of this, disable power sliders in spectrum for smaller values than this number. Smaller values require more resolution and increase baked data size.")]
        [@Predicated(nameof(_EnableBakedCollision))]
        [@Predicated(nameof(_Mode), inverted: true, nameof(LodInputMode.Global), hide: true)]
        [@DecoratedField, SerializeField]
        internal float _SmallestWavelengthRequired = 2f;

        [Tooltip("FFT waves will loop with a period of this many seconds. Smaller values decrease data size but can make waves visibly repetitive.")]
        [@Predicated(nameof(_EnableBakedCollision))]
        [@Predicated(nameof(_Mode), inverted: true, nameof(LodInputMode.Global), hide: true)]
        [@Range(4f, 128f)]
        [SerializeField]
        internal float _BakedTimeLoopLength = 32f;

        internal float LoopPeriod => _EnableBakedCollision ? _BakedTimeLoopLength : _TimeLoopLength;

        protected override int MinimumResolution => 16;
        protected override int MaximumResolution => int.MaxValue;

        FFTCompute.Parameters _OldFFTParameters;
        internal FFTCompute.Parameters FFTParameters => new
        (
            _ActiveSpectrum,
            WindSpeedMPS,
            WindDirRadForFFT,
            _WindTurbulence,
            _WindAlignment
        );

        protected override void OnLateUpdate(WaterRenderer water)
        {
            base.OnLateUpdate(water);

            // We do not filter FFTs.
            _FirstCascade = 0;
            _LastCascade = k_CascadeCount - 1;

            // If geometry is being used, the water input shader will rotate the waves to align to geo
            var parameters = FFTParameters;

            // Don't create tons of generators when values are varying. Notify so that existing generators may be adapted.
            if (parameters.GetHashCode() != _OldFFTParameters.GetHashCode())
            {
                FFTCompute.OnGenerationDataUpdated
                (
                    _Resolution,
                    LoopPeriod,
                    _OldFFTParameters,
                    parameters
                );
            }

            _OldFFTParameters = parameters;
        }

        public override void UpdateShape(WaterRenderer water, CommandBuffer buf)
        {
            base.UpdateShape(water, buf);

            _WaveBuffers = FFTCompute.GenerateDisplacements
            (
                buf,
                _Resolution,
                LoopPeriod,
                water.CurrentTime,
                FFTParameters,
                UpdateDataEachFrame
            );
        }

        protected override void SetRenderParameters<T>(WaterRenderer water, T wrapper)
        {
            base.SetRenderParameters(water, wrapper);

            // If using geometry, the primary wave direction is used by the input shader to
            // rotate the waves relative to the geo rotation. If not, the wind direction is
            // already used in the FFT generation.
            var waveDir = (Mode is LodInputMode.Spline or LodInputMode.Paint) ? PrimaryWaveDirection : Vector2.right;
            wrapper.SetVector(ShaderIDs.s_AxisX, waveDir);
        }

        protected override void ReportMaxDisplacement(WaterRenderer water)
        {
            // Apply weight or will cause popping due to scale change.
            MaximumHorizontalDisplacement = _MaximumHorizontalDisplacement * _Weight;
            MaximumVerticalDisplacement = MaximumWavesDisplacement = _MaximumVerticalDisplacement * _Weight;

            if (Mode == LodInputMode.Global)
            {
                water.ReportMaximumDisplacement(MaximumHorizontalDisplacement, MaximumVerticalDisplacement, MaximumVerticalDisplacement);
            }
        }

        protected override void DestroySharedResources()
        {
            FFTCompute.CleanUpAll();
        }

        public float WindDirRadForFFT
        {
            get
            {
                // These input types use a wave direction provided by geometry or the painted user direction
                if (Mode is LodInputMode.Spline or LodInputMode.Paint)
                {
                    return 0f;
                }

                return _WaveDirectionHeadingAngle * Mathf.Deg2Rad;
            }
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            if (_DrawSlicesInEditor)
            {
                FFTCompute.GetInstance(_Resolution, LoopPeriod, FFTParameters)?.OnGUI();
            }
        }

        internal FFTCompute GetFFTComputeInstance()
        {
            return FFTCompute.GetInstance(_Resolution, LoopPeriod, FFTParameters);
        }
#endif
    }
}
