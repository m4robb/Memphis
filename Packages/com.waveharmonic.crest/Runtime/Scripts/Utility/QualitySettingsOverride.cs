// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    [System.Serializable]
    class QualitySettingsOverride
    {
        [@DecoratedField, SerializeField]
        public bool _OverrideLodBias;

        [Tooltip("Overrides the LOD bias for meshes. Highest quality is infinity.")]
        [@Predicated(nameof(_OverrideLodBias))]
        [@DecoratedField, SerializeField]
        public float _LodBias;

        [@DecoratedField, SerializeField]
        public bool _OverrideMaximumLodLevel;

        [Tooltip("Overrides the maximum LOD level. Highest quality is 0.")]
        [@Predicated(nameof(_OverrideMaximumLodLevel))]
        [@DecoratedField, SerializeField]
        public int _MaximumLodLevel;

        [@DecoratedField, SerializeField]
        public bool _OverrideTerrainPixelError;

        [Tooltip("Overrides the pixel error value for terrains. Highest quality is zero.")]
        [@Predicated(nameof(_OverrideTerrainPixelError))]
        [@DecoratedField, SerializeField]
        public float _TerrainPixelError;

        float _OldLodBias;
        int _OldMaximumLodLevelOverride;
        float _OldTerrainPixelError;
        TerrainQualityOverrides _OldTerrainOverrides;

        public void Override()
        {
            if (_OverrideLodBias)
            {
                _OldLodBias = QualitySettings.lodBias;
                QualitySettings.lodBias = _LodBias;
            }

            if (_OverrideMaximumLodLevel)
            {
                _OldMaximumLodLevelOverride = QualitySettings.maximumLODLevel;
                QualitySettings.maximumLODLevel = _MaximumLodLevel;
            }

            if (_OverrideTerrainPixelError)
            {
                _OldTerrainOverrides = QualitySettings.terrainQualityOverrides;
                _OldTerrainPixelError = QualitySettings.terrainPixelError;
                QualitySettings.terrainQualityOverrides = TerrainQualityOverrides.PixelError;
                QualitySettings.terrainPixelError = _TerrainPixelError;
            }
        }

        public void Restore()
        {
            if (_OverrideLodBias)
            {
                QualitySettings.lodBias = _OldLodBias;
            }

            if (_OverrideMaximumLodLevel)
            {
                QualitySettings.maximumLODLevel = _OldMaximumLodLevelOverride;
            }

            if (_OverrideTerrainPixelError)
            {
                QualitySettings.terrainQualityOverrides = _OldTerrainOverrides;
                QualitySettings.terrainPixelError = _OldTerrainPixelError;
            }
        }
    }
}
