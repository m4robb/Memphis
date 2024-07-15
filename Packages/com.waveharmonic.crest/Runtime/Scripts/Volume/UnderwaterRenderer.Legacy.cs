// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Rendering;

namespace WaveHarmonic.Crest
{
    partial class UnderwaterRenderer
    {
        bool _HasMaskCommandBuffersBeenRegistered;
        bool _HasEffectCommandBuffersBeenRegistered;

        void OnEnableLegacy()
        {
            SetupMask();
            OnEnableMask();
            SetupUnderwaterEffect();

            Camera.onPreCull -= OnBeforeCulling;
            Camera.onPreCull += OnBeforeCulling;
            Camera.onPreRender -= OnBeforeRender;
            Camera.onPreRender += OnBeforeRender;
            Camera.onPostRender -= OnAfterRender;
            Camera.onPostRender += OnAfterRender;
            RenderPipelineManager.activeRenderPipelineTypeChanged -= OnDisableLegacy;
            RenderPipelineManager.activeRenderPipelineTypeChanged += OnDisableLegacy;
        }

        void OnDisableLegacy()
        {
            Camera.onPreCull -= OnBeforeCulling;
            Camera.onPreRender -= OnBeforeRender;
            Camera.onPostRender -= OnAfterRender;
            RenderPipelineManager.activeRenderPipelineTypeChanged -= OnDisableLegacy;

            OnDisableMask();
        }

        internal void LateUpdate()
        {
            if (!Active)
            {
                return;
            }

            if (!RenderPipelineHelper.IsLegacy)
            {
                return;
            }

            Helpers.SetGlobalKeyword("CREST_UNDERWATER_BEFORE_TRANSPARENT", _EnableShaderAPI);
        }

        void OnBeforeRender(Camera camera)
        {
            XRHelpers.Update(camera);
            XRHelpers.SetInverseViewProjectionMatrix(camera);

            if (ShouldRender(camera, Pass.Mask))
            {
                // It could be either one event.
                camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, _MaskCommandBuffer);
                camera.AddCommandBuffer(CameraEvent.BeforeDepthTexture, _MaskCommandBuffer);
                OnPreRenderMask(camera);
                _HasMaskCommandBuffersBeenRegistered = true;
            }

            if (ShouldRender(camera, Pass.Effect))
            {
                var @event = _EnableShaderAPI ? CameraEvent.BeforeForwardAlpha : CameraEvent.AfterForwardAlpha;
                camera.AddCommandBuffer(@event, _EffectCommandBuffer);
                OnPreRenderUnderwaterEffect(camera);
                _HasEffectCommandBuffersBeenRegistered = true;
            }

            _FirstRender = false;
        }

        void OnAfterRender(Camera camera)
        {
            if (_HasMaskCommandBuffersBeenRegistered)
            {
                // It could be either one event.
                camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, _MaskCommandBuffer);
                camera.RemoveCommandBuffer(CameraEvent.BeforeDepthTexture, _MaskCommandBuffer);
                _MaskCommandBuffer?.Clear();
            }

            if (_HasEffectCommandBuffersBeenRegistered)
            {
                var @event = _EnableShaderAPI ? CameraEvent.BeforeForwardAlpha : CameraEvent.AfterForwardAlpha;
                camera.RemoveCommandBuffer(@event, _EffectCommandBuffer);
                _EffectCommandBuffer?.Clear();
            }

            _HasMaskCommandBuffersBeenRegistered = false;
            _HasEffectCommandBuffersBeenRegistered = false;
        }
    }
}
