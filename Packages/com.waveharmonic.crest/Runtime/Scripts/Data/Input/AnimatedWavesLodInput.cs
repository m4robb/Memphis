// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Registers a custom input to the wave shape. Attach this GameObjects that you want to render into the displacmeent textures to affect water shape.
    /// </summary>
    [@HelpURL("Manual/Waves.html#animated-waves-inputs")]
    sealed partial class AnimatedWavesLodInput : LodInput
        , AnimatedWavesLod.IShapeUpdatable
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414


        [@Space(10)]

        [Tooltip("Render into all LODs of the simulation after the combine step rather than before with filtering. If enabled, it will also affect dynamic waves.")]
        [@DecoratedField, SerializeField]
        bool _RenderPostCombine = true;

        [Tooltip("Whether to filter this input by wavelength. If disabled it will render to all LODs.")]
        [@Predicated(nameof(_RenderPostCombine), inverted: true)]
        [DecoratedField, SerializeField]
        bool _FilterByWavelength;

        [Tooltip("Which octave to render into, for example set this to 2 to use render into the 2m-4m octave. These refer to the same octaves as the wave spectrum editor.")]
        [@Predicated(nameof(_RenderPostCombine), inverted: true)]
        [@Predicated(nameof(_FilterByWavelength))]
        [@DecoratedField, SerializeField]
        float _OctaveWavelength = 512f;


        [Header("Culling")]

        [Tooltip("Inform water how much this input will displace the water surface vertically. This is used to set bounding box heights for the water tiles.")]
        [SerializeField]
        float _MaximumDisplacementVertical = 0f;

        [Tooltip("Inform water how much this input will displace the water surface horizontally. This is used to set bounding box widths for the water tiles.")]
        [SerializeField]
        float _MaximumDisplacementHorizontal = 0f;

        [Tooltip("Use the bounding box of an attached renderer component to determine the max vertical displacement.")]
        [@Predicated(nameof(_Mode), inverted: true, nameof(LodInputMode.Renderer))]
        [@DecoratedField, SerializeField]
        bool _ReportRendererBounds = false;


        internal override LodInputMode DefaultMode => LodInputMode.Renderer;
        public override int Pass => _RenderPostCombine ? AnimatedWavesLod.k_PassPostCombine : AnimatedWavesLod.k_PassPreCombine;

        public AnimatedWavesLodInput() : base()
        {
            _FollowHorizontalWaveMotion = true;
        }

        protected override void Attach()
        {
            base.Attach();
            AnimatedWavesLod.Attach(this);
        }

        protected override void Detach()
        {
            base.Detach();
            AnimatedWavesLod.Detach(this);
        }

        public override float Filter(WaterRenderer water, int slice)
        {
            return AnimatedWavesLod.FilterByWavelength(water, slice, _FilterByWavelength ? _OctaveWavelength : 0f);
        }

        public void UpdateShape(WaterRenderer water, UnityEngine.Rendering.CommandBuffer buf)
        {
            var maxDispVert = _MaximumDisplacementVertical;

            // let water system know how far from the sea level this shape may displace the surface
            if (_ReportRendererBounds)
            {
                var range = Data.HeightRange;
                var minY = range.x;
                var maxY = range.y;
                var seaLevel = water.SeaLevel;
                maxDispVert = Mathf.Max(maxDispVert, Mathf.Abs(seaLevel - minY), Mathf.Abs(seaLevel - maxY));
            }

            if (_MaximumDisplacementHorizontal > 0f || maxDispVert > 0f)
            {
                water.ReportMaximumDisplacement(_MaximumDisplacementHorizontal, maxDispVert, 0f);
            }
        }
    }
}
