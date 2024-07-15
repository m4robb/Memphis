// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// This time provider fixes the water time at a custom value which is usable for testing/debugging.
    /// </summary>
    [AddComponentMenu(Constants.k_MenuPrefixTime + "Custom Time Provider")]
    [@HelpURL("Manual/TimeProviders.html#supporting-pause")]
    sealed class CustomTimeProvider : TimeProvider
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        /// <summary>
        /// Freezes the time
        /// </summary>
        [Tooltip("Freeze progression of time. Only works properly in Play mode.")]
        [SerializeField]
        bool _Paused = false;

        [Tooltip("Override time used for water simulation to value below.")]
        [SerializeField]
        bool _OverrideTime = false;

        [@Predicated(nameof(_OverrideTime))]
        [@DecoratedField, SerializeField]
        float _Time = 0f;

        [Tooltip("Override delta time used for water simulation to value below. This in particular affects dynamic elements of the simulation like the foam simulation and the ripple simulation.")]
        [SerializeField]
        bool _OverrideDeltaTime = false;

        [@Predicated(nameof(_OverrideDeltaTime))]
        [@DecoratedField, SerializeField]
        float _DeltaTime = 0f;


        readonly DefaultTimeProvider _DefaultTimeProvider = new();
        float _TimeInternal = 0f;

        protected override void OnStart()
        {
            base.OnStart();

            // May as well start on the same time value as unity
            _TimeInternal = UnityEngine.Time.time;
        }

        void Update()
        {
            // Use default TP delta time to update our time, because this dt works
            // well in edit mode
            if (!_Paused)
            {
                _TimeInternal += _DefaultTimeProvider.Delta;
            }
        }

        public override float Time
        {
            get
            {
                // Override means override
                if (_OverrideTime)
                {
                    return _Time;
                }

                // In edit mode, update is seldom called, so rely on the default TP
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying && !_Paused)
                {
                    return _DefaultTimeProvider.Time;
                }
#endif

                // Otherwise use our accumulated time
                return _TimeInternal;
            }
        }

        // Either use override, or the default TP which works in edit mode
        public override float Delta => _OverrideDeltaTime ? _DeltaTime : _DefaultTimeProvider.Delta;
    }
}
