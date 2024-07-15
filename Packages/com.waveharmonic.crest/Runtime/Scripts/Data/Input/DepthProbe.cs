// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.Universal;
using WaveHarmonic.Crest.Editor;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Renders terrain height / water depth once into a render target to cache this off
    /// and avoid rendering it every frame. This should be used for static geometry,
    /// dynamic objects should be tagged with the Water Depth Input component.
    /// </summary>
    [@ExecuteDuringEditMode]
    [@HelpURL("Manual/ShallowsAndShorelines.html")]
    [AddComponentMenu(Constants.k_MenuPrefixInputs + "Depth Probe")]
    public sealed partial class DepthProbe : ManagedBehaviour<WaterRenderer>
#if UNITY_EDITOR
        , IOptionalDepthLod
#endif
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414


        [Tooltip("Specifies the setup for this probe.")]
        [SerializeField]
        internal ProbeMode _Type = ProbeMode.Realtime;

        [Tooltip("Controls how the probe is refreshed in the Player. Call Populate() if scripting.")]
        [@Predicated(nameof(_Type), inverted: true, nameof(ProbeMode.Realtime), hide: true)]
        [@DecoratedField, SerializeField]
        internal ProbeRefreshMode _RefreshMode = ProbeRefreshMode.OnStart;


        [@Heading("Capture")]

        [Tooltip("The layers to render into the probe.")]
        [@Predicated(nameof(_Type), inverted: true, nameof(ProbeMode.Realtime))]
        [@DecoratedField, SerializeField]
        internal LayerMask _Layers = 1; // Default

        [Tooltip("The resolution of the probe - lower will be more efficient.")]
        [@Predicated(nameof(_Type), inverted: true, nameof(ProbeMode.Realtime))]
        [@DecoratedField, SerializeField]
        internal int _Resolution = 512;

        // A big hill will still want to write its height into the depth texture
        [Tooltip("The 'near plane' for the depth probe camera (top down).")]
        [@Predicated(nameof(_Type), inverted: true, nameof(ProbeMode.Realtime))]
        [@DecoratedField, SerializeField]
        internal float _MaximumHeight = 100f;

        [@DecoratedField, SerializeField]
        QualitySettingsOverride _QualitySettingsOverride = new()
        {
            _OverrideLodBias = true,
            _LodBias = Mathf.Infinity,
            _OverrideMaximumLodLevel = true,
            _MaximumLodLevel = 0,
            _OverrideTerrainPixelError = true,
            _TerrainPixelError = 0,
        };

        [@Space(10)]

        [Tooltip("Baked probe. Can only bake in edit mode.")]
        [@Disabled]
        [@Predicated(nameof(_Type), inverted: true, nameof(ProbeMode.Baked), hide: true)]
        [@DecoratedField, SerializeField]
#pragma warning disable 649
        internal Texture2D _SavedTexture;
#pragma warning restore 649


        [@Heading("Signed Distance Field")]

        [@Label("Generate")]
        [Tooltip("Generate a signed distance field for the shoreline.")]
        [@DecoratedField, SerializeField]
        internal bool _GenerateSignedDistanceField = true;

        // Additional rounds of jump flood can help reduce innacuracies from JFA, see paper for details.
        [Tooltip("How many additional Jump Flood Algorithm rounds to use - (over the standard log2(Resolution).")]
        [@Predicated(nameof(_GenerateSignedDistanceField))]
        [@DecoratedField, SerializeField]
        int _AdditionalJumpFloodRounds = 7;


        [@Space(10)]

        [@DecoratedField, SerializeField]
        internal DebugFields _Debug = new();

        [Serializable]
        internal sealed class DebugFields
        {
            [Tooltip("Will render into the probe every frame. Intended for debugging, will generate garbage.")]
            [@Predicated(nameof(_Type), inverted: true, nameof(ProbeMode.Realtime))]
            [@DecoratedField, SerializeField]
            public bool _ForceAlwaysUpdateDebug;

            [Tooltip("Shows hidden objects like the camera which renders into the probe.")]
            [@DecoratedField, SerializeField]
            public bool _ShowHiddenObjects;
        }


        internal enum ProbeMode
        {
            Realtime,
            Baked,
        }

        internal enum ProbeRefreshMode
        {
            OnStart = 0,
            // EveryFrame,
            ViaScripting = 2,
        }

        internal Camera _Camera;
        Material _CopyDepthMaterial;

        internal ProbeMode Type => _Type;
        internal ProbeRefreshMode RefreshMode => _RefreshMode;
        internal float MaximumTerrainHeight => _MaximumHeight;
        internal Texture Texture => _Type == ProbeMode.Baked ? SavedTexture : RealtimeTexture;
        internal Texture2D SavedTexture => _SavedTexture;
        internal RenderTexture RealtimeTexture { get; set; }

        internal static class ShaderIDs
        {
            public static readonly int s_CamDepthBuffer = Shader.PropertyToID("_CamDepthBuffer");
            public static readonly int s_CustomZBufferParams = Shader.PropertyToID("_CustomZBufferParams");
            public static readonly int s_HeightNearHeightFar = Shader.PropertyToID("_HeightNearHeightFar");
            public static readonly int s_HeightOffset = Shader.PropertyToID("_HeightOffset");

            // Bind
            public static readonly int s_DepthProbe = Shader.PropertyToID("_Crest_DepthProbe");
            public static readonly int s_DepthProbeHeightOffset = Shader.PropertyToID("_Crest_DepthProbeHeightOffset");
            public static readonly int s_DepthProbeResolution = Shader.PropertyToID("_Crest_DepthProbeResolution");


            // SDF
            public static readonly int s_JumpSize = Shader.PropertyToID("_Crest_JumpSize");
            public static readonly int s_ProjectionToWorld = Shader.PropertyToID("_Crest_ProjectionToWorld");
            public static readonly int s_VoronoiPingPong0 = Shader.PropertyToID("_Crest_VoronoiPingPong0");
            public static readonly int s_VoronoiPingPong1 = Shader.PropertyToID("_Crest_VoronoiPingPong1");
        }

