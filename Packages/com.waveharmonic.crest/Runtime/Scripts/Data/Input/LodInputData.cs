// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace WaveHarmonic.Crest
{
    [AttributeUsage(AttributeTargets.Class)]
    sealed class ForLodInput : Attribute
    {
        public readonly Type _Type;
        public readonly LodInputMode _Mode;

        public ForLodInput(Type type, LodInputMode mode)
        {
            _Type = type;
            _Mode = mode;
        }
    }

    abstract class LodInputData
    {
        [SerializeField, HideInInspector]
        public LodInput _Input;

        // Can be a material or compute shader.
        public abstract IPropertyWrapper GetProperties(CommandBuffer buffer);

        public abstract Vector2 HeightRange { get; }
        public abstract Rect Rect { get; }

        protected internal abstract bool IsEnabled { get; }
        protected internal abstract void OnEnable();
        protected internal abstract void OnDisable();
        protected internal abstract void OnUpdate();
        protected internal abstract void Draw(Lod lod, Component component, CommandBuffer buffer, RenderTexture target, int slice);

#if UNITY_EDITOR
        protected internal abstract void OnChange(string propertyPath, object previousValue);
        public abstract bool InferMode(Component component, ref LodInputMode mode);
#endif
    }

    enum LodInputMode
    {
        /// <summary>
        /// Unset is serialisation default. Code in Awake() and Reset() then change this based on attached components.
        /// </summary>
        Unset = 0,
        /// <summary>
        /// Data hand-painted by user in editor.
        /// </summary>
        Paint,
        /// <summary>
        /// Driven by a user created spline.
        /// </summary>
        Spline,
        /// <summary>
        /// Attached 'Renderer' (mesh or particle or other) and assigned material used to drive data.
        /// </summary>
        Renderer,
        /// <summary>
        /// Driven by a mathematical primitive such as a cube or sphere.
        /// </summary>
        Primitive,
        /// <summary>
        /// Covers the entire water area.
        /// </summary>
        Global,
        /// <summary>
        /// Same as painted data except user provides a texture.
        /// </summary>
        Texture,
    }
}
