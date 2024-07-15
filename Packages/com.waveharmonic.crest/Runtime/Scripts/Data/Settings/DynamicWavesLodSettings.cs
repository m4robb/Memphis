// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    [CreateAssetMenu(fileName = "DynamicWavesSettings", menuName = "Crest/Simulation Settings/Dynamic Waves")]
    [@HelpURL("Manual/Waves.html#dynamic-waves-settings")]
    sealed class DynamicWavesLodSettings : LodSettings
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414


        [Header("Range")]

        [Tooltip("NOT CURRENTLY WORKING. The wave sim will not run if the simulation grid is smaller in resolution than this size. Useful to limit sim range for performance.")]
        [@Range(0f, 32f)]
        [SerializeField, HideInInspector]
        internal float _MinimumGridSize = 0f;

        [Tooltip("NOT CURRENTLY WORKING. The wave sim will not run if the simulation grid is bigger in resolution than this size. Zero means no constraint/unlimited resolutions. Useful to limit sim range for performance.")]
        [@Range(0f, 32f)]
        [SerializeField, HideInInspector]
        internal float _MaximumGridSize = 0f;


        [Header("Simulation")]

        [Tooltip("How much energy is dissipated each frame. Helps sim stability, but limits how far ripples will propagate. Set this as large as possible/acceptable. Default is 0.05.")]
        [@Range(0f, 1f)]
        [SerializeField]
        internal float _Damping = 0.05f;

        [Tooltip("Stability control. Lower values means more stable sim, but may slow down some dynamic waves. This value should be set as large as possible until sim instabilities/flickering begin to appear. Default is 0.7.")]
        [@Range(0.1f, 1f)]
        [SerializeField]
        internal float _CourantNumber = 0.7f;


        [Header("Displacement Generation")]

        [Tooltip("Induce horizontal displacements to sharpen simulated waves.")]
        [@Range(0f, 20f)]
        [SerializeField]
        internal float _HorizontalDisplace = 3f;

        [Tooltip("Clamp displacement to help prevent self-intersection in steep waves. Zero means unclamped.")]
        [@Range(0f, 1f)]
        [SerializeField]
        internal float _DisplaceClamp = 0.3f;


        [Tooltip("Multiplier for gravity. More gravity means dynamic waves will travel faster.")]
        [@Range(0f, 64f)]
        [SerializeField]
        internal float _GravityMultiplier = 1f;
    }
}
