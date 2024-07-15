// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using WaveHarmonic.Crest.Editor;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Registers a custom input to affect the water height.
    /// </summary>
    [@HelpURL("Manual/WaterBodies.html#water-bodies")]
    [@FilterEnum(nameof(_Blend), Filtered.Mode.Include, (int)Blend.None, (int)Blend.Additive, (int)Blend.Minimum, (int)Blend.Maximum)]
    sealed partial class LevelLodInput : LodInput
        , IReportsHeight
#if UNITY_EDITOR
        , IOptionalLevelLod
#endif
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [@Heading("Water Chunk Culling")]

        [Tooltip("Whether to use the manual \"Height Range\" for water chunk culling. Mandatory for non mesh inputs like \"Texture\".")]
        [@DecoratedField, SerializeField]
        bool _OverrideHeight;

        [Tooltip("The minimum and maximum height value to report for water chunk culling.")]
        [@Predicated(nameof(_OverrideHeight))]
        [@Range(-100, 100, Range.Clamp.None)]
        [SerializeField]
        Vector2 _HeightRange = new(-100, 100);


#if d_CrestPaint
        internal override LodInputMode DefaultMode => LodInputMode.Paint;
#else
        internal override LodInputMode DefaultMode => LodInputMode.Renderer;
#endif

        internal Rect _Rect;


        protected override void OnEnable()
        {
            base.OnEnable();
            WaterChunkRenderer.HeightReporters.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            WaterChunkRenderer.HeightReporters.Remove(this);
        }

        public bool ReportHeight(ref Rect bounds, ref float minimum, ref float maximum)
        {
            if (!Enabled)
            {
                return false;
            }

            _Rect = Data.Rect;

            // These modes do not provide a height yet.
            if (_Mode is LodInputMode.Paint or LodInputMode.Texture && !_OverrideHeight)
            {
                return false;
            }

            if (bounds.Overlaps(_Rect, false))
            {
                var range = _OverrideHeight ? _HeightRange : Data.HeightRange;
                minimum = range.x;
                maximum = range.y;
                return true;
            }

            return false;
        }
    }
}
