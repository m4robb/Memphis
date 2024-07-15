// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using WaveHarmonic.Crest.Internal;
using WaveHarmonic.Crest.RelativeSpace;
using WaveHarmonic.Crest.Utility;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// The main script for the water system. Attach this to a GameObject to create an water. This script initializes the various data types and systems
    /// and moves/scales the water based on the viewpoint. It also hosts a number of global settings that can be tweaked here.
    /// </summary>
    public sealed partial class WaterRenderer : ManagerBehaviour<WaterRenderer>
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        internal static partial class ShaderIDs
        {
            public static readonly int s_Center = Shader.PropertyToID("g_Crest_WaterCenter");
            public static readonly int s_Scale = Shader.PropertyToID("g_Crest_WaterScale");
            public static readonly int s_Time = Shader.PropertyToID("g_Crest_Time");
            public static readonly int s_CascadeData = Shader.PropertyToID("g_Crest_CascadeData");
            public static readonly int s_CascadeDataSource = Shader.PropertyToID("g_Crest_CascadeDataSource");
            public static readonly int s_LodChange = Shader.PropertyToID("g_Crest_LodChange");
            public static readonly int s_MeshScaleLerp = Shader.PropertyToID("g_Crest_MeshScaleLerp");
            public static readonly int s_LodCount = Shader.PropertyToID("g_Crest_LodCount");
            public static readonly int s_LodAlphaBlackPointFade = Shader.PropertyToID("g_Crest_LodAlphaBlackPointFade");
            public static readonly int s_LodAlphaBlackPointWhitePointFade = Shader.PropertyToID("g_Crest_LodAlphaBlackPointWhitePointFade");
            public static readonly int s_ForceUnderwater = Shader.PropertyToID("g_Crest_ForceUnderwater");

            // Shader Properties
            public static readonly int s_AbsorptionColor = Shader.PropertyToID("_Crest_AbsorptionColor");
            public static readonly int s_Absorption = Shader.PropertyToID("_Crest_Absorption");
            public static readonly int s_Scattering = Shader.PropertyToID("_Crest_Scattering");
            public static readonly int s_Anisotropy = Shader.PropertyToID("_Crest_Anisotropy");
            public static readonly int s_PlanarReflectionsEnabled = Shader.PropertyToID("_Crest_PlanarReflectionsEnabled");
            public static readonly int s_Occlusion = Shader.PropertyToID("_Crest_Occlusion");
            public static readonly int s_OcclusionUnderwater = Shader.PropertyToID("_Crest_OcclusionUnderwater");

            // Motion Vectors
            public static readonly int s_CenterDelta = Shader.PropertyToID("g_Crest_WaterCenterDelta");
            public static readonly int s_ScaleChange = Shader.PropertyToID("g_Crest_WaterScaleChange");
            public static readonly int s_UnityTime = Shader.PropertyToID("g_Crest_UnityTime");

            // Underwater
            public static readonly int s_VolumeExtinctionLength = Shader.PropertyToID("_Crest_VolumeExtinctionLength");


            // High Definition Render Pipeline
            public static readonly int s_PrimaryLightDirection = Shader.PropertyToID("g_Crest_PrimaryLightDirection");
            public static readonly int s_PrimaryLightIntensity = Shader.PropertyToID("g_Crest_PrimaryLightIntensity");
        }


        //
        // Viewer
        //

        /// <summary>
        /// Center of detail for the LOD system.
        /// </summary>
        public Transform Viewpoint
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && _FollowSceneCamera && SceneView.lastActiveSceneView != null && IsSceneViewActive)
                {
                    return SceneView.lastActiveSceneView.camera.transform;
                }
#endif
                if (_Viewpoint != null)
                {
                    return _Viewpoint;
                }

                // Even with performance improvements, it is still good to cache whenever possible.
                var camera = Viewer;

                if (camera != null)
                {
                    return camera.transform;
                }

                return null;
            }
            set
            {
                _Viewpoint = value;
            }
        }

        /// <summary>
        /// The viewing camera. Normaly the camera tagged as Main Camera.
        /// </summary>
        public Camera Viewer
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && _FollowSceneCamera && SceneView.lastActiveSceneView != null && IsSceneViewActive)
                {
                    return SceneView.lastActiveSceneView.camera;
                }
#endif

                if (_Camera != null)
                {
                    return _Camera;
                }

                // Unity has greatly improved performance of this operation in 2019.4.9.
                return Camera.main;
            }
            set
            {
                _Camera = value;
            }
        }

        // Cache the ViewCamera property for internal use.
        Camera _ViewCameraCached;


        //
        // Viewer Height
        //

        /// <summary>
        /// The water changes scale when viewer changes altitude, this gives the interpolation param between scales.
        /// </summary>
        internal float ViewerAltitudeLevelAlpha { get; private set; }

        /// <summary>
        /// Vertical offset of camera vs water surface.
        /// </summary>
        public float ViewerHeightAboveWater { get; private set; }

        /// <summary>
        /// Smoothly varying version of viewer height to combat sudden changes in water level that are possible
        /// when there are local bodies of water
        /// </summary>
        float _ViewerHeightAboveWaterSmooth;

        readonly SampleHeightHelper _SampleHeightHelper = new();

        float _ViewerHeightAboveWaterPerCamera;
        readonly SampleHeightHelper _SampleHeightHelperPerCamera = new();


        //
        // Teleport Threshold
        //

        float _TeleportTimerForHeightQueries;
        bool _IsFirstFrameSinceEnabled = true;
        internal bool _HasTeleportedThisFrame;
        Vector3 _OldViewerPosition;

#if d_WaveHarmonic_Crest_ShiftingOrigin
        Vector3 TeleportOriginThisFrame => ShiftingOrigin.ShiftThisFrame;
#else
        Vector3 TeleportOriginThisFrame => Vector3.zero;
