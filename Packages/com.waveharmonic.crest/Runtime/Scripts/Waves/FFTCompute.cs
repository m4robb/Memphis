// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

// Inspired by https://github.com/speps/GX-EncinoWaves

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Runs FFT to generate water surface displacements
    /// </summary>
    sealed class FFTCompute
    {
        // Must match 'SIZE' param of first kernel in FFTCompute.compute
        const int k_Kernel0Resolution = 8;

        // Must match CASCADE_COUNT in FFTCompute.compute
        const int k_CascadeCount = 16;

        bool _Initialized = false;

        RenderTexture _SpectrumInitial;
        Texture2D _TextureButterfly;

        /// <summary>
        /// Generated 'raw', uncombined, wave data. Input for putting into AnimWaves data before combine pass.
        /// </summary>
        public RenderTexture WaveBuffers { get; private set; }

        Texture2D _TextureSpectrumControls;
        bool _SpectrumInitialized = false;
        readonly Color[] _SpectrumDataScratch = new Color[WaveSpectrum.k_NumberOfOctaves];

        ComputeShader _ShaderSpectrum;
        ComputeShader _ShaderFFT;

        int _KernelSpectrumInitial;
        int _KernelSpectrumUpdate;

        // Generation data
        readonly int _Resolution;
        readonly float _LoopPeriod;

        Parameters _Parameters;

        float _GenerationTime = -1f;

        static readonly bool s_SupportsRandomWriteRGFloat =
            SystemInfo.SupportsRandomWriteOnRenderTextureFormat(RenderTextureFormat.RGFloat);

        public static class ShaderIDs
        {
            public static readonly int s_Size = Shader.PropertyToID("_Crest_Size");
            public static readonly int s_WindSpeed = Shader.PropertyToID("_Crest_WindSpeed");
            public static readonly int s_Turbulence = Shader.PropertyToID("_Crest_Turbulence");
            public static readonly int s_Alignment = Shader.PropertyToID("_Crest_Alignment");
            public static readonly int s_Gravity = Shader.PropertyToID("_Crest_Gravity");
            public static readonly int s_Period = Shader.PropertyToID("_Crest_Period");
            public static readonly int s_WindDir = Shader.PropertyToID("_Crest_WindDir");
            public static readonly int s_SpectrumControls = Shader.PropertyToID("_Crest_SpectrumControls");
            public static readonly int s_ResultInit = Shader.PropertyToID("_Crest_ResultInit");
            public static readonly int s_Time = Shader.PropertyToID("_Crest_Time");
            public static readonly int s_Chop = Shader.PropertyToID("_Crest_Chop");
            public static readonly int s_Init0 = Shader.PropertyToID("_Crest_Init0");
            public static readonly int s_ResultHeight = Shader.PropertyToID("_Crest_ResultHeight");
            public static readonly int s_ResultDisplaceX = Shader.PropertyToID("_Crest_ResultDisplaceX");
            public static readonly int s_ResultDisplaceZ = Shader.PropertyToID("_Crest_ResultDisplaceZ");
            public static readonly int s_InputH = Shader.PropertyToID("_Crest_InputH");
            public static readonly int s_InputX = Shader.PropertyToID("_Crest_InputX");
            public static readonly int s_InputZ = Shader.PropertyToID("_Crest_InputZ");
            public static readonly int s_InputButterfly = Shader.PropertyToID("_Crest_InputButterfly");
            public static readonly int s_Output1 = Shader.PropertyToID("_Crest_Output1");
            public static readonly int s_Output2 = Shader.PropertyToID("_Crest_Output2");
            public static readonly int s_Output3 = Shader.PropertyToID("_Crest_Output3");
            public static readonly int s_Output = Shader.PropertyToID("_Crest_Output");

            public static readonly int s_TemporaryFFT1 = Shader.PropertyToID("_Crest_TemporaryFFT1");
            public static readonly int s_TemporaryFFT2 = Shader.PropertyToID("_Crest_TemporaryFFT2");
            public static readonly int s_TemporaryFFT3 = Shader.PropertyToID("_Crest_TemporaryFFT3");
        }

        internal readonly struct Parameters
        {
            public readonly WaveSpectrum _Spectrum;
            public readonly float _WindSpeed;
            public readonly float _WindDirectionRadians;
            public readonly float _WindTurbulence;
            public readonly float _WindAlignment;

            public Parameters(WaveSpectrum spectrum, float speed, float direction, float turbulence, float alignment)
            {
                _Spectrum = spectrum;
                _WindSpeed = speed;
                _WindDirectionRadians = direction;
                _WindTurbulence = turbulence;
                _WindAlignment = alignment;
            }

            // Implement custom or incur allocations.
            public override int GetHashCode()
            {
                return System.HashCode.Combine(_Spectrum, _WindSpeed, _WindDirectionRadians, _WindTurbulence, _WindAlignment);
            }
        }

        public FFTCompute(int resolution, float loopPeriod, Parameters parameters)
        {
            Debug.Assert(Mathf.NextPowerOfTwo(resolution) == resolution, "Crest: FFTCompute resolution must be power of 2");

            _Resolution = resolution;
            _LoopPeriod = loopPeriod;
            _Parameters = parameters;
        }

        public void Release()
        {
            if (_TextureButterfly != null) Helpers.Destroy(_TextureButterfly);
            if (_TextureSpectrumControls != null) Helpers.Destroy(_TextureSpectrumControls);
            if (_SpectrumInitial != null) _SpectrumInitial.Release();

            if (WaveBuffers != null)
            {
                Helpers.Destroy(WaveBuffers);
                WaveBuffers = null;
            }

            _Initialized = false;
        }

        void InitializeTextures()
        {
            Release();

            _ShaderSpectrum = WaterResources.Instance.Compute._FFTSpectrum;
            _KernelSpectrumInitial = _ShaderSpectrum.FindKernel("SpectrumInitalize");
            _KernelSpectrumUpdate = _ShaderSpectrum.FindKernel("SpectrumUpdate");
            _ShaderFFT = WaterResources.Instance.Compute._FFT;

            _TextureButterfly = new(_Resolution, Mathf.RoundToInt(Mathf.Log(_Resolution, 2)), TextureFormat.RGBAFloat, false, true);

            _TextureSpectrumControls = new(WaveSpectrum.k_NumberOfOctaves, 1, TextureFormat.RFloat, false, true);

            var rtd = new RenderTextureDescriptor();
            rtd.width = rtd.height = _Resolution;
            rtd.dimension = TextureDimension.Tex2DArray;
            rtd.enableRandomWrite = true;
            rtd.depthBufferBits = 0;
            rtd.volumeDepth = k_CascadeCount;
            rtd.colorFormat = RenderTextureFormat.ARGBFloat;
            rtd.msaaSamples = 1;

            Helpers.SafeCreateRenderTexture(ref _SpectrumInitial, rtd);
            _SpectrumInitial.name = "_Crest_FFTSpectrumInit";
            _SpectrumInitial.Create();

            // Raw wave data buffer
            WaveBuffers = new(_Resolution, _Resolution, 0, GraphicsFormat.R16G16B16A16_SFloat)
            {
                wrapMode = TextureWrapMode.Repeat,
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 0,
                useMipMap = false,
                name = "_Crest_FFTCascades",
                dimension = TextureDimension.Tex2DArray,
                volumeDepth = k_CascadeCount,
                enableRandomWrite = true,
            };
            WaveBuffers.Create();

            InitializeButterfly(_Resolution);

            InitialiseSpectrumHandControls();

            _Initialized = true;
        }

        void CleanUp()
        {
            // Destroy to clear references.
            Helpers.Destroy(_SpectrumInitial);
            _SpectrumInitialized = false;
        }

        internal static void CleanUpAll()
        {
            foreach (var generator in s_Generators)
            {
                generator.Value.Release();
                generator.Value.CleanUp();
            }
        }

        static readonly Dictionary<int, FFTCompute> s_Generators = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStatics()
        {
            s_Generators.Clear();
        }

        static int CalculateWaveConditionsHash(int resolution, float loopPeriod, Parameters parameters)
        {
            var conditionsHash = Utility.Hash.CreateHash();
            Utility.Hash.AddInt(resolution, ref conditionsHash);
            Utility.Hash.AddFloat(loopPeriod, ref conditionsHash);
            Utility.Hash.AddInt(parameters.GetHashCode(), ref conditionsHash);
            return conditionsHash;
        }

        /// <summary>
        /// Computes water surface displacement, with wave components split across slices of the output texture array
        /// </summary>
        public static RenderTexture GenerateDisplacements(CommandBuffer buf, int resolution, float loopPeriod, float time, Parameters parameters, bool updateSpectrum)
        {
            // All static data arguments should be hashed here and passed to the generator constructor
            var conditionsHash = CalculateWaveConditionsHash(resolution, loopPeriod, parameters);
            if (!s_Generators.TryGetValue(conditionsHash, out var generator))
            {
                // No generator for these params - create one
                generator = new(resolution, loopPeriod, parameters);
                s_Generators.Add(conditionsHash, generator);
            }

            // The remaining dynamic data arguments should be passed in to the generation here
            return generator.GenerateDisplacementsInternal(buf, time, updateSpectrum);
        }

        RenderTexture GenerateDisplacementsInternal(CommandBuffer buffer, float time, bool updateSpectrum)
        {
            // Check if already generated, and we're not being asked to re-update the spectrum
            if (_GenerationTime == time && !updateSpectrum)
            {
                return WaveBuffers;
            }

            if (!_Initialized || _SpectrumInitial == null)
            {
                InitializeTextures();
            }

            if (!_SpectrumInitialized || updateSpectrum)
            {
                InitialiseSpectrumHandControls();
                InitializeSpectrum(buffer);
                _SpectrumInitialized = true;
            }

            // Update Spectrum.
            // Computes a spectrum for the current time which can be FFT'd into the final surface.
            {
                var wrapper = new PropertyWrapperCompute(buffer, _ShaderSpectrum, _KernelSpectrumUpdate);

                var descriptor = _SpectrumInitial.descriptor;

                if (s_SupportsRandomWriteRGFloat)
                {
                    descriptor.colorFormat = RenderTextureFormat.RGFloat;
                }

                buffer.GetTemporaryRT(ShaderIDs.s_TemporaryFFT1, descriptor);
                buffer.GetTemporaryRT(ShaderIDs.s_TemporaryFFT2, descriptor);
                buffer.GetTemporaryRT(ShaderIDs.s_TemporaryFFT3, descriptor);

                wrapper.SetInteger(ShaderIDs.s_Size, _Resolution);
                wrapper.SetFloat(ShaderIDs.s_Time, time * _Parameters._Spectrum._GravityScale);
                wrapper.SetFloat(ShaderIDs.s_Chop, _Parameters._Spectrum._Chop);
                wrapper.SetFloat(ShaderIDs.s_Period, _LoopPeriod < Mathf.Infinity ? _LoopPeriod : -1);
                wrapper.SetTexture(ShaderIDs.s_Init0, _SpectrumInitial);
                wrapper.SetTexture(ShaderIDs.s_ResultHeight, ShaderIDs.s_TemporaryFFT1);
                wrapper.SetTexture(ShaderIDs.s_ResultDisplaceX, ShaderIDs.s_TemporaryFFT2);
                wrapper.SetTexture(ShaderIDs.s_ResultDisplaceZ, ShaderIDs.s_TemporaryFFT3);
                wrapper.Dispatch(_Resolution / 8, _Resolution / 8, k_CascadeCount);
            }

            // Dispatch FFT.
            // FFT the spectrum into surface displacements.
            {
                var kernel = 2 * Mathf.RoundToInt(Mathf.Log(_Resolution / k_Kernel0Resolution, 2f));
                var wrapper = new PropertyWrapperCompute(buffer, _ShaderFFT, kernel);

                wrapper.SetTexture(ShaderIDs.s_InputButterfly, _TextureButterfly);
                wrapper.SetTexture(ShaderIDs.s_Output1, ShaderIDs.s_TemporaryFFT1);
                wrapper.SetTexture(ShaderIDs.s_Output2, ShaderIDs.s_TemporaryFFT2);
                wrapper.SetTexture(ShaderIDs.s_Output3, ShaderIDs.s_TemporaryFFT3);
                wrapper.Dispatch(1, _Resolution, k_CascadeCount);

                wrapper = new PropertyWrapperCompute(buffer, _ShaderFFT, kernel + 1);
                wrapper.SetTexture(ShaderIDs.s_InputH, ShaderIDs.s_TemporaryFFT1);
                wrapper.SetTexture(ShaderIDs.s_InputX, ShaderIDs.s_TemporaryFFT2);
                wrapper.SetTexture(ShaderIDs.s_InputZ, ShaderIDs.s_TemporaryFFT3);
                wrapper.SetTexture(ShaderIDs.s_InputButterfly, _TextureButterfly);
                wrapper.SetTexture(ShaderIDs.s_Output, WaveBuffers);
                wrapper.Dispatch(_Resolution, 1, k_CascadeCount);

                buffer.ReleaseTemporaryRT(ShaderIDs.s_TemporaryFFT1);
                buffer.ReleaseTemporaryRT(ShaderIDs.s_TemporaryFFT2);
                buffer.ReleaseTemporaryRT(ShaderIDs.s_TemporaryFFT3);
            }

            _GenerationTime = time;

            return WaveBuffers;
        }

        /// <summary>
        /// Changing wave gen data can result in creating lots of new generators. This gives a way to notify
        /// that a parameter has changed. If there is no existing generator for the new param values, but there
        /// is one for the old param values, this old generator is repurposed.
        /// </summary>
        public static void OnGenerationDataUpdated(int resolution, float loopPeriod, Parameters oldParameters, Parameters newParameters)
        {
            // If multiple wave components share one FFT, then one of them changes its settings, it will
            // actually steal the generator from the rest. Then the first from the rest which request the
            // old settings will trigger creation of a new generator, and the remaining ones will use this
            // new generator. In the end one new generator is created, but it's created for the old settings.
            // Generators are requested single threaded so there should not be a race condition. Odd pattern
            // but I don't think any other way works without ugly checks to see if old generators are still
            // used, or other complicated things.

            // Check if no generator exists for new values
            var newHash = CalculateWaveConditionsHash(resolution, loopPeriod, newParameters);
            if (!s_Generators.TryGetValue(newHash, out _))
            {
                // Try to adapt an existing generator rather than default to creating a new one
                var oldHash = CalculateWaveConditionsHash(resolution, loopPeriod, oldParameters);
                if (s_Generators.TryGetValue(oldHash, out var generator))
                {
                    // Hash will change for this generator, so remove the current one
                    s_Generators.Remove(oldHash);

                    // Update params
                    generator._Parameters = newParameters;

                    // Trigger generator to re-init the spectrum
                    generator._SpectrumInitialized = false;

                    // Re-add with new hash
                    s_Generators.Add(newHash, generator);
                }
            }
            else
            {
                // There is already a new generator which will be used. Remove the previous one - if it really is needed
                // then it will be created later.
                var oldHash = CalculateWaveConditionsHash(resolution, loopPeriod, oldParameters);
                s_Generators.Remove(oldHash);
            }
        }

        /// <summary>
        /// Number of FFT generators
        /// </summary>
        public static int GeneratorCount => s_Generators != null ? s_Generators.Count : 0;

        /// <summary>
        /// Computes the offsets used for the FFT calculation
        /// </summary>
        void InitializeButterfly(int resolution)
        {
            var log2Size = Mathf.RoundToInt(Mathf.Log(resolution, 2));
            var butterflyColors = new Color[resolution * log2Size];

            int offset = 1, numIterations = resolution >> 1;
            for (var rowIndex = 0; rowIndex < log2Size; rowIndex++)
            {
                var rowOffset = rowIndex * resolution;
                {
                    int start = 0, end = 2 * offset;
                    for (var iteration = 0; iteration < numIterations; iteration++)
                    {
                        var bigK = 0f;
                        for (var k = start; k < end; k += 2)
                        {
                            var phase = 2.0f * Mathf.PI * bigK * numIterations / resolution;
                            var cos = Mathf.Cos(phase);
                            var sin = Mathf.Sin(phase);
                            butterflyColors[rowOffset + k / 2] = new(cos, -sin, 0, 1);
                            butterflyColors[rowOffset + k / 2 + offset] = new(-cos, sin, 0, 1);

                            bigK += 1f;
                        }
                        start += 4 * offset;
                        end = start + 2 * offset;
                    }
                }
                numIterations >>= 1;
                offset <<= 1;
            }

            _TextureButterfly.SetPixels(butterflyColors);
            _TextureButterfly.Apply();
        }

        void InitialiseSpectrumHandControls()
        {
            for (var i = 0; i < WaveSpectrum.k_NumberOfOctaves; i++)
            {
                var pow = _Parameters._Spectrum._PowerDisabled[i] ? 0f : Mathf.Pow(10f, _Parameters._Spectrum._PowerLogarithmicScales[i]);
                pow *= _Parameters._Spectrum._Multiplier * _Parameters._Spectrum._Multiplier;
                _SpectrumDataScratch[i] = pow * Color.white;
            }

            _TextureSpectrumControls.SetPixels(_SpectrumDataScratch);
            _TextureSpectrumControls.Apply();
        }

        /// <summary>
        /// Computes base spectrum values based on wind speed and turbulence and spectrum controls
        /// </summary>
        void InitializeSpectrum(CommandBuffer buf)
        {
            buf.SetComputeIntParam(_ShaderSpectrum, ShaderIDs.s_Size, _Resolution);
            buf.SetComputeFloatParam(_ShaderSpectrum, ShaderIDs.s_WindSpeed, _Parameters._WindSpeed);
            buf.SetComputeFloatParam(_ShaderSpectrum, ShaderIDs.s_Turbulence, _Parameters._WindTurbulence);
            buf.SetComputeFloatParam(_ShaderSpectrum, ShaderIDs.s_Alignment, _Parameters._WindAlignment);
            buf.SetComputeFloatParam(_ShaderSpectrum, ShaderIDs.s_Gravity, Mathf.Abs(Physics.gravity.magnitude));
            buf.SetComputeFloatParam(_ShaderSpectrum, ShaderIDs.s_Period, _LoopPeriod < Mathf.Infinity ? _LoopPeriod : -1);
            buf.SetComputeVectorParam(_ShaderSpectrum, ShaderIDs.s_WindDir, new(Mathf.Cos(_Parameters._WindDirectionRadians), Mathf.Sin(_Parameters._WindDirectionRadians)));
            buf.SetComputeTextureParam(_ShaderSpectrum, _KernelSpectrumInitial, ShaderIDs.s_SpectrumControls, _TextureSpectrumControls);
            buf.SetComputeTextureParam(_ShaderSpectrum, _KernelSpectrumInitial, ShaderIDs.s_ResultInit, _SpectrumInitial);
            buf.DispatchCompute(_ShaderSpectrum, _KernelSpectrumInitial, _Resolution / 8, _Resolution / 8, k_CascadeCount);
        }

        public static FFTCompute GetInstance(int resolution, float loopPeriod, Parameters parameters)
        {
            return s_Generators.GetValueOrDefault(CalculateWaveConditionsHash(resolution, loopPeriod, parameters), null);
        }

        public bool HasData()
        {
            return WaveBuffers != null && WaveBuffers.IsCreated();
        }

        internal void OnGUI()
        {
            if (WaveBuffers != null && WaveBuffers.IsCreated())
            {
                DebugGUI.DrawTextureArray(WaveBuffers, 8, 0.5f, 20f);
            }

            if (_TextureSpectrumControls != null)
            {
                GUI.DrawTexture(new(0f, 0f, 100f, 10f), _TextureSpectrumControls);
            }
        }
    }
}
