// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using WaveHarmonic.Crest.Editor;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Registers a custom input to the albedo data. Attach this GameObjects that you want to influence the surface colour.
    /// </summary>
    [@HelpURL("Manual/WaterAppearance.html#albedo-inputs")]
    sealed partial class AlbedoLodInput : LodInput
#if UNITY_EDITOR
        , IOptionalAlbedoLod
#endif
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        internal override LodInputMode DefaultMode => LodInputMode.Renderer;
    }
}