#endif

        //
        // Serialized Fields
        //

        internal float WindSpeedKPH => _WindSpeed;

        //
        // Transform
        //

        internal Transform Root { get; private set; }
        internal GameObject Container { get; private set; }

        /// <summary>
        /// Sea level is given by y coordinate of GameObject with WaterRenderer script.
        /// </summary>
        public float SeaLevel => Root.position.y;

        // Anything higher (minus 1 for near plane) will be clipped.
        const float k_RenderAboveSeaLevel = 10000f;
        // Anything lower will be clipped.
        const float k_RenderBelowSeaLevel = 10000f;

        Matrix4x4[] _ProjectionMatrix;
        internal Matrix4x4 GetProjectionMatrix(int slice) => _ProjectionMatrix[slice];

        internal static Matrix4x4 CalculateViewMatrixFromSnappedPositionRHS(Vector3 snapped)
        {
            return Helpers.CalculateWorldToCameraMatrixRHS(snapped + Vector3.up * k_RenderAboveSeaLevel, Quaternion.AngleAxis(90f, Vector3.right));
        }


        //
        // Time Provider
        //

        /// <summary>
        /// Loosely a stack for time providers. The last TP in the list is the active one.
        /// When a TP gets added to the stack, it is bumped to the top of the list. When a
        /// TP is removed, all instances of it are removed from the stack. This is less
        /// rigid than a real stack which would be harder to use as users have to keep a
        /// close eye on the order that things are pushed/popped.
        /// </summary>
        public Utility.Stack<ITimeProvider> TimeProviders { get; private set; } = new();

        /// <summary>
        /// The current time provider.
        /// </summary>
        public ITimeProvider TimeProvider => TimeProviders.Peek();

        internal float CurrentTime => TimeProvider.Time;
        internal float DeltaTime => TimeProvider.Delta;


        //
        // Environment
        //

        /// <summary>
        /// The primary light that affects the water. This should be a directional light.
        /// </summary>
        public Light PrimaryLight => _PrimaryLight == null ? RenderSettings.sun : _PrimaryLight;

        /// <summary>
        /// Physics gravity applied to water.
        /// </summary>
        public float Gravity => _GravityMultiplier * Physics.gravity.magnitude;


        //
        // Rendering
        //

        /// <summary>
        /// The water material.
        /// </summary>
        public Material Material { get => _Material; set => _Material = value; }
        internal Material AboveOrBelowSurfaceMaterial => _VolumeMaterial == null ? _Material : _VolumeMaterial;

        /// <summary>
        /// The water layer.
        /// </summary>
        public int Layer => _Layer;

        enum SurfaceSelfIntersectionFixMode
        {
            Off,
            ForceBelowWater,
            ForceAboveWater,
            On,
            Automatic,
        }

#if d_CrestPortals
        bool Portaled => _Portals.Active;
#else
        bool Portaled => false;
#endif

        internal bool CastShadows => _CastShadows && !RenderPipelineHelper.IsLegacy;


        //
        // Material
        //

        internal static void UpdateAbsorptionFromColor(Material material)
        {
            var fogColour = material.GetColor(ShaderIDs.s_AbsorptionColor);
            var alpha = Vector3.zero;
            alpha.x = Mathf.Log(Mathf.Max(fogColour.r, 0.0001f));
            alpha.y = Mathf.Log(Mathf.Max(fogColour.g, 0.0001f));
            alpha.z = Mathf.Log(Mathf.Max(fogColour.b, 0.0001f));
            // Magic numbers that make fog density easy to control using alpha channel
            material.SetVector(ShaderIDs.s_Absorption, -fogColour.a * 32f * alpha / 5f);
        }


        //
        // Simulations
        //

        internal List<Lod> Simulations { get; } = new();
        internal AnimatedWavesLod AnimatedWavesLod => _AnimatedWavesLod;
        internal DepthLod DepthLod => _DepthLod;
        internal LevelLod LevelLod => _LevelLod;
        internal FoamLod FoamLod => _FoamLod;
        internal DynamicWavesLod DynamicWavesLod => _DynamicWavesLod;
        internal FlowLod FlowLod => _FlowLod;
        internal ShadowLod ShadowLod => _ShadowLod;
        internal ClipLod ClipLod => _ClipLod;
        internal AlbedoLod AlbedoLod => _AlbedoLod;


        //
        // Water Chunks
        //

        internal List<WaterChunkRenderer> Chunks { get; } = new();


        //
        // Water Chunk Culling
        //

        bool _CanSkipCulling;


        //
        // Instance
        //

        /// <summary>
        /// The water singleton instance.
        /// </summary>
        public static WaterRenderer Instance { get; private set; }
        bool _Initialized;
        internal bool Active => enabled && this == Instance;


        //
        // Hash
        //

        // A hash of the settings used to generate the water, used to regenerate when necessary
        int _GeneratedSettingsHash;


        //
        // Runtime Environment
        //

        /// <summary>
        /// Is runtime environment without graphics card
        /// </summary>
        public static bool RunningWithoutGraphics
        {
            get
            {
                var noGPU = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
                var emulateNoGPU = Instance != null && Instance._Debug._ForceNoGraphics;
                return noGPU || emulateNoGPU;
            }
        }

        /// <summary>
        /// Is runtime environment non-interactive (not displaying to user).
        /// </summary>
        public static bool RunningHeadless => Application.isBatchMode || (Instance != null && Instance._Debug._ForceBatchMode);


        //
        // Frame Timing
        //

        internal int LastUpdateFrame { get; private set; } = -1;

        /// <summary>
        /// The frame count for Crest.
        /// </summary>
        public static int FrameCount
        {
            get
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    return s_EditorFrames;
                }
                else
