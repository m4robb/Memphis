// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace WaveHarmonic.Crest.Editor
{
    //
    // Lod
    //

    abstract class LodPreview : TexturePreview
    {
        protected abstract Lod Lod { get; }
        public override GUIContent GetPreviewTitle() => new(Lod.Name);
        protected RenderTexture _TemporaryTexture;
        protected override Texture Texture
        {
            get
            {
                if (!Lod.Enabled || !((WaterRenderer)target).isActiveAndEnabled) return null;

                var texture = Lod.DataTexture;

                if (texture == null)
                {
                    return null;
                }

                if (_TemporaryTexture != null)
                {
                    return _TemporaryTexture;
                }

                return texture;
            }
        }

        public override void OnPreviewSettings()
        {
            base.OnPreviewSettings();
            // OnPreviewSettings is called after OnPreviewGUI so release here.
            RenderTexture.ReleaseTemporary(_TemporaryTexture);
            _TemporaryTexture = null;
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            var texture = Lod.DataTexture;
            var descriptor = texture.descriptor;
            _TemporaryTexture = RenderTexture.GetTemporary(descriptor);
            _TemporaryTexture.name = "Crest Preview (Temporary)";
            Graphics.CopyTexture(texture, _TemporaryTexture);
            ModifyTexture();
            base.OnPreviewGUI(rect, background);
        }


        protected virtual void ModifyTexture()
        {

        }

        public override void Cleanup()
        {
            base.Cleanup();
            RenderTexture.ReleaseTemporary(_TemporaryTexture);
        }

        // FIXME: Without constructor Unity complains:
        // WaveHarmonic.Crest.Editor.LodPreview does not contain a default constructor, it
        // will not be registered as a preview handler. Use the Initialize function to set
        // up your object instead.
        public LodPreview() { }
    }

    [CustomPreview(typeof(WaterRenderer))]
    sealed class AlbedoLodPreview : LodPreview
    {
        protected override Lod Lod => (target as WaterRenderer)._AlbedoLod;
    }

    [CustomPreview(typeof(WaterRenderer))]
    sealed class AnimatedWavesLodPreview : LodPreview
    {
        protected override Lod Lod => (target as WaterRenderer)._AnimatedWavesLod;
        protected override void ModifyTexture()
        {
            // Set alpha to one otherwise it shows nothing when set to RGB.
            var clear = WaterResources.Instance.Compute._Clear;
            if (clear != null)
            {
                clear.SetTexture(0, ShaderIDs.s_Target, _TemporaryTexture);
                clear.SetVector(ShaderIDs.s_ClearMask, Color.black);
                clear.SetVector(ShaderIDs.s_ClearColor, Color.black);
                clear.Dispatch
                (
                    0,
                    Lod.Resolution / Lod.k_ThreadGroupSizeX,
                    Lod.Resolution / Lod.k_ThreadGroupSizeY,
                    Lod.Slices
                );
            }
        }
    }

#if CREST_DEBUG
    [CustomPreview(typeof(WaterRenderer))]
    sealed class AnimatedWavesLodWaveBufferPreview : TexturePreview
    {
        public override GUIContent GetPreviewTitle() => new("Animated Waves: Wave Buffer");
        protected override Texture Texture => (target as WaterRenderer)._AnimatedWavesLod._WaveBuffers;
    }
#endif

    [CustomPreview(typeof(WaterRenderer))]
    sealed class ClipLodPreview : LodPreview
    {
        protected override Lod Lod => (target as WaterRenderer)._ClipLod;
    }

    [CustomPreview(typeof(WaterRenderer))]
    sealed class DepthLodPreview : LodPreview
    {
        protected override Lod Lod => (target as WaterRenderer)._DepthLod;
    }

    [CustomPreview(typeof(WaterRenderer))]
    sealed class DynamicWavesLodPreview : LodPreview
    {
        protected override Lod Lod => (target as WaterRenderer)._DynamicWavesLod;
    }

    [CustomPreview(typeof(WaterRenderer))]
    sealed class FlowLodPreview : LodPreview
    {
        protected override Lod Lod => (target as WaterRenderer)._FlowLod;
    }

    [CustomPreview(typeof(WaterRenderer))]
    sealed class FoamLodPreview : LodPreview
    {
        protected override Lod Lod => (target as WaterRenderer)._FoamLod;
    }

    [CustomPreview(typeof(WaterRenderer))]
    sealed class LevelLodPreview : LodPreview
    {
        protected override Lod Lod => (target as WaterRenderer)._LevelLod;
    }

    [CustomPreview(typeof(WaterRenderer))]
    sealed class ShadowLodPreview : LodPreview
    {
        protected override Lod Lod => (target as WaterRenderer)._ShadowLod;
    }


    //
    // LodInput
    //

    // Adding abstract causes exception:
    // does not contain a default constructor, it will not be registered as a preview
    // handler. Use the Initialize function to set up your object instead.
    class ShapeWavesPreview : TexturePreview
    {
        public override GUIContent GetPreviewTitle() => new($"{target.GetType().Name}: Wave Buffer");
        protected override Texture Texture => (target as ShapeWaves).WaveBuffer;
    }

    [CustomPreview(typeof(ShapeFFT))]
    sealed class ShapeFFTPreview : ShapeWavesPreview
    {
    }

    [CustomPreview(typeof(ShapeGerstner))]
    sealed class ShapeGerstnerPreview : ShapeWavesPreview
    {
    }

    [CustomPreview(typeof(DepthProbe))]
    sealed class DepthProbePreview : TexturePreview
    {
        public override GUIContent GetPreviewTitle() => new("Depth Probe");
        protected override Texture Texture => (target as DepthProbe).Texture;
    }

#if CREST_DEBUG
    [CustomPreview(typeof(DepthProbe))]
    sealed class DepthProbeCameraPreview : TexturePreview
    {
        public override GUIContent GetPreviewTitle() => new("Depth Probe: Camera");
        protected override Texture Texture
        {
            get
            {
                var target = this.target as DepthProbe;
                if (target._Camera == null) return null;
                return target._Camera.targetTexture;
            }
        }
    }
#endif


    //
    // Other
    //

    [CustomPreview(typeof(WaterRenderer))]
    sealed class ReflectionPreview : TexturePreview
    {
        public override GUIContent GetPreviewTitle() => new("Water Reflections");
        protected override Texture Texture => (target as WaterRenderer)._Reflections._Enabled ? (target as WaterRenderer)._Reflections.ReflectionTexture : null;
    }
}
