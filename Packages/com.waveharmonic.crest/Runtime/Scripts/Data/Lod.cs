// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using WaveHarmonic.Crest.Internal;
using WaveHarmonic.Crest.Utility;

#if !UNITY_2023_2_OR_NEWER
using GraphicsFormatUsage = UnityEngine.Experimental.Rendering.FormatUsage;
#endif

namespace WaveHarmonic.Crest
{
    using Inputs = SortedList<int, ILodInput>;

    enum LodTextureFormatMode
    {
        Manual,
        Performance = 100,
        Precision = 200,
        Automatic = 300,
    }

    /// <summary>
    /// Base class for data/behaviours created on each LOD.
    /// </summary>
    [System.Serializable]
    abstract class Lod
    {
        [Tooltip("Whether the simulation is enabled.")]
        [@Predicated(typeof(AnimatedWavesLod), inverted: true, hide: true)]
        [@DecoratedField, SerializeField]
        internal bool _Enabled;

        [Tooltip("If not enabled then the simulation will use the resolution defined on the Water Renderer.")]
        [@Predicated(typeof(AnimatedWavesLod), inverted: true, hide: true)]
        [@DecoratedField, SerializeField]
        internal bool _OverrideResolution = true;

        /// <summary>
        /// Resolution control. Set higher for sharper results at the cost of higher memory usage.
        /// </summary>
        [Tooltip("Resolution control. Set higher for sharper results at the cost of higher memory usage.")]
        [@Predicated(typeof(AnimatedWavesLod), inverted: true, hide: true)]
        [@Delayed]
        [SerializeField]
        internal int _Resolution = 256;

        [@Filtered]
        [SerializeField]
        protected LodTextureFormatMode _TextureFormatMode = LodTextureFormatMode.Performance;

        [Tooltip("The render texture format used for this simulation.")]
        [@ShowComputedProperty(nameof(RequestedTextureFormat))]
        [@Predicated(nameof(_TextureFormatMode), inverted: true, nameof(LodTextureFormatMode.Manual), hide: true)]
        [@DecoratedField, SerializeField]
        internal GraphicsFormat _TextureFormat;

        internal LodTextureFormatMode TextureFormatMode => _TextureFormatMode;
        // protected abstract GraphicsFormat TextureFormat { get; }

        // NOTE: This MUST match the value in Constants.hlsl, as it
        // determines the size of the texture arrays in the shaders.
        internal const int k_MaximumSlices = 15;

        // NOTE: these MUST match the values in Constants.hlsl
        // 64 recommended as a good common minimum: https://www.reddit.com/r/GraphicsProgramming/comments/aeyfkh/for_compute_shaders_is_there_an_ideal_numthreads/
        public const int k_ThreadGroupSize = 8;
        internal const int k_ThreadGroupSizeX = k_ThreadGroupSize;
        internal const int k_ThreadGroupSizeY = k_ThreadGroupSize;

        internal static class ShaderIDs
        {
            public static readonly int s_LodIndex = Shader.PropertyToID("_Crest_LodIndex");
            public static readonly int s_LodChange = Shader.PropertyToID("_Crest_LodChange");
        }

        // Used for creating shader property names etc.
        internal abstract string ID { get; }
        internal virtual string Name => ID;

        // This is the texture format we want to use.
        protected abstract GraphicsFormat RequestedTextureFormat { get; }

        // This is the platform compatible texture format we will use.
        protected GraphicsFormat CompatibleTextureFormat { get; private set; }

        protected abstract Color ClearColor { get; }

        public int Resolution => _OverrideResolution ? _Resolution : Water.LodResolution;

        protected abstract bool NeedToReadWriteTextureData { get; }

        private protected abstract Inputs Inputs { get; }
        internal abstract Color GizmoColor { get; }

        internal virtual int BufferCount => 1;

        protected virtual Texture2DArray NullTexture => BlackTextureArray;

        // This is used as alternative to Texture2D.blackTexture, as using that
        // is not possible in some shaders.
        static Texture2DArray s_BlackTextureArray = null;
        static Texture2DArray BlackTextureArray
        {
            get
            {
                if (s_BlackTextureArray == null)
                {
                    s_BlackTextureArray = TextureArrayHelpers.CreateTexture2DArray(Texture2D.blackTexture, k_MaximumSlices);
                    s_BlackTextureArray.name = "_Crest_LodBlackTexture";
                }

                return s_BlackTextureArray;
            }
        }