#endif
                {
                    return Time.frameCount;
                }
            }
        }


        //
        // Level of Detail
        //

        // We are computing these values to be optimal based on the base mesh vertex density.
        float _LodAlphaBlackPointFade;
        float _LodAlphaBlackPointWhitePointFade;

        /// <summary>
        /// The number of LODs/scales that the water is currently using.
        /// </summary>
        public int LodLevels => _Slices;

        /// <summary>
        /// The resolution of the data.
        /// </summary>
        public int LodResolution => _Resolution;

        internal CommandBuffer SimulationBuffer { get; private set; }

        internal BufferedData<Vector4[]> _CascadeData;

        internal struct PerCascadeInstanceData
        {
            public float _MeshScaleLerp;
            public float _FarNormalsWeight;
            public float _GeometryGridWidth;
            public Vector2 _NormalScrollSpeeds;
        }

        internal BufferedData<PerCascadeInstanceData[]> _PerCascadeInstanceData;
        internal int BufferSize { get; private set; }

        internal float MaximumWavelength(int slice)
        {
            var maximumDiameter = 4f * Scale * Mathf.Pow(2f, slice);
            // TODO: Do we need to pass in resolution? Could resolution mismatch with animated
            // and dynamic waves be an issue?
            var maximumTexelSize = maximumDiameter / LodResolution;
            var texelsPerWave = 2f;
            return 2f * maximumTexelSize * texelsPerWave;
        }


        //
        // Scale
        //

        /// <summary>
        /// Current water scale (changes with viewer altitude).
        /// </summary>
        public float Scale { get; private set; }
        internal float CalcLodScale(float slice) => Scale * Mathf.Pow(2f, slice);
        internal float CalcGridSize(int slice) => CalcLodScale(slice) / LodResolution;

        /// <summary>
        /// Could the water horizontal scale increase (for e.g. if the viewpoint gains altitude). Will be false if water already at maximum scale.
        /// </summary>
        internal bool ScaleCouldIncrease => _ScaleRange.y < Mathf.Infinity || Root.localScale.x < _ScaleRange.y * 0.99f;
        /// <summary>
        /// Could the water horizontal scale decrease (for e.g. if the viewpoint drops in altitude). Will be false if water already at minimum scale.
        /// </summary>
        internal bool ScaleCouldDecrease => Root.localScale.x > _ScaleRange.x * 1.01f;

        internal int ScaleDifferencePower2 { get; private set; }


        //
        // Displacement Reporting
        //

        /// <summary>
        /// User shape inputs can report in how far they might displace the shape horizontally and vertically. The max value is
        /// saved here. Later the bounding boxes for the water tiles will be expanded to account for this potential displacement.
        /// </summary>
        internal void ReportMaximumDisplacement(float horizontal, float vertical, float verticalFromWaves)
        {
            MaximumHorizontalDisplacement += horizontal;
            MaximumVerticalDisplacement += vertical;
            _MaximumVerticalDisplacementFromWaves += verticalFromWaves;
        }

        float _MaximumVerticalDisplacementFromWaves = 0f;
        /// <summary>
        /// The maximum horizontal distance that the shape scripts are displacing the shape.
        /// </summary>
        internal float MaximumHorizontalDisplacement { get; private set; }
        /// <summary>
        /// The maximum height that the shape scripts are displacing the shape.
        /// </summary>
        internal float MaximumVerticalDisplacement { get; private set; }


        //
        // Query Providers
        //

        /// <summary>
        /// Provides water shape to CPU.
        /// </summary>
        public ICollisionProvider CollisionProvider { get; private set; }

        /// <summary>
        /// Provides flow to the CPU.
        /// </summary>
        public IFlowProvider FlowProvider { get; private set; }


        //
        // Component
        //

        // Drive state from OnEnable and OnDisable? OnEnable on RegisterLodDataInput seems to get called on script reload
        void OnEnable()
        {
            _IsFirstFrameSinceEnabled = true;
            _ViewCameraCached = Viewer;

            if (_Initialized)
            {
                Enable();
                return;
            }

            _Reflections._Water = this;
            _Reflections._UnderWater = _Underwater;
            _Underwater._Water = this;
#if d_CrestPortals
            _Underwater._Portals = _Portals;
            _Portals._Water = this;
            _Portals._UnderWater = _Underwater;
#endif

            _DepthLod._Water = this;
            _LevelLod._Water = this;
            _FlowLod._Water = this;
            _DynamicWavesLod._Water = this;
            _AnimatedWavesLod._Water = this;
            _FoamLod._Water = this;
            _ClipLod._Water = this;
            _AlbedoLod._Water = this;
            _ShadowLod._Water = this;

            // Add simulations to a list for common operations. Order is important.
            Simulations.Clear();
            Simulations.Add(_DepthLod);
            Simulations.Add(_LevelLod);
            Simulations.Add(_FlowLod);
            Simulations.Add(_DynamicWavesLod);
            Simulations.Add(_AnimatedWavesLod);
            Simulations.Add(_FoamLod);
            Simulations.Add(_ClipLod);
            Simulations.Add(_AlbedoLod);
            Simulations.Add(_ShadowLod);

            // Setup a default time provider, and add the override one (from the inspector)
            TimeProviders.Clear();

            // Put a base TP that should always be available as a fallback
            TimeProviders.Push(new DefaultTimeProvider());

            // Add the TP from the inspector
            if (_TimeProvider != null)
            {
                TimeProviders.Push(_TimeProvider);
            }

            if (!VerifyRequirements())
            {
                enabled = false;
                return;
            }

            Container = new()
            {
                name = "Container",
                hideFlags = _Debug._ShowHiddenObjects ? HideFlags.DontSave : HideFlags.HideAndDontSave
            };
            Container.transform.SetParent(transform, worldPositionStays: false);

            Instance = this;
            s_Instance = this;
            Scale = Mathf.Clamp(Scale, _ScaleRange.x, _ScaleRange.y);

            foreach (var simulation in Simulations)
            {
                // Bypasses Enabled and has an internal check.
                simulation.Enable();
            }
            CreateDestroySubSystems();

            // TODO: Have a BufferCount which will be the run-time buffer size or prune data.
            // NOTE: Hardcode minimum (2) to avoid breaking server builds and LodData* toggles.
            // Gather the buffer size for shared data.
            BufferSize = 2;
            foreach (var simulation in Simulations)
            {
                if (!simulation.Enabled) continue;
                BufferSize = Mathf.Max(BufferSize, simulation.BufferCount);
            }


            _PerCascadeInstanceData = new(BufferSize, () => new PerCascadeInstanceData[Lod.k_MaximumSlices]);

            // The extra LOD accounts for reading off the cascade (eg CurrentIndex + LodChange + 1).
            _CascadeData = new(BufferSize, () => new Vector4[Lod.k_MaximumSlices + 1]);

            _ProjectionMatrix = new Matrix4x4[LodLevels];

            // Resolution is 4 tiles across.
            var baseMeshDensity = _Resolution * 0.25f / _GeometryDownSampleFactor;
            // 0.4f is the "best" value when base mesh density is 8. Scaling down from there produces results similar to
            // hand crafted values which looked good when the water is flat.
            _LodAlphaBlackPointFade = 0.4f / (baseMeshDensity / 8f);
            // We could calculate this in the shader, but we can save two subtractions this way.
            _LodAlphaBlackPointWhitePointFade = 1f - _LodAlphaBlackPointFade - _LodAlphaBlackPointFade;

            Root = WaterBuilder.GenerateMesh(this, Chunks, _Resolution, _GeometryDownSampleFactor, _Slices);
            Root.SetParent(Container.transform, worldPositionStays: false);

            if (Application.isPlaying && _Debug._AttachDebugGUI && !TryGetComponent<DebugGUI>(out _))
            {
                gameObject.AddComponent<DebugGUI>().hideFlags = HideFlags.DontSave;
            }

#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
#endif

            _CanSkipCulling = false;

            _GeneratedSettingsHash = CalculateSettingsHash();

            // Prevent MVs from popping on first frame.
            if (!_Debug._DisableFollowViewpoint && _ViewCameraCached != null)
            {
                LateUpdatePosition();
                LateUpdateScale();
            }

            Enable();
            _Initialized = true;

            // Call last so everything is initialized.
            for (var i = s_OnEnable.Count - 1; i >= 0; --i)
            {
                s_OnEnable[i].Invoke(this);
            }
        }

        void OnDisable()
        {
            // Call first so everything is still initialized.
            for (var i = s_OnDisable.Count - 1; i >= 0; --i)
            {
                s_OnDisable[i].Invoke(this);
            }


            // Always clean up in OnDisable during edit mode as OnDestroy is not always called.
            if (_Debug._DestroyResourcesInOnDisable || !Application.isPlaying)
            {
                CleanUp();
#if d_CrestPortals
                _Portals.OnDestroy();
#endif
                _Underwater.OnDestroy();
                _Reflections.OnDestroy();
            }

#if d_CrestPortals
            if (_Portals._Enabled)
            {
                _Portals.OnDisable();
            }
#endif

            if (_Underwater._Enabled)
            {
                _Underwater.OnDisable();
            }

            if (_Reflections._Enabled)
            {
                _Reflections.OnDisable();
            }

            Disable();
        }

        void OnDestroy()
        {
            // Only clean up in OnDestroy when not in edit mode.
            if (_Debug._DestroyResourcesInOnDisable || !Application.isPlaying)
            {
                return;
            }

            CleanUp();

#if d_CrestPortals
            _Portals.OnDestroy();
#endif
            _Underwater.OnDestroy();
            _Reflections.OnDestroy();
        }

        void Update()
        {
            for (var i = s_OnUpdate.Count - 1; i >= 0; --i)
            {
                s_OnUpdate[i].Invoke(this);
            }
        }

        void FixedUpdate()
        {
            for (var i = s_OnFixedUpdate.Count - 1; i >= 0; --i)
            {
                s_OnFixedUpdate[i].Invoke(this);
            }
        }

        void LateUpdate()
        {
#if UNITY_EDITOR
            // Don't run immediately if in edit mode - need to count editor frames so this is run through EditorUpdate()
            if (Application.isPlaying)
#endif
            {
                RunUpdate();
            }

            for (var i = s_OnLateUpdate.Count - 1; i >= 0; --i)
            {
                s_OnLateUpdate[i].Invoke(this);
            }
        }


        //
        // Methods
        //

        void Enable()
        {
            Instance = this;
            s_Instance = this;

            foreach (var simulation in Simulations)
            {
                if (!simulation.Enabled) continue;
                simulation.EnableExternalItems();
            }

#if d_WaveHarmonic_Crest_ShiftingOrigin
            ShiftingOrigin.OnShift -= OnOriginShift;
            ShiftingOrigin.OnShift += OnOriginShift;
#endif

            // This event should not when not using the built-in renderer, but in some cases it can in the editor like
            // when using scene filtering.
            if (RenderPipelineHelper.IsLegacy)
            {
                Camera.onPreCull -= OnPreRenderCamera;
                Camera.onPreCull += OnPreRenderCamera;
                Camera.onPostRender -= OnPostRenderCamera;
                Camera.onPostRender += OnPostRenderCamera;
            }
#if d_UnityURP
            else if (RenderPipelineHelper.IsUniversal)
            {
                ConfigureUniversalRenderer.Enable(this);
            }
#endif

            RenderPipelineManager.activeRenderPipelineTypeChanged -= OnActiveRenderPipelineTypeChanged;
            RenderPipelineManager.activeRenderPipelineTypeChanged += OnActiveRenderPipelineTypeChanged;
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;

#if UNITY_EDITOR
            EnableWaterLevelDepthTexture();
#endif

            Container.SetActive(true);

            if (_Underwater._Enabled)
            {
                _Underwater.OnEnable();
            }

#if d_CrestPortals
            if (_Portals._Enabled)
            {
                _Portals.OnEnable();
            }
#endif

            if (_Reflections._Enabled)
            {
                _Reflections.OnEnable();
            }
        }

        void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
