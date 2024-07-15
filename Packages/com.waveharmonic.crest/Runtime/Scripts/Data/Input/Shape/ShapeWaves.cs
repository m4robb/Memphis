// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// FFT water wave shape
    /// </summary>
    [@ExecuteDuringEditMode(ExecuteDuringEditMode.Include.None)]
    [@HelpURL("Manual/Waves.html#wave-conditions")]
    [@FilterEnum(nameof(_Blend), Filtered.Mode.Include, (int)Blend.Additive, (int)Blend.Alpha)]
    abstract partial class ShapeWaves : LodInput
        , AnimatedWavesLod.IShapeUpdatable
        , IReportsDisplacement
    {
        // Input Settings

        [Tooltip("When alpha blending, where will the alpha value come from?")]
        [@Predicated(nameof(_Blend), inverted: true, nameof(Blend.Alpha))]
        [@DecoratedField, SerializeField]
        AlphaSource _AlphaSource;


        [@Heading("Waves")]

        [Tooltip("The spectrum that defines the water surface shape. Assign asset of type WavesSpectrum.")]
        [@Embedded]
        [SerializeField]
        internal WaveSpectrum _Spectrum;

        [Tooltip("When true, the wave spectrum is evaluated once on startup in editor play mode and standalone builds, rather than every frame. This is less flexible but reduces the performance cost significantly.")]
        [SerializeField]
        bool _SpectrumFixedAtRuntime = true;

        [Tooltip("How much these waves respect the shallow water attenuation setting in the Animated Waves Settings. Set to 0 to ignore shallow water.")]
        [@Range(0f, 1f)]
        [SerializeField]
        float _RespectShallowWaterAttenuation = 1f;

        [Tooltip("Primary wave direction heading (deg). This is the angle from x axis in degrees that the waves are oriented towards. If a spline is being used to place the waves, this angle is relative ot the spline.")]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Paint))]
        [@Range(-180, 180)]
        [SerializeField]
        protected float _WaveDirectionHeadingAngle = 0f;

        [Tooltip("When true, uses the wind speed on this component rather than the wind speed from the Water Renderer component.")]
        [SerializeField]
        bool _OverrideGlobalWindSpeed = false;

        [Tooltip("Wind speed in km/h. Controls wave conditions.")]
        [@ShowComputedProperty(nameof(WindSpeedKPH))]
        [@Predicated(nameof(_OverrideGlobalWindSpeed), hide: true)]
        [@Range(0, 150f, scale: 2f)]
        [SerializeField]
        float _WindSpeed = 20f;


        [Header("Generation Settings")]

        [Tooltip("Resolution to use for wave generation buffers. Low resolutions are more efficient but can result in noticeable patterns in the shape.")]
        [@Stepped(16, 512, step: 2, power: true)]
        [SerializeField]
        protected int _Resolution = 128;


        // Debug

        [Tooltip("In Editor, shows the wave generation buffers on screen.")]
        [@DecoratedField(order = k_DebugGroupOrder * Constants.k_FieldGroupOrder), SerializeField]
        internal bool _DrawSlicesInEditor = false;


        protected static new class ShaderIDs
        {
            public static readonly int s_TransitionalWavelengthThreshold = Shader.PropertyToID("_Crest_TransitionalWavelengthThreshold");
            public static readonly int s_WaveResolutionMultiplier = Shader.PropertyToID("_Crest_WaveResolutionMultiplier");
            public static readonly int s_WaveBufferParameters = Shader.PropertyToID("_Crest_WaveBufferParameters");
            public static readonly int s_AlphaSource = Shader.PropertyToID("_Crest_AlphaSource");
            public static readonly int s_WaveBuffer = Shader.PropertyToID("_Crest_WaveBuffer");
            public static readonly int s_WaveBufferSliceIndex = Shader.PropertyToID("_Crest_WaveBufferSliceIndex");
            public static readonly int s_AverageWavelength = Shader.PropertyToID("_Crest_AverageWavelength");
            public static readonly int s_RespectShallowWaterAttenuation = Shader.PropertyToID("_Crest_RespectShallowWaterAttenuation");
            public static readonly int s_MaximumAttenuationDepth = Shader.PropertyToID("_Crest_MaximumAttenuationDepth");
            public static readonly int s_AxisX = Shader.PropertyToID("_Crest_AxisX");
        }

        static WaveSpectrum s_DefaultSpectrum;
        protected static WaveSpectrum DefaultSpectrum
        {
            get
            {
                if (s_DefaultSpectrum == null)
                {
                    s_DefaultSpectrum = ScriptableObject.CreateInstance<WaveSpectrum>();
                    s_DefaultSpectrum.name = "Default Waves (instance)";
                    s_DefaultSpectrum.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                }

                return s_DefaultSpectrum;
            }
        }

        protected abstract int MinimumResolution { get; }
        protected abstract int MaximumResolution { get; }

        static ComputeShader s_TransferWavesComputeShader;
        static LocalKeyword s_KeywordTexture;
        static LocalKeyword s_KeywordTextureBlend;
        readonly Vector4[] _WaveBufferParameters = new Vector4[Lod.k_MaximumSlices];

        internal static int s_RenderPassOverride = -1;

        protected WaveSpectrum _ActiveSpectrum = null;

        public int Resolution => _Resolution;
        public Vector2 PrimaryWaveDirection => new(Mathf.Cos(Mathf.PI * _WaveDirectionHeadingAngle / 180f), Mathf.Sin(Mathf.PI * _WaveDirectionHeadingAngle / 180f));
        public float WindSpeedKPH => _OverrideGlobalWindSpeed || WaterRenderer.Instance == null ? _WindSpeed : WaterRenderer.Instance.WindSpeedKPH;
        public float WindSpeedMPS => WindSpeedKPH / 3.6f;
        public bool OverrideGlobalWindSpeed => _OverrideGlobalWindSpeed;

        protected override void Attach()
        {
            base.Attach();
            AnimatedWavesLod.Attach(this);
        }

        protected override void Detach()
        {
            base.Detach();
            AnimatedWavesLod.Detach(this);
        }

        enum AlphaSource
        {
            AlwaysOne,
            FromZero,
            [InspectorName("From Zero (Normalized)")]
            FromZeroNormalized,
        }

        public override void Draw(Lod simulation, CommandBuffer buffer, RenderTexture target, int pass = -1, float weight = 1f, int slice = -1)
        {
            if (weight * _Weight <= 0f)
            {
                return;
            }

            // Iterating over slices which means this is non compute so pass to graphics draw.
            if (!IsCompute)
            {
                GraphicsDraw(simulation, buffer, target, pass, weight, slice);
                return;
            }

            var lodCount = simulation.Slices;

            var shape = (AnimatedWavesLod)simulation;

            var wrapper = new PropertyWrapperCompute(buffer, s_TransferWavesComputeShader, 0);

            if (_FirstCascade < 0 || _LastCascade < 0)
            {
                return;
            }

            // Write to per-octave _WaveBuffers (ie pre-combined). Not the same as _AnimatedWaves.
            wrapper.SetTexture(Crest.ShaderIDs.s_Target, shape._WaveBuffers);
            // Input weight. Weight for each octave calculated in compute.
            wrapper.SetFloat(LodInput.ShaderIDs.s_Weight, _Weight);

            var water = shape._Water;

            for (var lodIdx = lodCount - 1; lodIdx >= lodCount - slice; lodIdx--)
            {
                _WaveBufferParameters[lodIdx] = new(-1, -2, 0, 0);

                var found = false;
                var filter = new AnimatedWavesLod.WavelengthFilter(water, lodIdx);

                for (var i = _FirstCascade; i <= _LastCascade; i++)
                {
                    _Wavelength = MinWavelength(i) / shape.WaveResolutionMultiplier;

                    // Do the weight from scratch because this is the real filter.
                    var w = AnimatedWavesLod.FilterByWavelength(filter, _Wavelength) * _Weight;

                    if (w <= 0f)
                    {
                        continue;
                    }

                    if (!found)
                    {
                        _WaveBufferParameters[lodIdx].x = i;
                        found = true;
                    }

                    _WaveBufferParameters[lodIdx].y = i;
                }
            }

            // Set transitional weights.
            _WaveBufferParameters[lodCount - 2].w = 1f - water.ViewerAltitudeLevelAlpha;
            _WaveBufferParameters[lodCount - 1].w = water.ViewerAltitudeLevelAlpha;

            SetRenderParameters(water, wrapper);

            wrapper.SetFloat(ShaderIDs.s_WaveResolutionMultiplier, shape.WaveResolutionMultiplier);
            wrapper.SetFloat(ShaderIDs.s_TransitionalWavelengthThreshold, water.MaximumWavelength(water.LodLevels - 1) * 0.5f);
            wrapper.SetVectorArray(ShaderIDs.s_WaveBufferParameters, _WaveBufferParameters);

            var isTexture = Mode is LodInputMode.Paint or LodInputMode.Texture;
            var isAlphaBlend = _Blend == Blend.Alpha;

            wrapper.SetKeyword(s_KeywordTexture, isTexture && !isAlphaBlend);
            wrapper.SetKeyword(s_KeywordTextureBlend, isTexture && isAlphaBlend);

            if (isTexture)
            {
                wrapper.SetInteger(ShaderIDs.s_AlphaSource, (int)_AlphaSource);
            }

            if (Mode == LodInputMode.Global)
            {
                var threads = shape.Resolution / Lod.k_ThreadGroupSize;
                wrapper.Dispatch(threads, threads, slice);
            }
            else
            {
                base.Draw(simulation, buffer, target, pass, weight, slice);
            }
        }

        void GraphicsDraw(Lod simulation, CommandBuffer buffer, RenderTexture target, int pass, float weight, int slice)
        {
            var lod = simulation as AnimatedWavesLod;

            var wrapper = new PropertyWrapperBuffer(buffer);
            SetRenderParameters(simulation._Water, wrapper);

            var isFirst = true;

            for (var i = _FirstCascade; i <= _LastCascade; i++)
            {
                _Wavelength = MinWavelength(i) / lod.WaveResolutionMultiplier;

                // Do the weight from scratch because this is the real filter.
                weight = AnimatedWavesLod.FilterByWavelength(simulation._Water, slice, _Wavelength) * _Weight;
                if (weight <= 0f) continue;

                var average = _Wavelength * 1.5f * lod.WaveResolutionMultiplier;
                // We only have one renderer so we need to use global.
                buffer.SetGlobalFloat(ShaderIDs.s_AverageWavelength, average);
                buffer.SetGlobalInt(ShaderIDs.s_WaveBufferSliceIndex, i);

                // Only apply blend mode once per component / LOD. Multiple passes can happen to gather all
                // wavelengths and is incorrect to apply blend mode to those subsequent passes (ie component
                // would be blending against itself).
                if (!isFirst)
                {
                    s_RenderPassOverride = 1;
                }

                isFirst = false;

                base.Draw(simulation, buffer, target, pass, weight, slice);
            }

            // Wavelength must be zero or waves will be filtered beforehand and not be written to every LOD.
            _Wavelength = 0;
            s_RenderPassOverride = -1;
        }

        public override float Filter(WaterRenderer water, int slice)
        {
            return 1f;
        }

        protected const int k_CascadeCount = 16;

        // First cascade of wave buffer that has waves and will be rendered.
        protected int _FirstCascade = -1;
        // Last cascade of wave buffer that has waves and will be rendered.
        // Default to lower than first default to break loops.
        protected int _LastCascade = -2;

        // Used to populate data on first frame.
        protected bool _FirstUpdate = true;

        public override bool Enabled => _FirstCascade > -1 && Mode switch
        {
            LodInputMode.Global => enabled && s_TransferWavesComputeShader != null,
            _ => base.Enabled,
        };

        internal override LodInputMode DefaultMode => LodInputMode.Global;
        public override int Pass => AnimatedWavesLod.k_PassPreCombine;

        float _Wavelength;

        protected RenderTexture _WaveBuffers;
        internal RenderTexture WaveBuffer => _WaveBuffers;

        internal Rect _Rect;

        protected float MaximumHorizontalDisplacement { get; set; }
        protected float MaximumVerticalDisplacement { get; set; }
        protected float MaximumWavesDisplacement { get; set; }

        static int s_InstanceCount = 0;

        protected bool UpdateDataEachFrame
        {
            get
            {
                var updateDataEachFrame = !_SpectrumFixedAtRuntime;
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying) updateDataEachFrame = true;
#endif
                return updateDataEachFrame;
            }
        }

        /// <summary>
        /// Min wavelength for a cascade in the wave buffer. Does not depend on viewpoint.
        /// </summary>
        protected float MinWavelength(int cascadeIdx)
        {
            var diameter = 0.5f * (1 << cascadeIdx);
            // Matches constant WAVE_SAMPLE_FACTOR in FFTSpectrum.compute
            return diameter / 8f;
        }

        protected abstract void ReportMaxDisplacement(WaterRenderer water);
        protected abstract void DestroySharedResources();

        protected override void OnLateUpdate(WaterRenderer water)
        {
            base.OnLateUpdate(water);

#if UNITY_EDITOR
            UpdateEditorOnly();
#endif

            _FirstUpdate = false;
        }

        protected virtual void SetRenderParameters<T>(WaterRenderer water, T wrapper) where T : IPropertyWrapper
        {
            wrapper.SetTexture(ShaderIDs.s_WaveBuffer, _WaveBuffers);
            wrapper.SetFloat(ShaderIDs.s_RespectShallowWaterAttenuation, _RespectShallowWaterAttenuation);
            wrapper.SetFloat(ShaderIDs.s_MaximumAttenuationDepth, water._AnimatedWavesLod.MaximumAttenuationDepth);
        }

        public virtual void UpdateShape(WaterRenderer water, CommandBuffer buffer)
        {
            ReportMaxDisplacement(water);
        }

        protected void Awake()
        {
            s_InstanceCount++;
        }

        protected void OnDestroy()
        {
            // Since FFTCompute resources are shared we will clear after last ShapeFFT is destroyed.
            if (--s_InstanceCount <= 0)
            {
                DestroySharedResources();

                if (s_DefaultSpectrum != null)
                {
                    Helpers.Destroy(s_DefaultSpectrum);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            WaterResources.Instance.AfterEnabled -= InitializeResources;
            WaterResources.Instance.AfterEnabled += InitializeResources;
            InitializeResources();

            _FirstUpdate = true;

            // Initialise with spectrum
            if (_Spectrum != null)
            {
                _ActiveSpectrum = _Spectrum;
            }

            if (_ActiveSpectrum == null)
            {
                _ActiveSpectrum = DefaultSpectrum;
            }

            WaterChunkRenderer.DisplacementReporters.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            WaterResources.Instance.AfterEnabled -= InitializeResources;

            WaterChunkRenderer.DisplacementReporters.Remove(this);
        }

        void InitializeResources()
        {
            s_TransferWavesComputeShader = WaterResources.Instance.Compute._ShapeWavesTransfer;
            s_KeywordTexture = WaterResources.Instance.Keywords.AnimatedWavesTransferWavesTexture;
            s_KeywordTextureBlend = WaterResources.Instance.Keywords.AnimatedWavesTransferWavesTextureBlend;
        }

        public bool ReportDisplacement(ref Rect bounds, ref float horizontal, ref float vertical)
        {
            if (Mode == LodInputMode.Global || !Enabled)
            {
                return false;
            }

            _Rect = Data.Rect;

            if (bounds.Overlaps(_Rect, false))
            {
                horizontal = MaximumHorizontalDisplacement;
                vertical = MaximumVerticalDisplacement;
                return true;
            }

            return false;
        }
    }

#if UNITY_EDITOR
    // Editor
    partial class ShapeWaves
    {
        void UpdateEditorOnly()
        {
            _ActiveSpectrum = _Spectrum != null ? _Spectrum : DefaultSpectrum;
        }
    }
#endif
}
