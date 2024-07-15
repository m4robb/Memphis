// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

// BIRP fallback not really tested yet - shaders need fixing up.

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using WaveHarmonic.Crest.Editor;
using WaveHarmonic.Crest.Internal;
using WaveHarmonic.Crest.Utility;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Stores shadowing data to use during water shading. Shadowing is persistent and supports sampling across
    /// many frames and jittered sampling for (very) soft shadows. Soft shadows is red, hard shadows is green.
    /// In HDRP, hard shadows are not computed and y channel will be 0.
    /// </summary>
    [FilterEnum(nameof(_TextureFormatMode), Filtered.Mode.Exclude, (int)LodTextureFormatMode.Automatic)]
    sealed partial class ShadowLod : Lod
#if UNITY_EDITOR
        , IOptionalShadowLod
#endif
    {
        [@Space(10)]

        [Tooltip("Jitter diameter for soft shadows, controls softness of this shadowing component.")]
        [@Range(0f, 32f)]
        [SerializeField]
        internal float _JitterDiameterSoft = 15f;

        [Tooltip("Current frame weight for accumulation over frames for soft shadows. Roughly means 'responsiveness' for soft shadows.")]
        [@Range(0f, 1f)]
        [SerializeField]
        internal float _CurrentFrameWeightSoft = 0.03f;

        [Tooltip("Jitter diameter for hard shadows, controls softness of this shadowing component.")]
        [@Range(0f, 32f)]
        [SerializeField]
        internal float _JitterDiameterHard = 0.6f;

        [Tooltip("Current frame weight for accumulation over frames for hard shadows. Roughly means 'responsiveness' for hard shadows.")]
        [@Range(0f, 1f)]
        [SerializeField]
        internal float _CurrentFrameWeightHard = 0.15f;

        [@Space(10)]

        [Tooltip("Whether to disable the null light warning, use this if you assign it dynamically and expect it to be null at points")]
        [@DecoratedField, SerializeField]
        internal bool _AllowNullLight = false;

        [Tooltip("Whether to disable the no shadows warning. Use this if you toggle the shadows on the primary light dynamically.")]
        [@DecoratedField, SerializeField]
        internal bool _AllowNoShadows = false;

        static new class ShaderIDs
        {
            public static readonly int s_CenterPos = Shader.PropertyToID("_Crest_CenterPos");
            public static readonly int s_Scale = Shader.PropertyToID("_Crest_Scale");
            public static readonly int s_JitterDiameters_CurrentFrameWeights = Shader.PropertyToID("_Crest_JitterDiameters_CurrentFrameWeights");
            public static readonly int s_MainCameraProjectionMatrix = Shader.PropertyToID("_Crest_MainCameraProjectionMatrix");
            public static readonly int s_SimDeltaTime = Shader.PropertyToID("_Crest_SimDeltaTime");
        }

        internal static readonly Color s_GizmoColor = new(0f, 0f, 0f, 0.5f);
        internal static bool s_ProcessData = true;

        internal override string ID => "Shadow";
        internal override string Name => "Shadows";
        internal override Color GizmoColor => s_GizmoColor;
        protected override Color ClearColor => Color.black;
        protected override bool NeedToReadWriteTextureData => true;
        internal override int BufferCount => 2;

        protected override GraphicsFormat RequestedTextureFormat => _TextureFormatMode switch
        {
            LodTextureFormatMode.Performance => GraphicsFormat.R8G8_UNorm,
            LodTextureFormatMode.Precision => GraphicsFormat.R16G16_UNorm,
            LodTextureFormatMode.Manual => _TextureFormat,
            _ => throw new System.NotImplementedException(),
        };

        Light _Light;

        // SRP version needs access to this externally, hence internal get.
        internal CommandBuffer CopyShadowMapBuffer { get; private set; }
        PropertyWrapperMaterial[] _RenderMaterial;

        enum Error
        {
            None,
            NoLight,
            NoShadows,
            IncorrectLightType,
        }

        Error _Error;

        public override void Enable()
        {
            if (WaterResources.Instance.Shaders._UpdateShadow == null)
            {
                _Valid = false;
                return;
            }

            var isShadowsDisabled = false;

            if (RenderPipelineHelper.IsLegacy)
            {
                if (QualitySettings.shadows == UnityEngine.ShadowQuality.Disable)
                {
                    isShadowsDisabled = true;
                }
            }
            else if (RenderPipelineHelper.IsUniversal)
            {
#if d_UnityURP
                var asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

                // TODO: Support single casacades as it is possible.
                if (asset && asset.shadowCascadeCount < 2)
                {
                    Debug.LogError("Crest shadowing requires shadow cascades to be enabled on the pipeline asset.", asset);
                    _Valid = false;
                    return;
                }

                if (asset.mainLightRenderingMode == LightRenderingMode.Disabled)
                {
                    Debug.LogError("Crest: Main Light must be enabled to enable water shadowing.", _Water);
                    _Valid = false;
                    return;
                }

                isShadowsDisabled = !asset.supportsMainLightShadows;
#endif
            }

            if (isShadowsDisabled)
            {
                Debug.LogError("Crest: Shadows must be enabled in the quality settings to enable water shadowing.", _Water);
                _Valid = false;
                return;
            }

            base.Enable();
        }

        public override void Disable()
        {
            base.Disable();

            DisableExternalItems();

#if d_UnityHDRP
            SampleShadowsHDRP.Disable();
#endif

#if d_UnityURP
            SampleShadowsURP.Disable();
#endif

            for (var index = 0; index < _RenderMaterial.Length; index++)
            {
                Helpers.Destroy(_RenderMaterial[index].Material);
            }
        }

        public override void EnableExternalItems()
        {
            base.EnableExternalItems();

            if (RenderPipelineHelper.IsLegacy)
            {
                Camera.onPreCull -= OnPreCullCamera;
                Camera.onPreCull += OnPreCullCamera;
                Camera.onPostRender -= OnPostRenderCamera;
                Camera.onPostRender += OnPostRenderCamera;
            }

            CleanUpShadowCommandBuffers();

            if (RenderPipelineHelper.IsHighDefinition)
            {
#if d_UnityHDRP
                SampleShadowsHDRP.Enable();
#endif
            }
            else if (RenderPipelineHelper.IsUniversal)
            {
#if d_UnityURP
                SampleShadowsURP.Enable();
#endif
            }
        }

        public override void DisableExternalItems()
        {
            base.DisableExternalItems();

            CleanUpShadowCommandBuffers();

            Camera.onPreCull -= OnPreCullCamera;
            Camera.onPostRender -= OnPostRenderCamera;
        }

        protected override void Allocate()
        {
            base.Allocate();

            _Targets.RunLambda(buffer => Clear(buffer));

            {
                _RenderMaterial = new PropertyWrapperMaterial[Slices];
                var shader = WaterResources.Instance.Shaders._UpdateShadow;
                for (var i = 0; i < _RenderMaterial.Length; i++)
                {
                    _RenderMaterial[i] = new(shader);
                    _RenderMaterial[i].SetInteger(Lod.ShaderIDs.s_LodIndex, i);
                }
            }

            // Enable sample shadows custom pass.
            if (RenderPipelineHelper.IsHighDefinition)
            {
#if d_UnityHDRP
                SampleShadowsHDRP.Enable();
#endif
            }
            else if (RenderPipelineHelper.IsUniversal)
            {
#if d_UnityURP
                SampleShadowsURP.Enable();
#endif
            }
        }

        internal override void ClearLodData()
        {
            base.ClearLodData();
            _Targets.RunLambda(buffer => Clear(buffer));
        }

        void OnPreCullCamera(Camera camera)
        {
#if UNITY_EDITOR
            // Do not execute when editor is not active to conserve power and prevent possible leaks.
            if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                CopyShadowMapBuffer?.Clear();
                return;
            }

            if (!WaterRenderer.IsWithinEditorUpdate)
            {
                CopyShadowMapBuffer?.Clear();
                return;
            }
#endif

            var water = _Water;

            if (water == null)
            {
                return;
            }

            if (!Helpers.MaskIncludesLayer(camera.cullingMask, water.Layer))
            {
                return;
            }

            if (camera == water.Viewer && CopyShadowMapBuffer != null)
            {
                // Calling this in OnPreRender was too late to be executed in the same frame.
                AddCommandBufferToPrimaryLight();

                // Disable for XR SPI otherwise input will not have correct world position.
                if (camera.stereoEnabled && XRHelpers.IsSinglePass)
                {
                    CopyShadowMapBuffer.DisableShaderKeyword("STEREO_INSTANCING_ON");
                }

                BuildCommandBuffer(water, CopyShadowMapBuffer);

                // Restore XR SPI as we cannot rely on remaining pipeline to do it for us.
                if (camera.stereoEnabled && XRHelpers.IsSinglePass)
                {
                    CopyShadowMapBuffer.EnableShaderKeyword("STEREO_INSTANCING_ON");
                }
            }
        }

        void OnPostRenderCamera(Camera camera)
        {
#if UNITY_EDITOR
            // Do not execute when editor is not active to conserve power and prevent possible leaks.
            if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                CopyShadowMapBuffer?.Clear();
                return;
            }

            if (!WaterRenderer.IsWithinEditorUpdate)
            {
                CopyShadowMapBuffer?.Clear();
                return;
            }
