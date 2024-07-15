// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using WaveHarmonic.Crest.Editor;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Registers a custom input for shadow data. Attach this to GameObjects that you want use to override shadows.
    /// </summary>
    [@HelpURL("Manual/WaterAppearance.html#shadows-lod")]
    sealed partial class ShadowLodInput : LodInput
#if UNITY_EDITOR
        , IOptionalShadowLod
#endif
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        internal override LodInputMode DefaultMode => LodInputMode.Renderer;
    }
}