#if d_UnityHDRP
            // Most compatible with lighting options if computed here.
            if (RenderPipelineHelper.IsHighDefinition)
            {
                var lightDirection = Vector3.zero;
                var lightIntensity = Color.black;
                var sun = PrimaryLight;

                if (sun != null && sun.isActiveAndEnabled)
                {
                    lightDirection = -sun.transform.forward;
                    lightIntensity = sun.FinalColor();

                    // Transmittance is for Physically Based Sky.
                    var hdCamera = HDCamera.GetOrCreate(_ViewCameraCached);
                    var settings = SkyManager.GetSkySetting(hdCamera.volumeStack);
                    var transmittance = settings != null
                        ? settings.EvaluateAtmosphericAttenuation(lightDirection, hdCamera.camera.transform.position)
                        : Vector3.one;

                    lightIntensity *= transmittance.x;
                    lightIntensity *= transmittance.y;
                    lightIntensity *= transmittance.z;
                }

                Shader.SetGlobalVector(ShaderIDs.s_PrimaryLightDirection, lightDirection);
                Shader.SetGlobalVector(ShaderIDs.s_PrimaryLightIntensity, lightIntensity);
            }
#endif
        }

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
#if UNITY_EDITOR
            UpdateLastActiveSceneCamera(camera);
