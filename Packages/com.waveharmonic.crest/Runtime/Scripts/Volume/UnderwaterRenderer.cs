// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Rendering;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest
{
    [System.Serializable]
    sealed partial class UnderwaterRenderer
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        internal const float k_CullLimitMinimum = 0.000001f;
        internal const float k_CullLimitMaximum = 0.01f;

        [@Space(10)]

        [@DecoratedField, SerializeField]
        internal bool _Enabled = true;

        [Tooltip("Any camera or probe with this layer in its culling mask will render underwater.")]
        [@Layer]
        [SerializeField]
        int _Layer = 4; // Water

        [@AttachMaterialEditor]
        [@MaterialField("Crest/Underwater", name: "Underwater", title: "Create Underwater Material")]
        [SerializeField]
        internal Material _Material;


        [@Heading("Environmental Lighting")]

        [@Label("Enable")]
        [Tooltip("Provides out-scattering based on the camera's underwater depth. It scales down environmental lighting (sun, reflections, ambient etc) with the underwater depth. This works with vanilla lighting, but uncommon or custom lighting will require a custom solution (use this for reference)")]
        [@DecoratedField, SerializeField]
        internal bool _EnvironmentalLightingEnable;

        [@Label("Weight")]
        [Tooltip("How much this effect applies. Values less than 1 attenuate light less underwater. Value of 1 is physically based.")]
        [@Range(0, 3)]
        [SerializeField]
        internal float _EnvironmentalLightingWeight = 1f;

#if d_UnitySRP
        [@Label("Volume")]
        [Tooltip("This profile will be weighed in the deeper underwater the camera goes.")]
        [@Predicated(RenderPipeline.HighDefinition, hide: true)]
        [@DecoratedField, SerializeField]
        VolumeProfile _EnvironmentalLightingVolumeProfile = null;

        Volume _EnvironmentalLightingVolume;
#endif


        [@Heading("Shader API")]

        [Tooltip("Renders the underwater effect before the transparent pass (instead of after). So one can apply the underwater fog themselves to transparent objects. Cannot be changed at runtime.")]
        [@DecoratedField, SerializeField]
        [HideInInspector]
        bool _EnableShaderAPI = false;
        internal bool EnableShaderAPI { get => _EnableShaderAPI; set => _EnableShaderAPI = value; }

        [@Predicated(nameof(_EnableShaderAPI))]
        [@Predicated(RenderPipeline.Legacy, inverted: true, hide: true)]
        [@DecoratedField, SerializeField]
        [HideInInspector]
        internal LayerMask _TransparentObjectLayers;


        [@Heading("Advanced")]

        [Tooltip("If enabled then additionally ignore any camera that is not the view camera or our reflection camera. It will require managing culling masks of all cameras.")]
        [@DecoratedField, SerializeField]
        bool _AllCameras;

        [Tooltip("Copying params each frame ensures underwater appearance stays consistent with water material params. Has a small overhead so should be disabled if not needed.")]
        [@DecoratedField, SerializeField]
        bool _CopyWaterMaterialParametersEachFrame = true;

        [Tooltip("Adjusts the far plane for horizon line calculation. Helps with horizon line issue.")]
        [@Range(0f, 1f)]
        [SerializeField]
        float _FarPlaneMultiplier = 0.68f;

        [Tooltip("Proportion of visibility below which water will be culled underwater. The larger the number, the closer to the camera the water tiles will be culled.")]
        [@Range(k_CullLimitMinimum, k_CullLimitMaximum)]
        [SerializeField]
        internal float _CullLimit = 0.001f;

        [@Space(10)]

        [@DecoratedField, SerializeField]
        DebugFields _Debug = new();

        [System.Serializable]
        sealed class DebugFields
        {
            [SerializeField]
            internal bool _VisualizeMask;

            [SerializeField]
            internal bool _DisableMask;

            [SerializeField]
            internal bool _VisualizeStencil;

            [SerializeField]
            internal bool _DisableHeightAboveWaterOptimization;

            [SerializeField]
            internal bool _DisableArtifactCorrection;

            [SerializeField]
            internal bool _OnlyReflectionCameras;
        }

        internal WaterRenderer _Water;

#if d_CrestPortals
        // BUG: NonSerialized as Unity shows a serialization depth warning even though field is internal.
        [System.NonSerialized]
        internal Portals.PortalRenderer _Portals;
        bool Portaled => _Portals.Active;
#else
        bool Portaled => false;
#endif

        bool _FirstRender = true;

        internal bool UseStencilBufferOnMask { get; set; }
        internal bool UseStencilBufferOnEffect { get; set; }

        internal int Layer => _Layer;

        internal enum Pass
        {
            Culling,
            Mask,
            Effect,
        }

        // These are the materials we actually use, overridable by Water Body.
        Material _SurfaceMaterial;
        Material _VolumeMaterial;

        readonly SampleHeightHelper _SamplingHeightHelper = new();
        float _ViewerWaterHeight;

        public static partial class ShaderIDs
        {
            // Empty.
        }

        // Disable underwater effect if height enough above surface.
        internal bool Active => _Enabled && _Material != null && _ViewerWaterHeight < 2f || Portaled || _Debug._DisableHeightAboveWaterOptimization;

        internal void OnEnable()
        {
            _VolumeMaterial = _Material;

            if (_MaskMaterial == null)
            {
                _MaskMaterial = new(WaterResources.Instance.Shaders._UnderwaterMask);
            }

            if (_ArtifactsShader == null)
            {
                _ArtifactsShader = WaterResources.Instance.Compute._UnderwaterArtifacts;
            }

            if (!RenderPipelineHelper.IsLegacy)
            {
                RenderPipelineManager.beginCameraRendering -= OnBeforeCulling;
                RenderPipelineManager.beginCameraRendering += OnBeforeCulling;
            }

            if (RenderPipelineHelper.IsUniversal)
            {
#if d_UnityURP
                UnderwaterMaskPassURP.Enable(this);
                UnderwaterEffectPassURP.Enable(this);
#endif
            }
            else if (RenderPipelineHelper.IsHighDefinition)
            {
#if d_UnityHDRP
                UnderwaterMaskPassHDRP.Enable(this);
                UnderwaterEffectPassHDRP.Enable(this);
#endif
            }
            else
            {
                OnEnableLegacy();
            }

            EnableEnvironmentalLighting();

            RenderPipelineManager.activeRenderPipelineTypeChanged -= OnActiveRenderPipelineTypeChanged;
            RenderPipelineManager.activeRenderPipelineTypeChanged += OnActiveRenderPipelineTypeChanged;
        }

        void OnActiveRenderPipelineTypeChanged()
        {
            // Disable is handled by another handler so we need to run enabled.
            if (_Water.isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        internal void OnDisable()
        {
            RenderPipelineManager.activeRenderPipelineTypeChanged -= OnActiveRenderPipelineTypeChanged;

#if d_UnityURP
            UnderwaterMaskPassURP.Disable();
            UnderwaterEffectPassURP.Disable();
#endif

#if d_UnityHDRP
            UnderwaterMaskPassHDRP.Disable();
            UnderwaterEffectPassHDRP.Disable();
#endif

            OnDisableLegacy();

            RenderPipelineManager.beginCameraRendering -= OnBeforeCulling;

            DisableEnvironmentalLighting();

            _ArtifactsShader = null;
        }

        internal void OnDestroy()
        {
            Helpers.Destroy(_MaskMaterial);
        }

        internal bool ShouldRender(Camera camera, Pass pass)
        {
            if (_Water == null)
            {
                return false;
            }

            if (!Helpers.MaskIncludesLayer(camera.cullingMask, _Layer))
            {
                return false;
            }

#if UNITY_EDITOR
            // Do not execute when editor is not active to conserve power and prevent possible leaks.
            if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                return false;
            }

            if (GL.wireframe)
            {
                return false;
            }

            // Skip camera if fog is disabled. Do not skip if mask pass and a portal or volume as we want it to still
            // mask the water surface.
            if ((pass != Pass.Mask || !Portaled) && !IsFogEnabledForEditorCamera(camera))
            {
                return false;
            }

            if (_Water.IsProxyPlaneRendering)
            {
                return false;
            }

            if (camera.cameraType == CameraType.Preview)
            {
                return false;
            }
#endif

            var isReflectionCamera = camera.cameraType == CameraType.Reflection;

            // Mask or culling is not needed for reflections.
            if (isReflectionCamera && pass != Pass.Effect)
            {
                return false;
            }

            if (_Debug._OnlyReflectionCameras && !isReflectionCamera)
            {
                return false;
            }

            // Option to exclude cameras that is not the view camera or our reflection camera.
            // Otherwise, filtering depends on the camera's culling mask which is not always
            // accessible like with the global "Reflection Probes Camera". But whether those
            // cameras triggering camera events is a bug is TBD as it is intermittent.
            if (!_AllCameras && camera != _Water.Viewer && camera.cameraType != CameraType.SceneView && camera != WaterReflections.CurrentCamera)
            {
                return false;
            }

            if (pass != Pass.Culling && !Active)
            {
                return false;
            }

            return true;
        }

        void RevertCulling()
        {
            foreach (var tile in _Water.Chunks)
            {
                if (tile.Rend == null || tile._Culled)
                {
                    continue;
                }

                tile.Rend.enabled = true;
            }
        }

        void OnBeforeCulling(ScriptableRenderContext context, Camera camera) => OnBeforeCulling(camera);

        void OnBeforeCulling(Camera camera)
        {
            if (!ShouldRender(camera, Pass.Culling))
            {
                return;
            }

            var viewpoint = camera.transform.position;
            _SamplingHeightHelper.Init(viewpoint, allowMultipleCallsPerFrame: true);
            _SamplingHeightHelper.Sample(_Water, System.HashCode.Combine(GetHashCode(), camera.GetHashCode()), out var height);
            _ViewerWaterHeight = viewpoint.y - height;

            if (!Active)
            {
                RevertCulling();
                RestoreEnvironmentalLighting();
                return;
            }

            _SurfaceMaterial = _Water.AboveOrBelowSurfaceMaterial;
            _VolumeMaterial = _Material;

            // Grab material from a water body if camera is within its XZ bounds.
            foreach (var body in WaterBody.WaterBodies)
            {
                if (body.AboveOrBelowSurfaceMaterial == null && body._VolumeMaterial == null)
                {
                    continue;
                }

                var bounds = body.AABB;
                var contained =
                    viewpoint.x >= bounds.min.x && viewpoint.x <= bounds.max.x &&
                    viewpoint.z >= bounds.min.z && viewpoint.z <= bounds.max.z;

                if (contained)
                {
                    if (body.AboveOrBelowSurfaceMaterial != null) _SurfaceMaterial = body.AboveOrBelowSurfaceMaterial;
                    if (body.VolumeMaterial != null) _VolumeMaterial = body.VolumeMaterial;
                    // Water bodies should not overlap so grab the first one.
                    break;
                }
            }

            var extinction = Vector3.zero;
            float minimumFogDensity = 0;

            // Calculate extinction.
            if (_SurfaceMaterial != null)
            {
                var densityFactor = _VolumeMaterial.GetFloat(ShaderIDs.s_ExtinctionMultiplier);

                // Get absorption from current material.
                if (_SurfaceMaterial.HasVector(WaterRenderer.ShaderIDs.s_Absorption))
                {
                    extinction = _SurfaceMaterial.GetVector(WaterRenderer.ShaderIDs.s_Absorption);
                    Shader.SetGlobalVector(WaterRenderer.ShaderIDs.s_Absorption, extinction);
                }

                // Do not use for culling because:
                // - Scattering is not uniform due to anisotropy
                // - Also need to take sun light into account
                if (_SurfaceMaterial.HasProperty(WaterRenderer.ShaderIDs.s_Scattering))
                {
                    var volumeExtinction = extinction + _SurfaceMaterial.GetVector(WaterRenderer.ShaderIDs.s_Scattering).XYZ();
                    volumeExtinction *= densityFactor;
                    minimumFogDensity = Mathf.Min(Mathf.Min(volumeExtinction.x, volumeExtinction.y), volumeExtinction.z);
                    Shader.SetGlobalFloat(WaterRenderer.ShaderIDs.s_VolumeExtinctionLength, -Mathf.Log(k_CullLimitMinimum) / minimumFogDensity);
                }

                extinction *= densityFactor;
                minimumFogDensity = Mathf.Min(Mathf.Min(extinction.x, extinction.y), extinction.z);
                // Prevent divide by zero.
                minimumFogDensity = Mathf.Max(minimumFogDensity, 0.0001f);
            }

            UpdateEnvironmentalLighting(camera, extinction, _ViewerWaterHeight);

            if (Portaled || _ViewerWaterHeight > -5f)
            {
                RevertCulling();
                return;
            }

            var extinctionLength = -Mathf.Log(_CullLimit) / minimumFogDensity;

            foreach (var tile in _Water.Chunks)
            {
                if (tile.Rend == null || tile._Culled)
                {
                    continue;
                }

                // Cull tiles the viewer cannot see through the underwater fog.
                // Only run optimisation in play mode due to shared height above water.
                if ((viewpoint - tile.Rend.bounds.ClosestPoint(viewpoint)).magnitude >= extinctionLength)
                {
                    tile.Rend.enabled = false;
                }
                else
                {
                    // Previous camera might have culled in underwater pass.
                    tile.Rend.enabled = true;
                }
            }
        }

#if UNITY_EDITOR
        [@OnChange]
        void OnChange(string propertyPath, object previousValue)
        {
            switch (propertyPath)
            {
                case nameof(_Enabled):
                    if (_Enabled) OnEnable(); else OnDisable();
                    break;
            }
        }
#endif
    }
}
