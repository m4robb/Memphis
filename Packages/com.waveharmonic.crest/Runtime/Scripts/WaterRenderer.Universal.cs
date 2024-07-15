// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

#if d_UnityURP

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WaveHarmonic.Crest
{
    // Universal Render Pipeline
    partial class WaterRenderer
    {
        class ConfigureUniversalRenderer : ScriptableRenderPass
        {
            readonly WaterRenderer _Water;
            public static ConfigureUniversalRenderer Instance { get; set; }

            public ConfigureUniversalRenderer(WaterRenderer water)
            {
                _Water = water;
                renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
                ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
            }

            public static void Enable(WaterRenderer water)
            {
                Instance = new ConfigureUniversalRenderer(water);
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
                RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            }

            public static void Disable()
            {
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            }

            static void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
            {
                if (!Helpers.MaskIncludesLayer(camera.cullingMask, Instance._Water.Layer))
                {
                    return;
                }

                // TODO: Could also check RenderType. Which is better?
                if (!Instance._Water.Material.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"))
                {
                    return;
                }

                camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(Instance);
            }

#if UNITY_2023_3_OR_NEWER
            class PassData { }

            public override void RecordRenderGraph(UnityEngine.Rendering.RenderGraphModule.RenderGraph graph, ContextContainer frame)
            {
                using (var builder = graph.AddUnsafePass<PassData>("Crest Register Color/Depth Requirements.", out var data))
                {
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc<PassData>((data, context) => { });
                }
            }

            [System.Obsolete]
#endif
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                // Blank
            }
        }
    }
}

#endif