#endif

#if d_UnityHDRP
            if (RenderPipelineHelper.IsHighDefinition)
            {
                // Could not get this working without resorting to the following.
                // Graphics code says Time.realtimeSinceStartup but looked more like EditorApplication.timeSinceStartup.
                // _Time did not match HDCamera.time.
                var hd = HDCamera.GetOrCreate(camera);
                // For detecting motion vector pass.
                Shader.SetGlobalFloat(ShaderIDs.s_UnityTime, hd.time);
            }
#endif

            OnBeginCameraRendering(camera);
        }

        void OnBeginCameraRendering(Camera camera)
        {
            if (_Reflections._Enabled)
            {
                _Reflections.OnPreRenderCamera(camera);
            }

            if (!Helpers.MaskIncludesLayer(camera.cullingMask, Layer))
            {
                return;
            }

            var viewpoint = camera.transform.position;
            _SampleHeightHelperPerCamera.Init(viewpoint, allowMultipleCallsPerFrame: true);
            _SampleHeightHelperPerCamera.Sample(this, System.HashCode.Combine(GetHashCode(), camera.GetHashCode()), out var height);
            _ViewerHeightAboveWaterPerCamera = viewpoint.y - height;

            WritePerCameraMaterialParameters();
        }

        void OnActiveRenderPipelineTypeChanged()
        {
            if (isActiveAndEnabled)
            {
                Disable();
                Enable();
            }
        }

        internal void Rebuild()
        {
            CleanUp();
            Disable();
            OnEnable();
        }

        void CreateDestroySubSystems()
        {
            if (!RunningWithoutGraphics)
            {
                if (_FlowLod.Enabled)
                {
                    if (FlowProvider == IFlowProvider.None)
                    {
                        FlowProvider.CleanUp();
                        FlowProvider = null;
                    }
                }
                else
                {
                    if (FlowProvider != IFlowProvider.None)
                    {
                        FlowProvider?.CleanUp();
                        FlowProvider = null;
                    }
                }

                FlowProvider ??= _FlowLod.CreateFlowProvider();
            }

            // Potential extension - add 'type' field to collprovider and change provider if settings have changed - this would support runtime changes.
            CollisionProvider ??= _AnimatedWavesLod.CreateCollisionProvider();
        }

        bool VerifyRequirements()
        {
            if (!RunningWithoutGraphics)
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    Debug.LogError("Crest: Crest does not support WebGL backends.", this);
                    return false;
                }
#if UNITY_EDITOR
                if (SystemInfo.graphicsDeviceType is GraphicsDeviceType.OpenGLES3 or GraphicsDeviceType.OpenGLCore)
                {
                    Debug.LogError("Crest: Crest does not support OpenGL backends.", this);
                    return false;
                }
