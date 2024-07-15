// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Demarcates an AABB area where water is present in the world. If present, water tiles will be
    /// culled if they don't overlap any WaterBody.
    /// </summary>
    [@ExecuteDuringEditMode]
    [AddComponentMenu(Constants.k_MenuPrefixScripts + "Water Body")]
    [@HelpURL("Manual/WaterBodies.html")]
    sealed partial class WaterBody : ManagedBehaviour<WaterRenderer>
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [Tooltip("If clipping is enabled and set to clip everywhere by default, this option will register this water body to ensure its area does not get clipped.")]
        [SerializeField]
        bool _Clip = true;

        [Tooltip("Water geometry tiles that overlap this waterbody area will be assigned this material. This is useful for varying water appearance across different water bodies. If no override material is specified, the default material assigned to the WaterRenderer component will be used.")]
        [@AttachMaterialEditor]
        [@MaterialField("Crest/Water", name: "Water", title: "Create Water Material"), SerializeField]
        internal Material _Material = null;

        [Tooltip("Overrides the property on the Water Renderer with the same name when the camera is inside the bounds.")]
        [@AttachMaterialEditor]
        [@MaterialField("Crest/Water", name: "Water (Below)", title: "Create Water Material", parent: nameof(_Material)), SerializeField]
        internal Material _BelowSurfaceMaterial;

        [Tooltip("Overrides the Water Renderer's volume material when the camera is inside the bounds.")]
        [@MaterialField("Crest/Underwater", name: "Underwater", title: "Create Underwater Material")]
        [@AttachMaterialEditor]
        [SerializeField]
        internal Material _VolumeMaterial;

        sealed class ClipInput : ILodInput
        {
            readonly WaterBody _Owner;
            readonly Transform _Transform;

            public bool Enabled => true;
            public bool IsCompute => true;
            public int Pass => -1;

            // TODO: Expose serialized queue.
            public int Queue => 0;
            public MonoBehaviour Component => _Owner;

            public Rect Rect
            {
                get
                {
                    var size = _Transform.lossyScale.XZ();
                    return new(_Transform.position.XZ() - size * 0.5f, size);
                }
            }

            public ClipInput(WaterBody owner)
            {
                _Owner = owner;
                _Transform = owner.transform;
            }

            public void Draw(Lod simulation, CommandBuffer buffer, RenderTexture target, int pass = -1, float weight = 1f, int slices = -1)
            {
                var wrapper = new PropertyWrapperCompute(buffer, WaterResources.Instance.Compute._ClipPrimitive, 0);

                wrapper.SetMatrix(ShaderIDs.s_Matrix, _Transform.worldToLocalMatrix);

                // For culling.
                wrapper.SetVector(ShaderIDs.s_Position, _Transform.position);
                wrapper.SetFloat(ShaderIDs.s_Diameter, _Transform.lossyScale.Maximum());

                wrapper.SetKeyword(WaterResources.Instance.Keywords.ClipPrimitiveInverted, true);
                wrapper.SetKeyword(WaterResources.Instance.Keywords.ClipPrimitiveSphere, false);
                wrapper.SetKeyword(WaterResources.Instance.Keywords.ClipPrimitiveCube, false);
                wrapper.SetKeyword(WaterResources.Instance.Keywords.ClipPrimitiveRectangle, true);

                wrapper.SetTexture(ShaderIDs.s_Target, target);

                var threads = simulation.Resolution / Lod.k_ThreadGroupSize;
                wrapper.Dispatch(threads, threads, slices);
            }

            public float Filter(WaterRenderer water, int slice)
            {
                return 1f;
            }
        }

        internal static List<WaterBody> WaterBodies { get; } = new();

        public Bounds AABB { get; private set; }

        internal Material AboveSurfaceMaterial => _Material;
        internal Material AboveOrBelowSurfaceMaterial => _BelowSurfaceMaterial == null ? _Material : _BelowSurfaceMaterial;
        internal Material VolumeMaterial => _VolumeMaterial;

        ClipInput _ClipInput;

        protected override void OnEnable()
        {
            base.OnEnable();

            CalculateBounds();

            WaterBodies.Add(this);

            // Needs to execute after the Water Renderer as Update is stripped from builds.
            HandleClipInputRegistration();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            WaterBodies.Remove(this);

            if (_ClipInput != null)
            {
                ILodInput.Detach(_ClipInput, ClipLod.s_Inputs);

                _ClipInput = null;
            }
        }

        internal void CalculateBounds()
        {
            var bounds = new Bounds();
            bounds.center = transform.position;
            bounds.Encapsulate(transform.TransformPoint(Vector3.right / 2f + Vector3.forward / 2f));
            bounds.Encapsulate(transform.TransformPoint(Vector3.right / 2f - Vector3.forward / 2f));
            bounds.Encapsulate(transform.TransformPoint(-Vector3.right / 2f + Vector3.forward / 2f));
            bounds.Encapsulate(transform.TransformPoint(-Vector3.right / 2f - Vector3.forward / 2f));

            AABB = bounds;
        }

        void HandleClipInputRegistration()
        {
            var registered = _ClipInput != null;
            var water = WaterRenderer.Instance;
            var shouldBeRegistered = _Clip && water && water._ClipLod.Enabled
                && water._ClipLod._DefaultClippingState == ClipLod.DefaultClippingState.EverythingClipped;

            if (registered != shouldBeRegistered)
            {
                if (shouldBeRegistered)
                {
                    _ClipInput = new(this);

                    ILodInput.Attach(_ClipInput, ClipLod.s_Inputs);
                }
                else
                {
                    ILodInput.Detach(_ClipInput, ClipLod.s_Inputs);

                    _ClipInput = null;
                }
            }
        }

#if UNITY_EDITOR
        protected override System.Action<WaterRenderer> OnUpdateMethod => OnUpdate;
        void OnUpdate(WaterRenderer water)
        {
            HandleClipInputRegistration();
        }
#endif
    }
}
