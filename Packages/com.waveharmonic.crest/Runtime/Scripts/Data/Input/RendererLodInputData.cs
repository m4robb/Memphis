// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace WaveHarmonic.Crest
{
    abstract class RendererLodInputData : LodInputData
    {
        [Tooltip("The renderer to use for this input. Can be anything that inherits from <i>Renderer</i> like <i>MeshRenderer</i>, <i>TrailRenderer</i> etc.")]
        [@DecoratedField, SerializeField]
        internal Renderer _Renderer;

        [Tooltip("Forces the renderer to only render into the LOD data and not to render in the scene as it normally would.")]
        [@DecoratedField, SerializeField]
        internal bool _DisableRenderer = true;

        [Tooltip("Set the shader pass manually. By default")]
        [@DecoratedField, SerializeField]
        internal bool _OverrideShaderPass;

        [Tooltip("The shader pass to execute. Set to -1 to execute all passes.")]
        [@Predicated(nameof(_OverrideShaderPass))]
        [@DecoratedField, SerializeField]
        internal int _ShaderPassIndex;

#pragma warning disable 414
        [Tooltip("Check that the shader applied to this object matches the input type (so e.g. an Animated Waves input object has an Animated Waves input shader.")]
        [@DecoratedField, SerializeField]
        internal bool _CheckShaderName = true;

        [Tooltip("Check that the shader applied to this object has only a single pass as only the first pass is executed for most inputs.")]
        [@DecoratedField, SerializeField]
        internal bool _CheckShaderPasses = true;
#pragma warning restore 414


        // Some renderers require multiple materials like particles with trails.
        // We pass this to GetSharedMaterials to avoid allocations.
        internal List<Material> _Materials = new();
        MaterialPropertyBlock _MaterialPropertyBlock;

        internal abstract string ShaderPrefix { get; }

        protected internal override bool IsEnabled => _Renderer != null && _MaterialPropertyBlock != null;
        public override IPropertyWrapper GetProperties(CommandBuffer buffer) => new PropertyWrapperRenderer(_Renderer, _MaterialPropertyBlock);
        public override Vector2 HeightRange => new(_Renderer.bounds.min.y, _Renderer.bounds.max.y);

        public override Rect Rect
        {
            get
            {
                return Rect.MinMaxRect(_Renderer.bounds.min.x, _Renderer.bounds.min.z, _Renderer.bounds.max.x, _Renderer.bounds.max.z);
            }
        }

        bool AnyOtherInputsControllingRenderer(Renderer renderer)
        {
            for (var index = 0; index < SceneManager.sceneCount; index++)
            {
                var scene = SceneManager.GetSceneAt(index);

                if (!scene.isLoaded)
                {
                    continue;
                }

                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    foreach (var component in rootGameObject.GetComponentsInChildren<LodInput>())
                    {
                        if (component == _Input)
                        {
                            continue;
                        }

                        if (component.Data is not RendererLodInputData data)
                        {
                            continue;
                        }

                        if (component.isActiveAndEnabled && data._DisableRenderer && data._Renderer == renderer)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected internal override void OnEnable()
        {
            _MaterialPropertyBlock ??= new();

            if (_Renderer == null)
            {
                return;
            }

            _Renderer.GetSharedMaterials(_Materials);

            if (_DisableRenderer)
            {
                // If we disable using "enabled" then the renderer might not behave correctly (eg line/trail positions
                // will not be updated). This keeps the scripting side of the component running and just disables the
                // rendering. Similar to disabling the Renderer module on the Particle System. It also is not serialized.
                _Renderer.forceRenderingOff = true;
            }
        }

        protected internal override void OnDisable()
        {
            if (_Renderer != null && _DisableRenderer && !AnyOtherInputsControllingRenderer(_Renderer))
            {
                _Renderer.forceRenderingOff = false;
            }
        }

        protected internal override void OnUpdate()
        {
            if (_Renderer == null)
            {
                return;
            }

            // We have to check this every time as the user could change the materials and it is too difficult to track.
            // Doing this in LateUpdate could add one frame latency to receiving the change.
            _Renderer.GetSharedMaterials(_Materials);
        }

        protected internal override void Draw(Lod lod, Component component, CommandBuffer buffer, RenderTexture target, int slice)
        {
            // NOTE: Inputs will only change the first material (only ShapeWaves at the moment).

            for (var i = 0; i < _Materials.Count; i++)
            {
                var material = _Materials[i];
                Debug.AssertFormat(material != null, _Renderer, "Crest: Attached renderer has an empty material slot which is not allowed.");

#if UNITY_EDITOR
                // Empty material slots is a user error, but skip so we do not spam errors.
                if (material == null)
                {
                    continue;
                }
#endif

                var pass = _ShaderPassIndex;
                if (ShapeWaves.s_RenderPassOverride > -1)
                {
                    // Needs to use a second pass to disable blending.
                    pass = ShapeWaves.s_RenderPassOverride;
                }
                else if (!_OverrideShaderPass)
                {
                    // BIRP/URP SG first pass is the right one.
                    pass = 0;

                    // Support HDRP SG. It will always have more than one pass.
                    if (RenderPipelineHelper.IsHighDefinition && material.shader.passCount > 1)
                    {
                        var sgPass = material.FindPass("ForwardOnly");
                        if (sgPass > -1) pass = sgPass;
                    }
                }
                else if (_ShaderPassIndex > material.shader.passCount - 1)
                {
                    return;
                }

                // By default, shaderPass is -1 which is all passes. Shader Graph will produce multi-pass shaders
                // for depth etc so we should only render one pass. Unlit SG will have the unlit pass first.
                // Submesh count generally must equal number of materials.
                buffer.DrawRenderer(_Renderer, material, submeshIndex: i, pass);
            }
        }

#if UNITY_EDITOR
        [@OnChange]
        protected internal override void OnChange(string propertyPath, object previousValue)
        {
            switch (propertyPath)
            {
                case nameof(_Renderer):
                    var renderer = (Renderer)previousValue;
                    if (renderer != null && _DisableRenderer && !AnyOtherInputsControllingRenderer(renderer))
                    {
                        // Turn off if there are no other inputs have set this value.
                        renderer.forceRenderingOff = false;
                    }

                    if (_Renderer != null)
                    {
                        _Renderer.forceRenderingOff = true;
                    }
                    break;
                case nameof(_DisableRenderer):
                    if (_Renderer != null && !AnyOtherInputsControllingRenderer(_Renderer))
                    {
                        _Renderer.forceRenderingOff = _DisableRenderer;
                    }
                    break;
            }
        }

        public override bool InferMode(Component component, ref LodInputMode mode)
        {
            if (component.TryGetComponent(out _Renderer))
            {
                mode = LodInputMode.Renderer;
                return true;
            }

            return false;
        }
#endif
    }
}