#endif

            var water = _Water;

            if (water == null)
            {
                return;
            }

            if (!Helpers.MaskIncludesLayer(camera.cullingMask, water.Layer))
            {
                return;
            }

            if (camera == water.Viewer)
            {
                // CBs added to a light are executed for every camera, but the LOD data is only supports a single
                // camera. Removing the CB after the camera renders restricts the CB to one camera.
                RemoveCommandBufferFromPrimaryLight();
            }
        }

        internal void AddCommandBufferToPrimaryLight()
        {
            if (_Light == null || CopyShadowMapBuffer == null) return;
            _Light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, CopyShadowMapBuffer);
            _Light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, CopyShadowMapBuffer);
        }

        internal void RemoveCommandBufferFromPrimaryLight()
        {
            if (_Light == null || CopyShadowMapBuffer == null) return;
            _Light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, CopyShadowMapBuffer);
        }

        /// <summary>
        /// Validates the primary light.
        /// </summary>
        /// <returns>
        /// Whether the light is valid. An invalid light should be treated as a developer error and not recoverable.
        /// </returns>
        bool ValidateLight()
        {
            if (_Light == null)
            {
                if (!_AllowNullLight)
                {
                    if (_Error != Error.NoLight)
                    {
                        Debug.LogWarning($"Crest: Primary light must be specified on {nameof(WaterRenderer)} script to enable shadows.", _Water);
                        _Error = Error.NoLight;
                    }
                    return false;
                }

                return true;
            }

            if (_Light.shadows == LightShadows.None)
            {
                if (!_AllowNoShadows)
                {
                    if (_Error != Error.NoShadows)
                    {
                        Debug.LogWarning("Crest: Shadows must be enabled on primary light to enable water shadowing (types Hard and Soft are equivalent for the water system).", _Light);
                        _Error = Error.NoShadows;
                    }
                    return false;
                }
            }

            if (_Light.type != LightType.Directional)
            {
                if (_Error != Error.IncorrectLightType)
                {
                    Debug.LogError("Crest: Primary light must be of type Directional.", _Light);
                    _Error = Error.IncorrectLightType;
                }
                return false;
            }

            _Error = Error.None;
            return true;
        }

        /// <summary>
        /// Stores the primary light.
        /// </summary>
        /// <returns>
        /// Whether there is a light that casts shadows.
        /// </returns>
        bool SetUpLight()
        {
            if (_Light == null)
            {
                _Light = _Water.PrimaryLight;

                if (_Light == null)
                {
                    return false;
                }
            }

            if (_Light.shadows == LightShadows.None)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// May happen if scenes change etc
        /// </summary>
        void ClearBufferIfLightChanged()
        {
            if (_Light != _Water.PrimaryLight)
            {
                _Targets.RunLambda(buffer => Clear(buffer));
                CleanUpShadowCommandBuffers();
                _Light = null;
            }
        }

        void CleanUpShadowCommandBuffers()
        {
            if (!RenderPipelineHelper.IsLegacy)
            {
                return;
            }

            CopyShadowMapBuffer?.Release();
            CopyShadowMapBuffer = null;
        }

        void Update()
        {
            // If disabled then we hit a failure state. Try and recover in edit mode by proceeding.
            if (!_Valid && Application.isPlaying)
            {
                return;
            }

            ClearBufferIfLightChanged();

            var hasShadowCastingLight = SetUpLight();
            // If in play mode, and this becomes false, then we hit a failed state and will not recover.
            _Valid = ValidateLight();

            if (!s_ProcessData || !_Valid || !hasShadowCastingLight)
            {
                if (CopyShadowMapBuffer != null)
                {
                    // If we have a command buffer, then there is likely shadow data so we need to clear it.
                    _Targets.RunLambda(buffer => Clear(buffer));
                    CleanUpShadowCommandBuffers();
                }

                return;
            }

            CopyShadowMapBuffer ??= new() { name = "Crest Shadow Data" };

            FlipBuffers();

            CopyShadowMapBuffer.Clear();

            // clear the shadow collection. it will be overwritten with shadow values IF the shadows render,
            // which only happens if there are (nontransparent) shadow receivers around. this is only reliable
            // in play mode, so don't do it in edit mode.
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
#endif
            {
                Clear(DataTexture);
            }
        }

        internal override void BuildCommandBuffer(WaterRenderer water, CommandBuffer buffer)
        {
            // Only do a partial update when called by WaterRenderer as we want to execute
            // with the camera's command buffer (in frame).
            if (buffer == _Water.SimulationBuffer)
            {
                Update();
                return;
            }

            // NOTE: FlipBuffers called elsewhere.

            // Cache the camera for further down.
            var camera = water.Viewer;

#pragma warning disable 618
            using (new ProfilingSample(buffer, "CrestSampleShadows"))
#pragma warning restore 618
            {
                var jitter = new Vector4
                (
                    _JitterDiameterSoft,
                    _JitterDiameterHard,
                    _CurrentFrameWeightSoft,
                    _CurrentFrameWeightHard
                );

                for (var slice = Slices - 1; slice >= 0; slice--)
                {
                    _RenderMaterial[slice].SetVector(ShaderIDs.s_CenterPos, _Cascades[slice]._SnappedPosition.XNZ(_Water.SeaLevel));
                    var scale = water.CalcLodScale(slice);
                    _RenderMaterial[slice].SetVector(ShaderIDs.s_Scale, new(scale, 1f, scale));
                    _RenderMaterial[slice].SetVector(ShaderIDs.s_JitterDiameters_CurrentFrameWeights, jitter);
                    _RenderMaterial[slice].SetMatrix(ShaderIDs.s_MainCameraProjectionMatrix, GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * camera.worldToCameraMatrix);
                    _RenderMaterial[slice].SetFloat(ShaderIDs.s_SimDeltaTime, Time.deltaTime);

                    _RenderMaterial[slice].SetTexture(_TextureSourceShaderID, _Targets.Previous(1));

#if UNITY_EDITOR
                    // On recompiles this becomes unset even though we run over the code path to set it again...
                    _RenderMaterial[slice].SetInteger(Lod.ShaderIDs.s_LodIndex, slice);
#endif

                    Helpers.Blit(buffer, DataTexture, _RenderMaterial[slice].Material, depthSlice: slice);
                }

                // BUG: These draw calls will "leak" and be duplicated before the above blit. They are executed at
                // the beginning of this CB before any commands are applied.
                SubmitDraws(buffer, s_Inputs, DataTexture);

                // Set the target texture as to make sure we catch the 'pong' each frame
                Shader.SetGlobalTexture(_TextureShaderID, DataTexture);
            }
        }

        internal ShadowLod()
        {
            _Enabled = true;
            _TextureFormat = GraphicsFormat.R8G8_UNorm;
        }

        internal static SortedList<int, ILodInput> s_Inputs = new(Helpers.DuplicateComparison);
        private protected override SortedList<int, ILodInput> Inputs => s_Inputs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnLoad()
        {
            s_Inputs.Clear();
        }
    }
}