#if d_UnityHDRP
        static readonly List<FrameSettingsField> s_FrameSettingsFields = new()
        {
            FrameSettingsField.OpaqueObjects,
            FrameSettingsField.TransparentObjects,
            FrameSettingsField.TransparentPrepass,
            FrameSettingsField.TransparentPostpass,
            FrameSettingsField.AsyncCompute,
        };
#endif

        internal void Bind(IPropertyWrapper wrapper)
        {
            wrapper.SetTexture(ShaderIDs.s_DepthProbe, Texture);
            wrapper.SetFloat(ShaderIDs.s_DepthProbeHeightOffset, transform.position.y);
            wrapper.SetFloat(ShaderIDs.s_DepthProbeResolution, _Resolution);
        }

        /// <inheritdoc/>
        protected override void OnStart()
        {
            base.OnStart();

            if (_Type == ProbeMode.Realtime && _RefreshMode == ProbeRefreshMode.OnStart)
            {
                // Flag is only to update - not for initialization.
                Populate(updateComponents: false);
            }
        }

        void OnDestroy()
        {
            if (_Camera != null) Helpers.Destroy(_Camera.gameObject);
        }

#if UNITY_EDITOR
        void Update()
        {
            if (_Debug._ForceAlwaysUpdateDebug)
            {
                Populate(updateComponents: true);
            }
        }