#endif
                if (SystemInfo.graphicsShaderLevel < 45)
                {
                    Debug.LogError("Crest: Crest requires graphics devices that support shader level 4.5 or above.", this);
                    return false;
                }
                if (!SystemInfo.supportsComputeShaders)
                {
                    Debug.LogError("Crest: Crest requires graphics devices that support compute shaders.", this);
                    return false;
                }
                if (!SystemInfo.supports2DArrayTextures)
                {
                    Debug.LogError("Crest: Crest requires graphics devices that support 2D array textures.", this);
                    return false;
                }
            }

            return true;
        }

        int CalculateSettingsHash()
        {
            var settingsHash = Hash.CreateHash();

            // Add all the settings that require rebuilding..
            Hash.AddInt(_Layer, ref settingsHash);
            Hash.AddInt(_Resolution, ref settingsHash);
            Hash.AddInt(_GeometryDownSampleFactor, ref settingsHash);
            Hash.AddInt(_Slices, ref settingsHash);
            Hash.AddFloat(_ExtentsSizeMultiplier, ref settingsHash);
            Hash.AddBool(CastShadows, ref settingsHash);
            Hash.AddBool(_Debug._ForceBatchMode, ref settingsHash);
            Hash.AddBool(_Debug._ForceNoGraphics, ref settingsHash);
            Hash.AddBool(_Debug._ShowHiddenObjects, ref settingsHash);
#if CREST_DEBUG
            Hash.AddBool(_Debug._DisableSkirt, ref settingsHash);
            Hash.AddBool(_Debug._UniformTiles, ref settingsHash);
#endif
            if (_ChunkTemplate != null)
            {
                Hash.AddObject(_ChunkTemplate, ref settingsHash);
            }

            // Also include anything from the simulations for rebuilding.
            foreach (var simulation in Simulations)
            {
                simulation.AddToHash(ref settingsHash);
            }

            return settingsHash;
        }

        void RunUpdate()
        {
            UnityEngine.Profiling.Profiler.BeginSample("Crest.WaterRenderer.RunUpdate");

            _ViewCameraCached = Viewer;

            // Rebuild if needed. Needs to run in builds (for MVs at the very least).
            if (CalculateSettingsHash() != _GeneratedSettingsHash)
            {
                Rebuild();
            }

            // Set global shader params
            Shader.SetGlobalFloat(ShaderIDs.s_Time, CurrentTime);
            Shader.SetGlobalFloat(ShaderIDs.s_LodCount, LodLevels);
            Shader.SetGlobalFloat(ShaderIDs.s_LodAlphaBlackPointFade, _LodAlphaBlackPointFade);
            Shader.SetGlobalFloat(ShaderIDs.s_LodAlphaBlackPointWhitePointFade, _LodAlphaBlackPointWhitePointFade);

            // LOD 0 is blended in/out when scale changes, to eliminate pops. Here we set it as a global, whereas in WaterChunkRenderer it
            // is applied to LOD0 tiles only through instance data. This global can be used in compute, where we only apply this factor for slice 0.
            var needToBlendOutShape = ScaleCouldIncrease;
            var meshScaleLerp = needToBlendOutShape ? ViewerAltitudeLevelAlpha : 0f;
            Shader.SetGlobalFloat(ShaderIDs.s_MeshScaleLerp, meshScaleLerp);

            // HDRP will automatically disable this pass for the water shader for unknown reasons. It might be that we
            // are sampling from the depth texture does not work with shadow casting.
            if (RenderPipelineHelper.IsHighDefinition && Material != null)
            {
                Material.SetShaderPassEnabled("ShadowCaster", CastShadows);
            }

            if (!_Debug._DisableFollowViewpoint && _ViewCameraCached != null)
            {
                LateUpdatePosition();
                LateUpdateViewerHeight();
                LateUpdateScale();
            }

            // Needs updated transform values like scale.
            WritePerFrameMaterialParams();

            CreateDestroySubSystems();

            if (_ViewCameraCached != null)
            {
                LateUpdateTiles();
            }

            // Reset displacement reporting values.
            {
                MaximumHorizontalDisplacement = MaximumVerticalDisplacement = _MaximumVerticalDisplacementFromWaves = 0f;
            }

            // Construct the command buffer and attach it to the camera so that it will be executed in the render.
#if UNITY_EDITOR
            if (EditorApplication.isPlaying || !_ShowWaterProxyPlane)
#endif
            {
                SimulationBuffer ??= new()
                {
                    name = "Crest.LodData",
                };

                SimulationBuffer.Clear();

                foreach (var simulation in Simulations)
                {
                    if (!simulation.Enabled) continue;
                    simulation.BuildCommandBuffer(this, SimulationBuffer);
                }

                // This will execute at the beginning of the frame before the graphics queue.
                Graphics.ExecuteCommandBuffer(SimulationBuffer);

                LastUpdateFrame = FrameCount;
            }

            // Run queries at end of update. For CollProviderBakedFFT calling this kicks off
            // collision processing job, and the next call to Query() will force a complete, and
            // we don't want that to happen until they've had a chance to run, so schedule them
            // late.
#if UNITY_EDITOR
            // Issue #630 - seems to be a terrible memory leak coming from creating async gpu readbacks. We don't rely on queries in edit mode AFAIK
            // so knock this out.
            // This was marked as resolved by Unity and confirmed fixed by forum posts.
            if (_HeightQueries || EditorApplication.isPlaying)
#endif
            {
                CollisionProvider?.UpdateQueries(this);
                FlowProvider?.UpdateQueries(this);
            }

            if (_Underwater._Enabled)
            {
                _Underwater.LateUpdate();
            }

            if (_Reflections._Enabled)
            {
                _Reflections.LateUpdate();
            }

            _IsFirstFrameSinceEnabled = false;

            UnityEngine.Profiling.Profiler.EndSample();
        }

        void WritePerCameraMaterialParameters()
        {
            if (Material != null)
            {
                // Override isFrontFace when camera is far enough from the water surface to fix self-intersecting waves.
                // Hack - due to SV_IsFrontFace occasionally coming through as true for back faces,
                // add a param here that forces water to be in underwater state. I think the root
                // cause here might be imprecision or numerical issues at water tile boundaries, although
                // i'm not sure why cracks are not visible in this case.
                var height = _ViewerHeightAboveWaterPerCamera;
                var value = (int)_SurfaceSelfIntersectionFixMode;

                switch (_SurfaceSelfIntersectionFixMode)
                {
                    case SurfaceSelfIntersectionFixMode.On:
                        value = height < -2f ? 1 : height > 2f ? 2 : 0;
                        break;
                    case SurfaceSelfIntersectionFixMode.Automatic:
                        // Skip for portals as it is possible to see both sides of the surface at any position.
                        value = Portaled ? 0 : height < -2f ? 1 : height > 2f ? 2 : 0;
                        break;
                }

                Shader.SetGlobalInteger(ShaderIDs.s_ForceUnderwater, value);
            }
        }

        void WritePerFrameMaterialParams()
        {
            _CascadeData.Flip();

            // Update rendering parameters.
            {
                for (var slice = 0; slice < LodLevels; slice++)
                {
                    var scale = CalcLodScale(slice);
                    _CascadeData.Current[slice].x = scale;
                    _CascadeData.Current[slice].y = 1f;
                    _CascadeData.Current[slice].z = MaximumWavelength(slice);

                    _ProjectionMatrix[slice] = Matrix4x4.Ortho(-2f * scale, 2f * scale, -2f * scale, 2f * scale, 1f, k_RenderAboveSeaLevel + k_RenderBelowSeaLevel);
                    if (slice == 0) Shader.SetGlobalFloat(ShaderIDs.s_Scale, scale);
                }

                // Duplicate last element so that things can safely read off the end of the cascades
                _CascadeData.Current[LodLevels] = _CascadeData.Current[LodLevels - 1];
                _CascadeData.Current[LodLevels].y = 0f;
            }

            Shader.SetGlobalVectorArray(ShaderIDs.s_CascadeData, _CascadeData.Current);
            Shader.SetGlobalVectorArray(ShaderIDs.s_CascadeDataSource, _CascadeData.Previous(1));

            _PerCascadeInstanceData.Flip();
            WritePerCascadeInstanceData(_PerCascadeInstanceData);
        }

        void WritePerCascadeInstanceData(BufferedData<PerCascadeInstanceData[]> instanceData)
        {
            for (var lodIdx = 0; lodIdx < LodLevels; lodIdx++)
            {
                // blend LOD 0 shape in/out to avoid pop, if the water might scale up later (it is smaller than its maximum scale)
                var needToBlendOutShape = lodIdx == 0 && ScaleCouldIncrease;
                instanceData.Current[lodIdx]._MeshScaleLerp = needToBlendOutShape ? ViewerAltitudeLevelAlpha : 0f;

                // blend furthest normals scale in/out to avoid pop, if scale could reduce
                var needToBlendOutNormals = lodIdx == LodLevels - 1 && ScaleCouldDecrease;
                instanceData.Current[lodIdx]._FarNormalsWeight = needToBlendOutNormals ? ViewerAltitudeLevelAlpha : 1f;

                // geometry data
                // compute grid size of geometry. take the long way to get there - make sure we land exactly on a power of two
                // and not inherit any of the lossy-ness from lossyScale.
                var scale = CalcLodScale(lodIdx);
                instanceData.Current[lodIdx]._GeometryGridWidth = scale / (0.25f * _Resolution / _GeometryDownSampleFactor);

                var mul = 1.875f; // fudge 1
                var pow = 1.4f; // fudge 2
                var texelWidth = instanceData.Current[lodIdx]._GeometryGridWidth / _GeometryDownSampleFactor;
                instanceData.Current[lodIdx]._NormalScrollSpeeds[0] = Mathf.Pow(Mathf.Log(1f + 2f * texelWidth) * mul, pow);
                instanceData.Current[lodIdx]._NormalScrollSpeeds[1] = Mathf.Pow(Mathf.Log(1f + 4f * texelWidth) * mul, pow);
            }
        }

        void LateUpdatePosition()
        {
            var pos = Viewpoint.position;

            // maintain y coordinate - sea level
            pos.y = Root.position.y;

            // Don't land very close to regular positions where things are likely to snap to, because different tiles might
            // land on either side of a snap boundary due to numerical error and snap to the wrong positions. Nudge away from
            // common by using increments of 1/60 which have lots of factors.
            // :WaterGridPrecisionErrors
            if (Mathf.Abs(pos.x * 60f - Mathf.Round(pos.x * 60f)) < 0.001f)
            {
                pos.x += 0.002f;
            }
            if (Mathf.Abs(pos.z * 60f - Mathf.Round(pos.z * 60f)) < 0.001f)
            {
                pos.z += 0.002f;
            }

            Shader.SetGlobalVector(ShaderIDs.s_CenterDelta, pos - Root.position);

            Root.position = pos;
            Shader.SetGlobalVector(ShaderIDs.s_Center, Root.position);
        }

        void LateUpdateScale()
        {
            var viewerHeight = _ViewerHeightAboveWaterSmooth;

            // Reach maximum detail at slightly below sea level. this should combat cases where visual range can be lost
            // when water height is low and camera is suspended in air. i tried a scheme where it was based on difference
            // to water height but this does help with the problem of horizontal range getting limited at bad times.
            viewerHeight += _MaximumVerticalDisplacementFromWaves * _DropDetailHeightBasedOnWaves;

            var camDistance = Mathf.Abs(viewerHeight);

            // offset level of detail to keep max detail in a band near the surface
            camDistance = Mathf.Max(camDistance - 4f, 0f);

            // scale water mesh based on camera distance to sea level, to keep uniform detail.
            var level = camDistance;
            level = Mathf.Max(level, _ScaleRange.x);
            if (_ScaleRange.y < Mathf.Infinity) level = Mathf.Min(level, 1.99f * _ScaleRange.y);

            var l2 = Mathf.Log(level) / Mathf.Log(2f);
            var l2f = Mathf.Floor(l2);

            ViewerAltitudeLevelAlpha = l2 - l2f;

            var newScale = Mathf.Pow(2f, l2f);

            if (Scale > 0f)
            {
                var ratio = newScale / Scale;
                var ratioL2 = Mathf.Log(ratio) / Mathf.Log(2f);
                ScaleDifferencePower2 = Mathf.RoundToInt(ratioL2);
                Shader.SetGlobalFloat(ShaderIDs.s_LodChange, ScaleDifferencePower2);
                Shader.SetGlobalFloat(ShaderIDs.s_ScaleChange, ratio);
            }

            Scale = newScale;

            Root.localScale = new(Scale, 1f, Scale);
        }

        void LateUpdateViewerHeight()
        {
            _SampleHeightHelper.Init(_ViewCameraCached.transform.position, 0f, true);

            _SampleHeightHelper.Sample(this, out var waterHeight);

            ViewerHeightAboveWater = _ViewCameraCached.transform.position.y - waterHeight;

            // Calculate teleport distance and create window for height queries to return a height change.
            {
                if (_TeleportTimerForHeightQueries > 0f)
                {
                    _TeleportTimerForHeightQueries -= Time.deltaTime;
                }

                var hasTeleported = _IsFirstFrameSinceEnabled;
                if (!_IsFirstFrameSinceEnabled)
                {
                    // Find the distance. Adding the FO offset will exclude FO shifts so we can determine a normal teleport.
                    // FO shifts are visually the same position and it is incorrect to treat it as a normal teleport.
                    var teleportDistanceSqr = (_OldViewerPosition - _ViewCameraCached.transform.position - TeleportOriginThisFrame).sqrMagnitude;
                    // Threshold as sqrMagnitude.
                    var thresholdSqr = _TeleportThreshold * _TeleportThreshold;
                    hasTeleported = teleportDistanceSqr > thresholdSqr;
                }

                if (hasTeleported)
                {
                    // Height queries can take a few frames so a one second window should be plenty.
                    _TeleportTimerForHeightQueries = 1f;
                }

                _HasTeleportedThisFrame = hasTeleported;

                _OldViewerPosition = _ViewCameraCached.transform.position;
            }

            // Smoothly varying version of viewer height to combat sudden changes in water level that are possible
            // when there are local bodies of water
            _ViewerHeightAboveWaterSmooth = _TeleportTimerForHeightQueries > 0f
                ? ViewerHeightAboveWater
                : Mathf.Lerp(_ViewerHeightAboveWaterSmooth, ViewerHeightAboveWater, 0.05f);
        }

        void LateUpdateTiles()
        {
            var canSkipCulling = WaterBody.WaterBodies.Count == 0 && _CanSkipCulling;

            foreach (var tile in Chunks)
            {
                if (tile.Rend == null)
                {
                    continue;
                }

                tile._Culled = false;
                tile.MaterialOverridden = false;

                // If there are local bodies of water, this will do overlap tests between the water tiles
                // and the water bodies and turn off any that don't overlap.
                if (!canSkipCulling)
                {
                    var chunkBounds = tile.Rend.bounds;
                    var chunkUndisplacedBoundsXZ = tile.UnexpandedBoundsXZ;

                    var largestOverlap = 0f;
                    var overlappingOne = false;
                    foreach (var body in WaterBody.WaterBodies)
                    {
                        // If tile has already been excluded from culling, then skip this iteration. But finish this
                        // iteration if the water body has a material override to work out most influential water body.
                        if (overlappingOne && body.AboveSurfaceMaterial == null)
                        {
                            continue;
                        }

                        var bounds = body.AABB;

                        var overlapping =
                            bounds.max.x > chunkBounds.min.x && bounds.min.x < chunkBounds.max.x &&
                            bounds.max.z > chunkBounds.min.z && bounds.min.z < chunkBounds.max.z;
                        if (overlapping)
                        {
                            overlappingOne = true;

                            if (body.AboveSurfaceMaterial != null)
                            {
                                var overlap = 0f;
                                {
                                    // Use the unexpanded bounds to prevent leaking as generally this feature will be
                                    // for an inland body of water where hopefully there is attenuation between it and
                                    // the water to handle the water's displacement. The inland water body will unlikely
                                    // have large displacement but can be mitigated with a decent buffer zone.
                                    var xMin = Mathf.Max(bounds.min.x, chunkUndisplacedBoundsXZ.min.x);
                                    var xMax = Mathf.Min(bounds.max.x, chunkUndisplacedBoundsXZ.max.x);
                                    var zMin = Mathf.Max(bounds.min.z, chunkUndisplacedBoundsXZ.min.y);
                                    var zMax = Mathf.Min(bounds.max.z, chunkUndisplacedBoundsXZ.max.y);
                                    if (xMin < xMax && zMin < zMax)
                                    {
                                        overlap = (xMax - xMin) * (zMax - zMin);
                                    }
                                }

                                // If this water body has the most overlap, then the chunk will get its material.
                                if (overlap > largestOverlap)
                                {
                                    tile.Rend.sharedMaterial = body.AboveSurfaceMaterial;
                                    tile.MaterialOverridden = true;
                                    largestOverlap = overlap;
                                }
                            }
                            else
                            {
                                tile.MaterialOverridden = false;
                            }
                        }
                    }

                    tile._Culled = _WaterBodyCulling && !overlappingOne && WaterBody.WaterBodies.Count > 0;
                }

                tile.Rend.enabled = !tile._Culled;
            }

            // Can skip culling next time around if water body count stays at 0
            _CanSkipCulling = WaterBody.WaterBodies.Count == 0;
        }

        void CleanUp()
        {
            foreach (var simulation in Simulations)
            {
                if (!simulation.Enabled) continue;
                simulation.Disable();
                simulation.Destroy();
            }
            Simulations.Clear();

            // Clean up everything created through the Water Builder.
            WaterBuilder.CleanUp(this);
            Root = null;

            if (Container)
            {
                Helpers.Destroy(Container);
                Container = null;
            }

            if (CollisionProvider != null)
            {
                CollisionProvider.CleanUp();
                CollisionProvider = null;
            }

            if (FlowProvider != null)
            {
                FlowProvider.CleanUp();
                FlowProvider = null;
            }

            Chunks.Clear();

            _Initialized = false;
        }

        void Disable()
        {
            foreach (var simulation in Simulations)
            {
                if (!simulation.Enabled) continue;
                simulation.DisableExternalItems();
            }

            if (RenderPipelineHelper.IsLegacy && Viewer != null)
            {
                // Need to call to prevent crash.
                OnPostRenderCamera(Viewer);
            }

            Camera.onPreCull -= OnPreRenderCamera;
            Camera.onPostRender -= OnPostRenderCamera;

#if d_UnityURP
            ConfigureUniversalRenderer.Disable();
#endif

#if d_WaveHarmonic_Crest_ShiftingOrigin
            ShiftingOrigin.OnShift -= OnOriginShift;
#endif

            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.activeRenderPipelineTypeChanged -= OnActiveRenderPipelineTypeChanged;

#if UNITY_EDITOR
            DisableWaterLevelDepthTexture();
#endif

            if (Container != null)
            {
                Container.SetActive(false);
            }

            Instance = null;
            s_Instance = null;
        }

        /// <summary>
        /// Notify water of origin shift
        /// </summary>
        void OnOriginShift(Vector3 newOrigin)
        {
            foreach (var simulation in Simulations)
            {
                if (!simulation.Enabled) continue;
                simulation.SetOrigin(newOrigin);
            }
        }

        /// <summary>
        /// Clears persistent LOD data. Some simulations have persistent data which can linger for a little while after
        /// being disabled. This will manually clear that data.
        /// </summary>
        void ClearLodData()
        {
            foreach (var simulation in Simulations)
            {
                if (!simulation.Enabled) continue;
                simulation.ClearLodData();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStatics()
        {
            // Init here from 2019.3 onwards
            Instance = null;
            s_Instance = null;
#if UNITY_EDITOR
            s_EditorFramesSinceUpdate = 0;
#endif
        }
    }
}
