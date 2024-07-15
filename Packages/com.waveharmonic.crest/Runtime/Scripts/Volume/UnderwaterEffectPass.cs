// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace WaveHarmonic.Crest
{
    sealed class UnderwaterEffectPass
    {
        readonly UnderwaterRenderer _Renderer;

        RTHandle _ColorTexture;

        bool _FirstRender = true;

        public UnderwaterEffectPass(UnderwaterRenderer renderer)
        {
            _Renderer = renderer;
        }

        public void Allocate(GraphicsFormat format)
        {
            // TODO: There may other settings we want to set or bring in. Not MSAA since this is a resolved texture.
            _ColorTexture = RTHandles.Alloc
            (
                Vector2.one,
                TextureXR.slices,
                dimension: TextureXR.dimension,
                colorFormat: format,
                depthBufferBits: DepthBits.None,
                useDynamicScale: true,
                wrapMode: TextureWrapMode.Clamp,
                name: "_Crest_UnderwaterCameraColorTexture"
            );
        }

        public void ReAllocate(RenderTextureDescriptor descriptor)
        {
            // Descriptor will not have MSAA bound.
            RenderPipelineCompatibilityHelper.ReAllocateIfNeeded(ref _ColorTexture, descriptor, name: "_Crest_UnderwaterCameraColorTexture");
        }

        public void Release()
        {
            _ColorTexture?.Release();
            _ColorTexture = null;
        }

        public void Execute(Camera camera, CommandBuffer buffer, RTHandle color, RTHandle depth, MaterialPropertyBlock mpb = null)
        {
            _Renderer.UpdateEffectMaterial(camera, _FirstRender);

            // Copy color texture.
            Blitter.BlitCameraTexture(buffer, color, _ColorTexture);
            buffer.SetGlobalTexture(UnderwaterRenderer.ShaderIDs.s_CameraColorTexture, _ColorTexture);

            CoreUtils.SetRenderTarget(buffer, color, depth, ClearFlag.None);
            _Renderer.ExecuteEffect(camera, buffer, mpb);

            _FirstRender = false;
        }
    }
}