#endif

        internal bool Outdated =>
                _Camera.orthographicSize != CalculateCameraOrthographicSize() ||
                _Camera.transform.position != CalculateCameraPosition() ||
                IsSavedTextureOutdated(_Camera.targetTexture) ||
                IsSavedTextureOutdated(RealtimeTexture, checkSDF: true);

        float CalculateCameraOrthographicSize()
        {
            return Mathf.Max(transform.lossyScale.x / 2f, transform.lossyScale.z / 2f);
        }

        Vector3 CalculateCameraPosition()
        {
            return transform.position + Vector3.up * _MaximumHeight;
        }

        bool IsSavedTextureOutdated(RenderTexture texture, bool checkSDF = false)
        {
            return texture == null || texture.width != _Resolution || texture.height != _Resolution
                || checkSDF && texture.enableRandomWrite != _GenerateSignedDistanceField;
        }

        RenderTexture MakeRT(bool depthStencilTarget)
        {
            RenderTextureFormat fmt;

            if (depthStencilTarget)
            {
                fmt = RenderTextureFormat.Depth;
            }
            else
            {
                fmt = _GenerateSignedDistanceField ? RenderTextureFormat.RGHalf : RenderTextureFormat.RHalf;
            }

            Debug.Assert(SystemInfo.SupportsRenderTextureFormat(fmt), "Crest: The graphics device does not support the render texture format " + fmt.ToString());
            return new RenderTexture(_Resolution, _Resolution, depthStencilTarget ? 24 : 0)
            {
                name = $"_Crest_WaterDepth_{(depthStencilTarget ? "DepthOnly" : "Cache")}_{gameObject.name}",
                format = fmt,
                useMipMap = false,
                anisoLevel = 0,
                enableRandomWrite = !depthStencilTarget && _GenerateSignedDistanceField,
            };
        }

        bool InitObjects(bool updateComponents)
        {
            if (updateComponents && IsSavedTextureOutdated(RealtimeTexture, checkSDF: true))
            {
                // Destroy the texture so it can be recreated.
                RealtimeTexture.Release();
                RealtimeTexture = null;
            }

            if (RealtimeTexture == null)
            {
                RealtimeTexture = MakeRT(false);
            }

            // We want to know this later.
            var isCameraCreation = _Camera == null;

            if (_Layers == 0)
            {
                Debug.LogError("Crest: No valid layers for populating depth probe, aborting.", this);
                return false;
            }

            if (isCameraCreation)
            {
                _Camera = new GameObject("_Crest_DepthProbeCamera").AddComponent<Camera>();
                _Camera.transform.parent = transform;
                _Camera.transform.localEulerAngles = 90f * Vector3.right;
                _Camera.orthographic = true;
                _Camera.clearFlags = CameraClearFlags.Depth;
                _Camera.enabled = false;
                _Camera.allowMSAA = false;
                _Camera.allowDynamicResolution = false;
                _Camera.depthTextureMode = DepthTextureMode.Depth;
                // Stops behaviour from changing in VR. I tried disabling XR before/after camera render but it makes the editor
                // go bonkers with split windows.
                _Camera.cameraType = CameraType.Reflection;
                // I'd prefer to destroy the camera object, but I found sometimes (on first start of editor) it will fail to render.
                _Camera.gameObject.SetActive(false);

                if (RenderPipelineHelper.IsUniversal)
                {
#if d_UnityURP
                    var additionalCameraData = _Camera.GetUniversalAdditionalCameraData();
                    additionalCameraData.renderShadows = false;
                    additionalCameraData.requiresColorTexture = false;
                    additionalCameraData.requiresDepthTexture = false;
                    additionalCameraData.renderPostProcessing = false;
                    additionalCameraData.allowXRRendering = false;
#endif
                }
                else if (RenderPipelineHelper.IsHighDefinition)
                {
#if d_UnityHDRP
                    var additionalCameraData = _Camera.gameObject.AddComponent<HDAdditionalCameraData>();

                    additionalCameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
                    additionalCameraData.volumeLayerMask = 0;
                    additionalCameraData.probeLayerMask = 0;
                    additionalCameraData.xrRendering = false;

                    // Override camera frame settings to disable most of the expensive rendering for this camera.
                    // Most importantly, disable custom passes and post-processing as third-party stuff might throw
                    // errors because of this camera. Even with excluding a lot of HDRP features, it still does a
                    // lit pass which is not cheap.
                    additionalCameraData.customRenderingSettings = true;

                    foreach (FrameSettingsField frameSetting in Enum.GetValues(typeof(FrameSettingsField)))
                    {
                        if (!s_FrameSettingsFields.Contains(frameSetting))
                        {
                            // Enable override and then disable the feature.
                            additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)frameSetting] = true;
                            additionalCameraData.renderingPathCustomFrameSettings.SetEnabled(frameSetting, false);
                        }
                    }
#endif
                }
            }

            if (updateComponents || isCameraCreation)
            {
                // Calculate here so it is always updated.
                _Camera.transform.position = CalculateCameraPosition();
                _Camera.orthographicSize = CalculateCameraOrthographicSize();
                _Camera.cullingMask = _Layers;
                _Camera.gameObject.hideFlags = _Debug._ShowHiddenObjects ? HideFlags.DontSave : HideFlags.HideAndDontSave;
            }

            if (updateComponents && IsSavedTextureOutdated(_Camera.targetTexture))
            {
                // Destroy the texture so it can be recreated.
                _Camera.targetTexture.Release();
                _Camera.targetTexture = null;
            }

            if (_Camera.targetTexture == null)
            {
                _Camera.targetTexture = MakeRT(true);
            }

            return true;
        }

        /// <summary>
        /// Populates the water depth probe. Call this method if using "On Demand".
        /// </summary>
        public void Populate()
        {
            Populate(false);
        }

        [@OnChange]
        void OnChange(string propertyPath, object oldValue)
        {
            InitObjects(true);
        }

        internal void Populate(bool updateComponents)
        {
            if (_Type == ProbeMode.Baked)
            {
                return;
            }

            ForcePopulate(updateComponents);
        }

        internal void ForcePopulate(bool updateComponents = false)
        {
            if (WaterRenderer.RunningWithoutGraphics)
            {
                // Don't bake in headless mode
                Debug.LogWarning("Crest: Depth probe will not be populated at runtime when in batched/headless mode. Please pre-bake the probe in the Editor.");
                return;
            }

            // Make sure we have required objects.
            if (!InitObjects(updateComponents))
            {
                return;
            }

            var oldShadowDistance = 0f;

            if (RenderPipelineHelper.IsLegacy)
            {
                // Stop shadow passes from executing.
                oldShadowDistance = QualitySettings.shadowDistance;
                QualitySettings.shadowDistance = 0f;
            }

            _QualitySettingsOverride.Override();

            // Render scene, saving depths in depth buffer.
#if d_UnityURP
            if (RenderPipelineHelper.IsUniversal)
            {
                Helpers.RenderCameraWithoutCustomPasses(_Camera);
            }
            else
#endif
            {
                _Camera.Render();
            }

            _QualitySettingsOverride.Restore();

            // Built-in only.
            if (RenderPipelineHelper.IsLegacy)
            {
                QualitySettings.shadowDistance = oldShadowDistance;
            }

            if (_CopyDepthMaterial == null)
            {
                _CopyDepthMaterial = new(WaterResources.Instance.Shaders._CopyDepthIntoCache);
            }

            _CopyDepthMaterial.SetTexture(ShaderIDs.s_CamDepthBuffer, _Camera.targetTexture);

            // Zbuffer params
            //float4 _ZBufferParams;            // x: 1-far/near,     y: far/near, z: x/far,     w: y/far
            float near = _Camera.nearClipPlane, far = _Camera.farClipPlane;
            _CopyDepthMaterial.SetVector(ShaderIDs.s_CustomZBufferParams, new(1f - far / near, far / near, (1f - far / near) / far, (far / near) / far));

            _CopyDepthMaterial.SetFloat(ShaderIDs.s_HeightOffset, transform.position.y);

            // Altitudes for near and far planes
            var ymax = _Camera.transform.position.y - near;
            var ymin = ymax - far;
            _CopyDepthMaterial.SetVector(ShaderIDs.s_HeightNearHeightFar, new(ymax, ymin));

            // Copy from depth buffer into the probe
            Graphics.Blit(null, RealtimeTexture, _CopyDepthMaterial);

            if (_GenerateSignedDistanceField)
            {
                GenerateSignedDistanceField();
            }
        }

        void GenerateSignedDistanceField()
        {
            var shader = WaterResources.Instance.Compute._JumpFloodSDF;

            if (shader == null)
            {
                return;
            }

            var cameraToWorldMatrix = _Camera.cameraToWorldMatrix;
            var projectionMatrix = _Camera.projectionMatrix;
            var projectionToWorldMatrix = cameraToWorldMatrix * projectionMatrix.inverse;

            var buffer = new CommandBuffer();
            buffer.name = "Jump Flood";
            // Common uniforms.
            buffer.SetComputeFloatParam(shader, DepthLodInput.ShaderIDs.s_HeightOffset, transform.position.y);
            buffer.SetComputeIntParam(shader, Crest.ShaderIDs.s_TextureSize, _Resolution);
            buffer.SetComputeMatrixParam(shader, ShaderIDs.s_ProjectionToWorld, projectionToWorldMatrix);

            var descriptor = new RenderTextureDescriptor(_Resolution, _Resolution)
            {
                autoGenerateMips = false,
                colorFormat = RenderTextureFormat.RGHalf,
                useMipMap = false,
                enableRandomWrite = true,
                depthBufferBits = 0,
            };

            var voronoiPingPong0 = ShaderIDs.s_VoronoiPingPong0;
            var voronoiPingPong1 = ShaderIDs.s_VoronoiPingPong1;

            buffer.GetTemporaryRT(voronoiPingPong0, descriptor);
            buffer.GetTemporaryRT(voronoiPingPong1, descriptor);

            // Initialize.
            {
                var kernel = shader.FindKernel("CrestInitialize");

                buffer.SetComputeTextureParam(shader, kernel, Crest.ShaderIDs.s_Source, RealtimeTexture);
                buffer.SetComputeTextureParam(shader, kernel, Crest.ShaderIDs.s_Target, voronoiPingPong0);
                buffer.DispatchCompute
                (
                    shader,
                    kernel,
                    RealtimeTexture.width / Lod.k_ThreadGroupSize,
                    RealtimeTexture.height / Lod.k_ThreadGroupSize,
                    1
                );
            }

            // Jump Flood.
            {
                var kernel = shader.FindKernel("CrestExecute");

                for (var jumpSize = _Resolution / 2; jumpSize > 0; jumpSize /= 2)
                {
                    ApplyJumpFlood
                    (
                        buffer,
                        shader,
                        kernel,
                        jumpSize,
                        voronoiPingPong0,
                        voronoiPingPong1
                    );
                    (voronoiPingPong0, voronoiPingPong1) = (voronoiPingPong1, voronoiPingPong0);
                }

                for (var roundNum = 0; roundNum < _AdditionalJumpFloodRounds; roundNum++)
                {
                    var jumpSize = 1 << roundNum;
                    ApplyJumpFlood
                    (
                        buffer,
                        shader,
                        kernel,
                        jumpSize,
                        voronoiPingPong0,
                        voronoiPingPong1
                    );
                    (voronoiPingPong0, voronoiPingPong1) = (voronoiPingPong1, voronoiPingPong0);
                }
            }

            // Apply.
            {
                var kernel = shader.FindKernel("CrestApply");
                buffer.SetComputeTextureParam(shader, kernel, Crest.ShaderIDs.s_Source, voronoiPingPong0);
                buffer.SetComputeTextureParam(shader, kernel, Crest.ShaderIDs.s_Target, RealtimeTexture);
                buffer.DispatchCompute
                (
                    shader,
                    kernel,
                    _Resolution / Lod.k_ThreadGroupSize,
                    _Resolution / Lod.k_ThreadGroupSize,
                    1
                );
            }

            Graphics.ExecuteCommandBuffer(buffer);
            buffer.ReleaseTemporaryRT(voronoiPingPong0);
            buffer.ReleaseTemporaryRT(voronoiPingPong1);
            buffer.Release();
        }

        void ApplyJumpFlood
        (
            CommandBuffer buffer,
            ComputeShader shader,
            int kernel,
            int jumpSize,
            RenderTargetIdentifier source,
            RenderTargetIdentifier target
        )
        {
            buffer.SetComputeIntParam(shader, ShaderIDs.s_JumpSize, jumpSize);
            buffer.SetComputeTextureParam(shader, kernel, Crest.ShaderIDs.s_Source, source);
            buffer.SetComputeTextureParam(shader, kernel, Crest.ShaderIDs.s_Target, target);
            buffer.DispatchCompute
            (
                shader,
                kernel,
                _Resolution / Lod.k_ThreadGroupSize,
                _Resolution / Lod.k_ThreadGroupSize,
                1
            );
        }
    }

    // LodInput
    partial class DepthProbe
    {
        Input _Input;

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();
            _Input ??= new(this);
            ILodInput.Attach(_Input, DepthLod.s_Inputs);
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            base.OnDisable();
            ILodInput.Detach(_Input, DepthLod.s_Inputs);
        }

        sealed class Input : ILodInput
        {
            public bool Enabled => _Probe.Texture != null;
            public bool IsCompute => true;
            public int Queue => 0;
            public int Pass => -1;
            public Rect Rect => _Probe.transform.RectXZ();
            public MonoBehaviour Component => _Probe;
            public float Filter(WaterRenderer water, int slice) => 1f;

            readonly DepthProbe _Probe;

            public Input(DepthProbe probe)
            {
                _Probe = probe;
            }

            public void Draw(Lod lod, CommandBuffer buffer, RenderTexture target, int pass = -1, float weight = 1, int slices = -1)
            {
                var resources = WaterResources.Instance;
                var wrapper = new PropertyWrapperCompute(buffer, resources.Compute._DepthTexture, 0);

                var transform = _Probe.transform;

                // Texture Input
                wrapper.SetVector(Crest.ShaderIDs.s_TextureSize, transform.lossyScale.XZ());
                wrapper.SetVector(Crest.ShaderIDs.s_TexturePosition, transform.position.XZ());
                wrapper.SetVector(Crest.ShaderIDs.s_TextureRotation, transform.RotationXZ());
                wrapper.SetInteger(Crest.ShaderIDs.s_Blend, (int)Blend.Maximum);
                wrapper.SetTexture(Crest.ShaderIDs.s_Texture, _Probe.Texture);
                wrapper.SetTexture(Crest.ShaderIDs.s_Target, target);

                // Depth Input
                wrapper.SetFloat(DepthLodInput.ShaderIDs.s_HeightOffset, transform.position.y);
                wrapper.SetInteger(DepthLodInput.ShaderIDs.s_SDF, _Probe._GenerateSignedDistanceField ? 1 : 0);
                wrapper.SetKeyword(resources.Keywords.DepthTextureSDF, lod._Water._DepthLod._EnableSignedDistanceFields);

                var threads = lod.Resolution / Lod.k_ThreadGroupSize;
                wrapper.Dispatch(threads, threads, slices);
            }
        }
    }
}
