// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Rendering;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Removes water using the clip simulation.
    /// </summary>
    [AddComponentMenu(Constants.k_MenuPrefixInputs + "Watertight Hull")]
    [@HelpURL("Manual/Clipping.html#watertight-hull")]
    sealed partial class WatertightHull : ManagedBehaviour<WaterRenderer>, ILodInput
#if UNITY_EDITOR
        , Editor.IOptionalClipLod
#endif
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [@Label("Convex Hull")]
        [Tooltip("The convex hull to keep water out.")]
        [@DecoratedField, SerializeField]
        internal Mesh _Mesh;

        [Tooltip("Inverts the effect to remove clipping (ie add water).")]
        [@DecoratedField, SerializeField]
        bool _Inverted;

        [Tooltip("Order this input will render. Queue is 'Queue + SiblingIndex'")]
        [@DecoratedField, SerializeField]
        int _Queue;

        [@Space(10)]

        [@DecoratedField, SerializeField]
        internal DebugFields _Debug = new();

        [System.Serializable]
        internal sealed class DebugFields
        {
            [@DecoratedField, SerializeField]
            public bool _DrawBounds;
        }

        Material _Material;

        public bool Enabled => enabled && _Mesh != null;
        public bool IsCompute => false;
        public int Queue => _Queue;
        public int Pass => -1;
        public MonoBehaviour Component => this;
        public Rect Rect => transform.TranformBounds(_Mesh.bounds).RectXZ();

        static class ShaderIDs
        {
            public static int s_Inverted = Shader.PropertyToID("_Crest_Inverted");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _Material = new(WaterResources.Instance.Shaders._ClipConvexHull);
            ILodInput.Attach(this, ClipLod.s_Inputs);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Helpers.Destroy(_Material);
            ILodInput.Detach(this, ClipLod.s_Inputs);
        }

        public void Draw(Lod simulation, CommandBuffer buffer, RenderTexture target, int pass = -1, float weight = 1, int slice = -1)
        {
            _Material.SetBoolean(ShaderIDs.s_Inverted, _Inverted);
            buffer.DrawMesh(_Mesh, transform.localToWorldMatrix, _Material);
        }

        public float Filter(WaterRenderer water, int slice)
        {
            return 1f;
        }
    }
}
