// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
#if !d_CrestPortals
    namespace Portals
    {
        // Dummy script to keep serializer from complaining.
        [System.Serializable]
        sealed class PortalRenderer { }
    }
#endif

    partial class WaterRenderer
    {
        internal const float k_MaximumWindSpeedKPH = 150f;

        [@Space(1, isAlwaysVisible: true)]

        [@Group("General", Group.Style.Accordian)]

        [Tooltip("The camera which drives the water data. Defaults to main camera.")]
        [@DecoratedField, SerializeField]
        Camera _Camera;

        [Tooltip("Optional provider for time, can be used to hard-code time for automation, or provide server time. Defaults to local Unity time.")]
        [@DecoratedField, SerializeField]
        internal TimeProvider _TimeProvider;


        [@Group("Environment", Group.Style.Accordian)]

        [Tooltip("Base wind speed in km/h. Controls wave conditions. Can be overridden on Shape* components.")]
        [@Range(0, k_MaximumWindSpeedKPH, scale: 2f)]
        [SerializeField]
        internal float _WindSpeed = 10f;

        [Tooltip("Multiplier for physics gravity.")]
        [@Range(0f, 10f)]
        [SerializeField]
        float _GravityMultiplier = 1f;

        [Tooltip("The primary directional light. Defaults to RenderSettings.sun.")]
        [@DecoratedField, SerializeField]
        Light _PrimaryLight;


        [@Group("Surface Renderer", Group.Style.Accordian)]

        [Tooltip("The water tile renderers will have this layer.")]
        [@Layer]
        [SerializeField]
        int _Layer = 4; // Water

        [Tooltip("Material to use for the water surface")]
        [@AttachMaterialEditor]
        [@MaterialField("Crest/Water", name: "Water", title: "Create Water Material")]
        [SerializeField]
        internal Material _Material = null;

        [Tooltip("Underwater will copy from this material if set. Useful for overriding properties for the underwater effect. To see what properties can be overriden, see the disabled properties on the underwater material. This does not affect the surface.")]
        [@AttachMaterialEditor]
        [@MaterialField("Crest/Water", name: "Water (Below)", title: "Create Water Material", parent: "_Material")]
        [SerializeField]
        internal Material _VolumeMaterial = null;

        [Tooltip("Use prefab for water chunks. The only requirements are that the prefab must contain a MeshRenderer at the root and not a MeshFilter or WaterChunkRenderer. MR values will be overwritten where necessary and the prefabs are linked in edit mode.")]
        [@PrefabField(title: "Create Chunk Prefab", name: "Water Chunk")]
        [SerializeField]
        internal GameObject _ChunkTemplate;

        [Tooltip("Have the water surface cast shadows for albedo (both foam and custom).")]
        [@Predicated(RenderPipeline.Legacy, inverted: true, hide: true)]
        [@DecoratedField, SerializeField]
        internal bool _CastShadows;

        [@Heading("Culling")]

        [Tooltip("Whether 'Water Body' components will cull the water tiles. Disable if you want to use the 'Material Override' feature and still have an ocean.")]
        [@DecoratedField, SerializeField]
        bool _WaterBodyCulling = true;

        [@Heading("Advanced")]

        [Tooltip("How to handle self-intersections of the water surface caused by choppy waves which can cause a flipped underwater effect. When not using the portals/volumes, this fix is only applied when within 2 metres of the water surface. Automatic will disable the fix if portals/volumes are used which is the recommend setting.")]
        [@DecoratedField, SerializeField]
        SurfaceSelfIntersectionFixMode _SurfaceSelfIntersectionFixMode = SurfaceSelfIntersectionFixMode.Automatic;


        [@Group("Level of Detail", Group.Style.Accordian)]

        [@Label("Scale")]
        [Tooltip("The scale the water can be (infinity for no maximum). Water is scaled horizontally with viewer height, to keep the meshing suitable for elevated viewpoints. This sets the minimum and maximum the water will be scaled. Low minimum values give lots of detail, but will limit the horizontal extents of the water detail. Increasing the minimum value can be a great performance saving for mobile as it will reduce draw calls.")]
        [@Range(0.25f, 256f, Range.Clamp.Minimum, delayed: false)]
        [SerializeField]
        Vector2 _ScaleRange = new(4f, 256f);

        [Tooltip("Drops the height for maximum water detail based on waves. This means if there are big waves, max detail level is reached at a lower height, which can help visual range when there are very large waves and camera is at sea level.")]
        [@Range(0f, 1f)]
        [SerializeField]
        float _DropDetailHeightBasedOnWaves = 0.2f;

        [@Label("Levels")]
        [Tooltip("Number of levels of details (chunks, scales etc) to generate. The horizontal range of the water surface doubles for each added LOD, while GPU processing time increases linearly. The higher the number, the further out detail will be. Furthermore, the higher the count, the more larger wavelengths can be filtering in queries.")]
        [@Range(2, Lod.k_MaximumSlices)]
        [SerializeField]
        int _Slices = 7;

        [@Label("Resolution")]
        [Tooltip("The resolution of the various water LOD data including mesh density, displacement textures, foam data, dynamic wave simulation, etc. Sets the 'detail' present in the water - larger values give more detail at increased run-time expense. This value can be overriden per LOD in their respective settings except for Animated Waves which is tied to this value.")]
        [@Range(80, 1024, Range.Clamp.Minimum, step: 16, delayed: true)]
        [SerializeField]
        int _Resolution = 384;

        [Tooltip("How much of the water shape gets tessellated by geometry. For example, if set to four, every geometry quad will span 4x4 LOD data texels. a value of 2 will generate one vert per 2x2 LOD data texels. A value of 1 means a vert is generated for every LOD data texel. Larger values give lower fidelity surface shape with higher performance.")]
        [@Delayed]
        [SerializeField]
        internal int _GeometryDownSampleFactor = 2;

        [Tooltip("Applied to the extents' far vertices to make them larger. Increase if the extents do not reach the horizon or you see the underwater effect at the horizon.")]
        [@Delayed]
        [SerializeField]
        internal float _ExtentsSizeMultiplier = 100f;

        [@Heading("Center of Detail")]

        [Tooltip("The viewpoint which drives the water detail. Defaults to the camera.")]
        [@DecoratedField, SerializeField]
        Transform _Viewpoint;

        [Tooltip("The height where detail is focused is smoothed to avoid popping which is undesireable after a teleport. Threshold is in Unity units.")]
        [@DecoratedField, SerializeField]
        float _TeleportThreshold = 10f;


        [@Group("Simulations", Group.Style.Accordian)]

        [@Label("Animated Waves")]
        [Tooltip("All waves (including Dynamic Waves) are written to this simulation.")]
        [@DecoratedField, SerializeReference]
        internal AnimatedWavesLod _AnimatedWavesLod = new();

        [@Label("Water Depth")]
        [Tooltip("Water depth information used for shallow water, shoreline foam, wave attenuation, among others.")]
        [@DecoratedField, SerializeReference]
        internal DepthLod _DepthLod = new();

        [@Label("Water Level")]
        [Tooltip("Varying water level to support water bodies at different heights and rivers to run down slopes.")]
        [@DecoratedField, SerializeReference]
        internal LevelLod _LevelLod = new();

        [@Label("Foam")]
        [Tooltip("Simulation of foam created in choppy water and dissipating over time.")]
        [@DecoratedField, SerializeReference]
        internal FoamLod _FoamLod = new();

        [@Label("Dynamic Waves")]
        [Tooltip("Dynamic waves generated from interactions with objects such as boats.")]
        [@DecoratedField, SerializeReference]
        internal DynamicWavesLod _DynamicWavesLod = new();

        [@Label("Flow")]
        [Tooltip("Horizontal motion of water body, akin to water currents.")]
        [@DecoratedField, SerializeReference]
        internal FlowLod _FlowLod = new();

        [@Label("Shadows")]
        [Tooltip("Shadow information used for lighting water.")]
        [@DecoratedField, SerializeReference]
        internal ShadowLod _ShadowLod = new();

        [@Label("Surface Clipping")]
        [Tooltip("Clip surface information for clipping the water surface.")]
        [@DecoratedField, SerializeReference]
        internal ClipLod _ClipLod = new();

        [@Label("Albedo / Decals")]
        [Tooltip("Albedo - a colour layer composited onto the water surface.")]
        [@DecoratedField, SerializeReference]
        internal AlbedoLod _AlbedoLod = new();


        [@Group(isCustomFoldout: true)]

        [@DecoratedField(isCustomFoldout: true), SerializeReference]
        internal WaterReflections _Reflections = new();


        [@Group(isCustomFoldout: true)]

        [@DecoratedField(isCustomFoldout: true), SerializeReference]
        internal UnderwaterRenderer _Underwater = new();


#if !d_CrestPortals
        // Hide if package is not present. Fallback to dummy script.
        [HideInInspector]
#endif

        [@Group(isCustomFoldout: true)]

        [@DecoratedField(isCustomFoldout: true), SerializeReference]
        internal Portals.PortalRenderer _Portals = new();


        [@Group("Edit Mode", Group.Style.Accordian)]

#pragma warning disable 414
        [@DecoratedField, SerializeField]
        internal bool _ShowWaterProxyPlane;

        [Tooltip("Sets the update rate of the water system when in edit mode. Can be reduced to save power.")]
        [@Range(0f, 120f, Range.Clamp.Minimum)]
        [SerializeField]
        float _EditModeFrameRate = 30f;

        [Tooltip("Move water with Scene view camera if Scene window is focused.")]
        [@Predicated(nameof(_ShowWaterProxyPlane), true)]
        [@DecoratedField, SerializeField]
        internal bool _FollowSceneCamera = true;

        [Tooltip("Whether height queries are enabled in edit mode.")]
        [@DecoratedField, SerializeField]
        bool _HeightQueries = true;
#pragma warning restore 414


        [@Group("Debug", isCustomFoldout: true)]

        [@DecoratedField(isCustomFoldout: true), SerializeField]
        internal DebugFields _Debug = new();

        [System.Serializable]
        internal sealed class DebugFields
        {
            [@Space(10)]

            [Tooltip("Attach debug GUI that adds some controls and allows to visualize the water data.")]
            [@DecoratedField, SerializeField]
            public bool _AttachDebugGUI;

            [Tooltip("Show hidden objects like water chunks in the hierarchy.")]
            [@DecoratedField, SerializeField]
            public bool _ShowHiddenObjects;

#if !CREST_DEBUG
            [HideInInspector]
#endif
            [Tooltip("Water will not move with viewpoint.")]
            [@DecoratedField, SerializeField]
            public bool _DisableFollowViewpoint;

            [Tooltip("Resources are normally released in OnDestroy (except in edit mode) which avoids expensive rebuilds when toggling this component. This option moves it to OnDisable. If you need this active then please report to us.")]
            [@DecoratedField, SerializeField]
            public bool _DestroyResourcesInOnDisable;

#if CREST_DEBUG
            [Tooltip("Whether to generate water geometry tiles uniformly (with overlaps).")]
            [@DecoratedField, SerializeField]
            public bool _UniformTiles;

            [Tooltip("Disable generating a wide strip of triangles at the outer edge to extend water to edge of view frustum.")]
            [@DecoratedField, SerializeField]
            public bool _DisableSkirt;

            [@DecoratedField, SerializeField]
            public bool _DrawLodOutline;

            [@DecoratedField, SerializeField]
            public bool _ShowDebugInformation;
#endif

            [@Heading("Server")]

            [Tooltip("Emulate batch mode which models running without a display (but with a GPU available). Equivalent to running standalone build with -batchmode argument.")]
            [@DecoratedField, SerializeField]
            public bool _ForceBatchMode;

            [Tooltip("Emulate running on a client without a GPU. Equivalent to running standalone with -nographics argument.")]
            [@DecoratedField, SerializeField]
            public bool _ForceNoGraphics;
        }

        [SerializeField, HideInInspector]
        internal WaterResources _Resources;
    }
}
