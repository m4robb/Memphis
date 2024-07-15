// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using WaveHarmonic.Crest.Editor;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Registers a custom input to the flow data. Attach this GameObjects that you want to influence the horizontal flow of the water volume.
    /// </summary>
    [@HelpURL("Manual/TidesAndCurrents.html#flow-inputs")]
    [FilterEnum(nameof(_Blend), Filtered.Mode.Include, (int)Blend.Additive, (int)Blend.Minimum, (int)Blend.Maximum, (int)Blend.Alpha)]
    sealed partial class FlowLodInput : LodInput
#if UNITY_EDITOR
        , IOptionalFlowLod
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