        static readonly GraphicsFormatUsage s_GraphicsFormatUsage =
            // Ensures a non compressed format is returned.
            GraphicsFormatUsage.LoadStore |
            // All these textures are sampled at some point.
            GraphicsFormatUsage.Sample |
            // Always use linear filtering.
            GraphicsFormatUsage.Linear;

        internal virtual bool RunsInHeadless => false;

        private protected BufferedData<RenderTexture> _Targets;
        internal RenderTexture DataTexture => _Targets.Current;
        internal RenderTexture GetDataTexture(int frameDelta) => _Targets.Previous(frameDelta);

        protected Matrix4x4[] _ViewMatrices = new Matrix4x4[k_MaximumSlices];
        private protected Cascade[] _Cascades = new Cascade[k_MaximumSlices];
        internal Cascade[] Cascades => _Cascades;
        private protected BufferedData<Vector4[]> _SamplingParameters;

        internal int Slices => _Water.LodLevels;

        // Currently use as a failure flag.
        protected bool _Valid;
        public bool Enabled => _Enabled && _Valid;

        internal WaterRenderer _Water;
        public WaterRenderer Water => _Water;

        protected int _TargetsToClear;

        protected readonly int _TextureShaderID;
        protected readonly int _TextureSourceShaderID;
        protected readonly int _SamplingParametersShaderID;
        protected readonly int _SamplingParametersCascadeShaderID;
        protected readonly int _SamplingParametersCascadeSourceShaderID;

        internal Lod()
        {
            // @Garbage
            var name = $"g_Crest_Cascade{ID}";
            _TextureShaderID = Shader.PropertyToID(name);
            _TextureSourceShaderID = Shader.PropertyToID($"{name}Source");
            _SamplingParametersShaderID = Shader.PropertyToID($"g_Crest_SamplingParameters{ID}");
            _SamplingParametersCascadeShaderID = Shader.PropertyToID($"g_Crest_SamplingParametersCascade{ID}");
            _SamplingParametersCascadeSourceShaderID = Shader.PropertyToID($"g_Crest_SamplingParametersCascade{ID}Source");
        }

        public virtual void AddToHash(ref int hash)
        {
            Hash.AddBool(_OverrideResolution, ref hash);
            Hash.AddInt(_Resolution, ref hash);
            Hash.AddInt((int)_TextureFormat, ref hash);
            Hash.AddInt((int)_TextureFormatMode, ref hash);
        }

        protected RenderTexture CreateLodDataTextures(RenderTextureDescriptor descriptor, string name, bool needToReadWriteTextureData)
        {
            RenderTexture result = new(descriptor)
            {
                wrapMode = TextureWrapMode.Clamp,
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 0,
                useMipMap = false,
                name = $"_Crest_{name}",
                dimension = TextureDimension.Tex2DArray,
                volumeDepth = Slices,
                enableRandomWrite = needToReadWriteTextureData
            };
            result.Create();
            return result;
        }

        protected void FlipBuffers()
        {
            _Targets.Flip();
            _SamplingParameters.Flip();

            UpdateSamplingParameters();
        }

        protected void Clear(RenderTexture target)
        {
            Helpers.ClearRenderTexture(target, ClearColor, depth: false);
        }

        /// <summary>
        /// Clears persistent LOD data. Some simulations have persistent data which can linger for a little while after
        /// being disabled. This will manually clear that data.
        /// </summary>
        internal virtual void ClearLodData()
        {
            // Empty.
        }

        // Only works with input-only data (ie no simulation steps).
        internal virtual void BuildCommandBuffer(WaterRenderer water, CommandBuffer buffer)
        {
            FlipBuffers();

            buffer.BeginSample(Name);

            if (_TargetsToClear > 0)
            {
                buffer.SetRenderTarget(DataTexture, 0, CubemapFace.Unknown, -1);
                buffer.ClearRenderTarget(RTClearFlags.Color, ClearColor, 0, 0);

                _TargetsToClear--;
            }

            if (Inputs.Count > 0)
            {
                SubmitDraws(buffer, Inputs, DataTexture);

                // Ensure all targets clear when there are no inputs.
                _TargetsToClear = _Targets.Size;
            }

            buffer.EndSample(Name);
        }

