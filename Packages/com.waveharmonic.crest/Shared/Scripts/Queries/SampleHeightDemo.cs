// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest.Examples
{
    /// <summary>
    /// Places the game object on the water surface by moving it vertically.
    /// </summary>
    [AddComponentMenu(Constants.k_MenuPrefixSample + "Sample Height Demo")]
    sealed class SampleHeightDemo : ManagedBehaviour<WaterRenderer>
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        readonly SampleHeightHelper _SampleHeightHelper = new();

        protected override System.Action<WaterRenderer> OnUpdateMethod => OnUpdate;
        void OnUpdate(WaterRenderer water)
        {
            // Assume a primitive like a sphere or box.
            var r = transform.lossyScale.magnitude;
            _SampleHeightHelper.Init(transform.position, 2f * r);

            if (_SampleHeightHelper.Sample(water, out var height))
            {
                var pos = transform.position;
                pos.y = height;
                transform.position = pos;
            }
        }
    }
}
