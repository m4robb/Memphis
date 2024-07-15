// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest
{
    interface ILodInput
    {
        const int k_QueueMaximumSubIndex = 1000;

        /// <summary>
        /// Draw the input (the render target will be bound)
        /// </summary>
        public void Draw(Lod simulation, CommandBuffer buffer, RenderTexture target, int pass = -1, float weight = 1f, int slice = -1);

        float Filter(WaterRenderer water, int slice);

        /// <summary>
        /// Whether to apply this input.
        /// </summary>
        bool Enabled { get; }

        bool IsCompute { get; }

        int Queue { get; }

        int Pass { get; }

        Rect Rect { get; }

        MonoBehaviour Component { get; }

        // Allow sorting within a queue. Callers can pass in things like sibling index to
        // get deterministic sorting.
        int Order => Queue * k_QueueMaximumSubIndex + Mathf.Min(Component.transform.GetSiblingIndex(), k_QueueMaximumSubIndex - 1);

        internal static void Attach(ILodInput input, Utility.SortedList<int, ILodInput> inputs)
        {
            inputs.Remove(input);
            inputs.Add(input.Order, input);
        }

        internal static void Detach(ILodInput input, Utility.SortedList<int, ILodInput> inputs)
        {
            inputs.Remove(input);
        }
    }

    /// <summary>
    /// Base class for scripts that register input to the various LOD data types.
    /// </summary>
    [@ExecuteDuringEditMode]
    [@HelpURL("Manual/WaterInputs.html")]
    abstract partial class LodInput : ManagedBehaviour<WaterRenderer>, ILodInput
    {
        [@Filtered((int)LodInputMode.Unset)]
        [SerializeField]
        internal LodInputMode _Mode = LodInputMode.Unset;

        // NOTE:
        // Weight and Feather do not support Depth and Clip as they do not make much sense.
        // For others it is a case of only supporting unsupported mode(s).

        [Tooltip("Scales the input.")]
        [@Predicated(typeof(AlbedoLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(AnimatedWavesLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(ClipLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(DepthLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(DynamicWavesLodInput), inverted: true, hide: true)]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Renderer))]
        [@Range(0f, 1f)]
        [SerializeField]
        internal float _Weight = 1f;

        [Tooltip("Order this input will render. Queue is <i>Queue + SiblingIndex</i>")]
        [@DecoratedField, SerializeField]
        protected int _Queue;

        [Tooltip("Similar to blend operations in shaders, how this input will blend into the data. For inputs which have materials, use its blend functionality.")]
        [@Predicated(typeof(AlbedoLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(AnimatedWavesLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(ClipLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(DepthLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(DynamicWavesLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(ShadowLodInput), inverted: true, hide: true)]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Global))]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Paint))]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Primitive))]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Renderer))]
        [@Filtered]
        [SerializeField]
        protected internal Blend _Blend = Blend.Additive;

        [@Label("Feather")]
        [@Predicated(typeof(AlbedoLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(AnimatedWavesLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(ClipLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(DepthLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(DynamicWavesLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(LevelLodInput), inverted: true, hide: true)]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Renderer))]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Global))]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Primitive))]
        [@DecoratedField, SerializeField]
        protected float _FeatherWidth = 0.1f;

        [Tooltip("If false, data will not move from side to side with the waves. Has a small performance overhead when disabled. Only suitable for inputs of small size.")]
        [@Predicated(typeof(ClipLodInput), inverted: true, hide: true)]
        [@Predicated(typeof(ShapeWaves), inverted: true, hide: true)]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Global))]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Spline))]
        [@DecoratedField, SerializeField]
        protected bool _FollowHorizontalWaveMotion = false;

        [@Heading("Mode")]

        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Unset), hide: true)]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Primitive), hide: true)]
        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Global), hide: true)]
        [@Stripped]
        [SerializeReference]
        private protected LodInputData _Data;

        // Need always visble for space to appear before foldout instead of inside.
        [@Space(10, isAlwaysVisible: true)]

        [@Group("Debug", order = k_DebugGroupOrder)]

        [@Predicated(nameof(_Mode), inverted: false, nameof(LodInputMode.Global))]
        [@DecoratedField, SerializeField]
        internal bool _DrawBounds;

        internal const int k_DebugGroupOrder = 10;

        public static class ShaderIDs
        {
            public static int s_Weight = Shader.PropertyToID("_Crest_Weight");
            public static int s_DisplacementAtInputPosition = Shader.PropertyToID("_Crest_DisplacementAtInputPosition");
        }


        public abstract Color GizmoColor { get; }
        internal abstract LodInputMode DefaultMode { get; }
        private protected abstract Utility.SortedList<int, ILodInput> Inputs { get; }

        public LodInputMode Mode => _Mode;
        internal LodInputData Data { get => _Data; set => _Data = value; }
        internal Blend Blend { get => _Blend; set => _Blend = value; }

        public int Queue => _Queue;
        public float FeatherWidth => _FeatherWidth;

        public bool IsCompute => Mode is LodInputMode.Texture or LodInputMode.Paint or LodInputMode.Global or LodInputMode.Primitive;
        public virtual int Pass => -1;
        public virtual Rect Rect
        {
            get
            {
                var rect = Rect.zero;
                if (_Data != null)
                {
                    rect = _Data.Rect;
                    rect.center -= _Displacement.XZ();
                }
                return rect;
            }
        }
        public MonoBehaviour Component => this;

        readonly SampleHeightHelper _SampleHeightHelper = new();
        Vector3 _Displacement;

        public virtual bool Enabled => enabled && Mode switch
        {
            LodInputMode.Unset => false,
            _ => Data?.IsEnabled ?? false,
        };

        // By default do not follow horizontal motion of waves. This means that the water input will appear on the surface at its XZ location, instead
        // of moving horizontally with the waves.
        protected virtual bool FollowHorizontalMotion => Mode == LodInputMode.Spline || _FollowHorizontalWaveMotion;


        //
        // Event Methods
        //

        protected override void OnEnable()
        {
            base.OnEnable();
            Data?.OnEnable();
            Attach();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Detach();
            Data?.OnDisable();
        }

        protected override Action<WaterRenderer> OnLateUpdateMethod => OnLateUpdate;
        protected virtual void OnLateUpdate(WaterRenderer water)
        {
            if (!FollowHorizontalMotion)
            {
                // allowMultipleCallsPerFrame because we are calling before the time values are updated.
                _SampleHeightHelper.Init(transform.position, minimumSpatialLength: 0f, allowMultipleCallsPerFrame: true, context: this);
                _SampleHeightHelper.Sample(water, out _Displacement, out _, out _);
            }
            else
            {
                _Displacement = Vector3.zero;
            }

            Data?.OnUpdate();
        }


        //
        // ILodInput
        //

        protected virtual void Attach()
        {
            ILodInput.Attach(this, Inputs);
        }

        protected virtual void Detach()
        {
            ILodInput.Detach(this, Inputs);
        }

        public virtual void Draw(Lod simulation, CommandBuffer buffer, RenderTexture target, int pass = -1, float weight = 1f, int slice = -1)
        {
            if (weight == 0f)
            {
                return;
            }

            // Must use global as weight can change per slice for ShapeWaves.
            var wrapper = new PropertyWrapperBuffer(buffer);
            wrapper.SetFloat(ShaderIDs.s_Weight, weight * _Weight);
            wrapper.SetVector(ShaderIDs.s_DisplacementAtInputPosition, _Displacement);

            Data?.Draw(simulation, this, buffer, target, slice);
        }

        public virtual float Filter(WaterRenderer water, int slice)
        {
            return 1f;
        }


        //
        // Editor Only Methods
        //