        private protected bool SubmitDraws(CommandBuffer buffer, Inputs draws, RenderTexture target, int pass = -1, bool filter = false)
        {
            var drawn = false;

            foreach (var draw in draws)
            {
                var input = draw.Value;
                if (!input.Enabled)
                {
                    continue;
                }

                if (pass != -1)
                {
                    var p = input.Pass;
                    if (p != -1 && p != pass) continue;
                }

                var rect = input.Rect;

                if (input.IsCompute)
                {
                    var smallest = 0;
                    if (rect != Rect.zero)
                    {
                        smallest = -1;
                        for (var slice = Slices - 1; slice >= 0; slice--)
                        {
                            if (rect != Rect.zero && !rect.Overlaps(Cascades[slice].TexelRect)) break;
                            smallest = slice;
                        }

                        if (smallest < 0) continue;
                    }

                    // Pass the slice count to only draw to valid slices.
                    input.Draw(this, buffer, target, pass, slice: Slices - smallest);
                    drawn = true;
                    continue;
                }

                for (var slice = Slices - 1; slice >= 0; slice--)
                {
                    if (rect != Rect.zero && !rect.Overlaps(Cascades[slice].TexelRect)) break;

                    var weight = filter ? input.Filter(_Water, slice) : 1f;
                    if (weight <= 0f) continue;

                    buffer.SetRenderTarget(target, 0, CubemapFace.Unknown, slice);
                    buffer.SetGlobalInt(ShaderIDs.s_LodIndex, slice);

                    // This will work for CG but not for HDRP hlsl files.
                    buffer.SetViewProjectionMatrices(_ViewMatrices[slice], _Water.GetProjectionMatrix(slice));

                    input.Draw(this, buffer, target, pass, weight, slice);
                    drawn = true;
                }
            }

            return drawn;
        }

        /// <summary>
        /// Set a new origin. This is equivalent to subtracting the new origin position from any world position state.
        /// </summary>
        internal void SetOrigin(Vector3 newOrigin)
        {
            _SamplingParameters.RunLambda(data =>
            {
                for (var index = 0; index < _Water.LodLevels; index++)
                {
                    data[index].x -= newOrigin.x;
                    data[index].y -= newOrigin.z;
                }
            });
        }

        void UpdateSamplingParameters()
        {
            for (var slice = 0; slice < Slices; slice++)
            {
                // Find snap period.
                var texel = 2f * 2f * _Water.CalcLodScale(slice) / Resolution;
                // Snap so that shape texels are stationary.
                var snapped = _Water.Root.position - new Vector3(Mathf.Repeat(_Water.Root.position.x, texel), 0, Mathf.Repeat(_Water.Root.position.z, texel));

                var cascade = new Cascade(snapped.XZ(), texel, Resolution);
                _Cascades[slice] = cascade;
                _SamplingParameters.Current[slice] = cascade.Packed;

                _ViewMatrices[slice] = WaterRenderer.CalculateViewMatrixFromSnappedPositionRHS(snapped);
            }

            Shader.SetGlobalVector(_SamplingParametersShaderID, new(_Water.LodLevels, Resolution, 1f / Resolution, 0));
            Shader.SetGlobalVectorArray(_SamplingParametersCascadeShaderID, _SamplingParameters.Current);

            if (BufferCount > 1)
            {
                Shader.SetGlobalVectorArray(_SamplingParametersCascadeSourceShaderID, _SamplingParameters.Previous(1));
            }
        }

        /// <summary>
        /// Returns index of lod that completely covers the sample area. If no such lod
        /// available, returns -1.
        /// </summary>
        internal int SuggestIndex(Rect sampleArea)
        {
            for (var slice = 0; slice < Slices; slice++)
            {
                var cascade = _Cascades[slice];

                // Shape texture needs to completely contain sample area.
                var rect = cascade.TexelRect;

                // Shrink rect by 1 texel border - this is to make finite differences fit as well.
                var texel = cascade._Texel;
                rect.x += texel; rect.y += texel;
                rect.width -= 2f * texel; rect.height -= 2f * texel;

                if (!rect.Contains(sampleArea.min) || !rect.Contains(sampleArea.max))
                {
                    continue;
                }

                return slice;
            }

            return -1;
        }

