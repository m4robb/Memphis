// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

#if d_UnityHDRP

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace WaveHarmonic.Crest
{
    sealed class UnderwaterMaskPassHDRP : CustomPass
    {
        const string k_Name = "Underwater Mask";

        static UnderwaterRenderer s_Renderer;
        static UnderwaterMaskPass s_UnderwaterMaskPass;
        static UnderwaterMaskPassHDRP s_Instance;
        static GameObject s_GameObject;

        public static void Enable(UnderwaterRenderer renderer)
        {
            CustomPassHelpers.CreateOrUpdate
            (
                ref s_GameObject,
                parent: renderer._Water.Container.transform,
                k_Name,
                hide: !renderer._Water._Debug._ShowHiddenObjects
            );

            CustomPassHelpers.CreateOrUpdate
            (
                s_GameObject,
                ref s_Instance,
                k_Name,
                CustomPassInjectionPoint.BeforeRendering
            );

            s_Renderer = renderer;
            s_UnderwaterMaskPass = new(renderer);
        }

        public static void Disable()
        {
            // It should be safe to rely on this reference for this reference to fail.
            if (s_GameObject != null)
            {
                // Will also trigger Cleanup below.
                s_GameObject.SetActive(false);
            }
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            s_UnderwaterMaskPass.Allocate();
        }

        protected override void Cleanup()
        {
            s_UnderwaterMaskPass?.Release();
        }

        protected override void Execute(CustomPassContext context)
        {
            var camera = context.hdCamera.camera;

            if (!s_Renderer.ShouldRender(camera, UnderwaterRenderer.Pass.Mask))
            {
                return;
            }

            s_UnderwaterMaskPass.Execute(camera, context.cmd);
        }
    }
}

#endif // d_UnityHDRP