#if UNITY_EDITOR
        [@OnChange]
        void OnChange(string propertyPath, object previousValue)
        {
            switch (propertyPath)
            {
                case nameof(_Queue):
                    Attach();
                    break;
                case nameof(_Mode):
                    OnDisable();
                    SetMode(Mode);
                    UnityEditor.EditorTools.ToolManager.RefreshAvailableTools();
                    OnEnable();
                    break;
                case nameof(_Blend):
                    Data.OnChange($"../{propertyPath}", previousValue);
                    break;
            }
        }

        internal void SetMode(LodInputMode mode)
        {
            _Data = null;

            // Try to infer the mode.
            var types = TypeCache.GetTypesWithAttribute<ForLodInput>();
            var self = GetType();
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes<ForLodInput>();
                foreach (var attribute in attributes)
                {
                    if (attribute._Mode != mode) continue;
                    if (!attribute._Type.IsAssignableFrom(self)) continue;
                    _Mode = mode;
                    _Data = (LodInputData)Activator.CreateInstance(type);
                    _Data._Input = this;
                    _Data.InferMode(this, ref _Mode);
                    return;
                }
            }

            _Mode = DefaultMode;
        }

        // Called when component attached in edit mode, or when Reset clicked by user.
        // Besides recovering from Unset default value, also does a nice bit of auto-config.
        protected void Reset()
        {
            var types = TypeCache.GetTypesWithAttribute<ForLodInput>();
            var self = GetType();

            // Use inferred mode.
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes<ForLodInput>();
                foreach (var attribute in attributes)
                {
                    if (!attribute._Type.IsAssignableFrom(self)) continue;

                    var instance = (LodInputData)Activator.CreateInstance(type);
                    instance._Input = this;

                    if (instance.InferMode(this, ref _Mode))
                    {
                        _Data = instance;
                        return;
                    }
                }
            }

            // Use default mode.
            SetMode(DefaultMode);
        }
#endif
    }

    static partial class Extensions
    {
        static I AddComponent<I, D>(this GameObject go, LodInputMode mode)
            where I : LodInput
            where D : LodInputData, new()
        {
            var input = go.AddComponent<I>();
            input._Mode = mode;
            input.Data = new D();
            input.Data._Input = input;
            return input;
        }
    }
}
