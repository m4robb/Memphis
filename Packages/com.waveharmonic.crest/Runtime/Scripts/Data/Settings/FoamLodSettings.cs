// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    [CreateAssetMenu(fileName = "FoamSettings", menuName = "Crest/Simulation Settings/Foam")]
    [@HelpURL("Manual/WaterAppearance.html#foam-settings")]
    sealed class FoamLodSettings : LodSettings
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414


        [Header("General settings")]

        [Tooltip("Foam will not exceed this value in the simulation which can be used to prevent foam from accumulating too much.")]
        [Min(0f)]
        [SerializeField]
        float _Maximum = 10f;

        [Tooltip("How quickly foam dissipates. Low values mean foam remains on surface for longer. This setting should be balanced with the generation *strength* parameters below.")]
        [@Range(0f, 20f)]
        [SerializeField]
        internal float _FoamFadeRate = 0.8f;

        [Header("Whitecaps")]

        [Tooltip("Scales intensity of foam generated from waves. This setting should be balanced with the Foam Fade Rate setting.")]
        [@Range(0f, 5f)]
        [SerializeField]
        internal float _WaveFoamStrength = 1f;

        [Tooltip("How much of the waves generate foam. Higher values will lower the threshold for foam generation, giving a larger area.")]
        [@Range(0f, 1f)]
        [SerializeField]
        internal float _WaveFoamCoverage = 0.55f;

        [Tooltip("The minimum LOD  to sample waves from. Zero means all waves and increasing will exclude lower wavelengths which can help with too much foam near the camera.")]
        [@Range(0, Lod.k_MaximumSlices - 2)]
        [SerializeField]
        internal int _FilterWaves = 2;


        [Header("Shoreline")]

        [Tooltip("Foam will be generated in water shallower than this depth. Controls how wide the band of foam at the shoreline will be. Note that this is not a distance to shoreline, but a threshold on water depth, so the width of the foam band can vary based on terrain slope. To address this limitation we allow foam to be manually added from geometry or from a texture, see the next section.")]
        [@Range(0.01f, 3f)]
        [SerializeField]
        internal float _ShorelineFoamMaximumDepth = 0.65f;

        [Tooltip("Scales intensity of foam generated in shallow water. This setting should be balanced with the Foam Fade Rate setting.")]
        [@Range(0f, 5f)]
        [SerializeField]
        internal float _ShorelineFoamStrength = 2f;

        [Tooltip("Primes foam when terrain height is this value above water. This ignores other foam settings and writes a constant foam value.")]
        [@Range(0f, 5f, Range.Clamp.Minimum)]
        [SerializeField]
        internal float _ShorelineFoamPriming = 5f;


        public int FilterWaves => _FilterWaves;
        public float Maximum => _Maximum;
    }
}
