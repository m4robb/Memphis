// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using WaveHarmonic.Crest.Editor;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Registers a custom input to the foam simulation. Attach this GameObjects that you want to influence the foam simulation, such as depositing foam on the surface.
    /// </summary>
    [@HelpURL("Manual/WaterAppearance.html#foam-inputs")]
    [@FilterEnum(nameof(_Blend), Filtered.Mode.Include, (int)Blend.Additive, (int)Blend.Maximum)]
    sealed partial class FoamLodInput : LodInput
#if UNITY_EDITOR
        , IOptionalFoamLod
#endif
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

#if d_CrestPaint
        internal override LodInputMode DefaultMode => LodInputMode.Paint;
#else
        internal override LodInputMode DefaultMode => LodInputMode.Renderer;
#endif
    }
}