        /// <summary>
        /// Returns index of lod that completely covers the sample area, and contains
        /// wavelengths that repeat no more than twice across the smaller spatial length. If
        /// no such lod available, returns -1. This means high frequency wavelengths are
        /// filtered out, and the lod index can be used for each sample in the sample area.
        /// </summary>
        public int SuggestIndexForWaves(Rect sampleArea)
        {
            return SuggestIndexForWaves(sampleArea, Mathf.Min(sampleArea.width, sampleArea.height));
        }

        internal int SuggestIndexForWaves(Rect sampleArea, float minimumSpatialLength)
        {
            var count = Slices;

            for (var index = 0; index < count; index++)
            {
                var cascade = _Cascades[index];

                // Shape texture needs to completely contain sample area.
                var rect = cascade.TexelRect;

                // Shrink rect by 1 texel border - this is to make finite differences fit as well.
                var texel = cascade._Texel;
                rect.x += texel; rect.y += texel;
                rect.width -= 2f * texel; rect.height -= 2f * texel;

                if (!rect.Contains(sampleArea.min) || !rect.Contains(sampleArea.max))
                {
                    continue;
                }

                // The smallest wavelengths should repeat no more than twice across the smaller
                // spatial length. Unless we're in the last LOD - then this is the best we can do.
                var minimumWavelength = _Water.MaximumWavelength(index) / 2f;
                if (minimumWavelength < minimumSpatialLength / 2f && index < count - 1)
                {
                    continue;
                }

                return index;
            }

            return -1;
        }

        /// <summary>
        /// Bind data needed to load or compute from this simulation.
        /// </summary>
        internal virtual void Bind<T>(T target) where T : IPropertyWrapper
        {

        }

        public virtual void Enable()
        {
            // All simulations require a GPU so do not proceed any further.
            if (WaterRenderer.RunningWithoutGraphics)
            {
                _Valid = false;
                return;
            }

            if (!_Enabled)
            {
                Shader.SetGlobalTexture(_TextureShaderID, NullTexture);
                return;
            }

            // Some simulations are pointless in non-interactive mode.
            if (WaterRenderer.RunningHeadless && !RunsInHeadless)
            {
                _Valid = false;
                return;
            }

            // Validate textures.
            {
                // Find a compatible texture format.
                CompatibleTextureFormat = Helpers.GetCompatibleTextureFormat(RequestedTextureFormat, s_GraphicsFormatUsage, NeedToReadWriteTextureData);

                if (CompatibleTextureFormat == GraphicsFormat.None)
                {
                    Debug.Log($"Crest: Disabling {Name} simulation due to no valid available texture format.");
                    _Valid = false;
                    return;
                }

                Debug.Assert(Slices <= k_MaximumSlices);
            }

            _Valid = true;

            Allocate();
        }

        public virtual void Disable()
        {
            // Unbind from all graphics shaders (not compute)
            Shader.SetGlobalTexture(_TextureShaderID, NullTexture);

            // Release resources and destroy object to avoid reference leak.
            _Targets.RunLambda(x =>
            {
                x.Release();
                Helpers.Destroy(x);
            });
        }

        public virtual void Destroy()
        {

        }

        public virtual void EnableExternalItems()
        {

        }

        public virtual void DisableExternalItems()
        {

        }

        protected virtual void Allocate()
        {
            var descriptor = new RenderTextureDescriptor(Resolution, Resolution, CompatibleTextureFormat, 0);
            _Targets = new(BufferCount, () => CreateLodDataTextures(descriptor, $"{ID}Lod", NeedToReadWriteTextureData));
            _Targets.RunLambda(Clear);

            _SamplingParameters = new(BufferCount, () => new Vector4[k_MaximumSlices]);

            // Bind globally once here on init, which will bind to all graphics shaders (not compute)
            Shader.SetGlobalTexture(_TextureShaderID, DataTexture);
        }

#if UNITY_EDITOR
        [@OnChange]
        void OnChange(string propertyPath, object previousValue)
        {
            switch (propertyPath)
            {
                case nameof(_Enabled):
                    if (_Enabled) Enable(); else Disable();
                    break;
            }
        }
#endif
    }
}
