// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace WaveHarmonic.Crest.Editor
{
    abstract class TexturePreview : ObjectPreview
    {
        public static bool AnyActivePreviews { get; private set; }

        UnityEditor.Editor _Editor;
        RenderTexture _RenderTexture;
        Texture _Current;

        protected abstract Texture Texture { get; }

        // Preview complains if not a certain set of formats.
        bool Incompatible => !(GraphicsFormatUtility.IsIEEE754Format(Texture.graphicsFormat)
            || GraphicsFormatUtility.IsNormFormat(Texture.graphicsFormat));

        public TexturePreview() { }

        public override bool HasPreviewGUI()
        {
            AnyActivePreviews = false;
            return Texture;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Object.DestroyImmediate(_Editor);
            Object.DestroyImmediate(_RenderTexture);
        }

        public override void OnPreviewSettings()
        {
            Allocate(Texture);
            _Editor.OnPreviewSettings();
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            AnyActivePreviews = true;

            // This check is in original.
            if (Event.current.type == EventType.Repaint)
            {
                background.Draw(rect, false, false, false, false);
            }

            Allocate(Texture);

            if (_RenderTexture == null && Incompatible && Texture is RenderTexture rt)
            {
                var descriptor = rt.descriptor;
                descriptor.graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat;
                _RenderTexture = new(descriptor);
                Helpers.Blit(rt, _RenderTexture);
                // Recreate editor with new texture with supported format.
                Object.DestroyImmediate(_Editor);
                _Editor = UnityEditor.Editor.CreateEditor(_RenderTexture);
            }
            else
            {
                _Editor.DrawPreview(rect);
            }
        }

        void Allocate(Texture texture)
        {
            // LOD with buffered data like foam will recreate every frame freezing controls.
            if (_Editor != null && _Current == Texture) return;
            _Current = texture;
            Object.DestroyImmediate(_Editor);
            _Editor = UnityEditor.Editor.CreateEditor(texture);
        }
    }
}
