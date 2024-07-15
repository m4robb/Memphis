// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Emits useful events (UnityEvents) based on the sampled height of the water surface.
    /// </summary>
    [AddComponentMenu(Constants.k_MenuPrefixScripts + "Query Events")]
    [@HelpURL("Manual/Events.html#query-events")]
    sealed class QueryEvents : ManagedBehaviour<WaterRenderer>
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414


        [Tooltip("What transform should the queries be based on. \"Viewer\" will reuse queries already performed by the Water Renderer")]
        [@DecoratedField, SerializeField]
        QuerySource _Source;

        [Tooltip("The higher the value, the more smaller waves will be ignored when sampling the water surface.")]
        [@Predicated(nameof(_Source), inverted: true, nameof(QuerySource.Transform))]
        [@DecoratedField, SerializeField]
        float _MinimumWavelength = 1f;


        [Header("Distance From Water Surface")]

        [Tooltip("A normalised distance from water surface will be between zero and one.")]
        [SerializeField]
        bool _NormaliseDistance = true;

        [Tooltip("The maximum distance passed to function. Always use a real distance value (not a normalised one).")]
        [SerializeField]
        float _MaximumDistance = 100f;

        [Tooltip("Apply a curve to the distance passed to the function.")]
        [SerializeField]
        AnimationCurve _DistanceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);


        [Header("Events")]

        [SerializeField]
        UnityEvent _OnBelowWater = new();

        [SerializeField]
        UnityEvent _OnAboveWater = new();

        [SerializeField]
        UnityEvent<float> _DistanceFromWater = new();

        enum QuerySource
        {
            Transform,
            Viewer
        }

        public UnityEvent OnBelowWater => _OnBelowWater;
        public UnityEvent OnAboveWater => _OnAboveWater;
        public UnityEvent<float> DistanceFromWater => _DistanceFromWater;

        // Store state
        bool _IsAboveSurface = false;
        bool _IsFirstUpdate = true;
        readonly SampleHeightHelper _SampleHeightHelper = new();

        protected override System.Action<WaterRenderer> OnLateUpdateMethod => OnLateUpdate;
        void OnLateUpdate(WaterRenderer water)
        {
            var distance = water.ViewerHeightAboveWater;

            if (_Source == QuerySource.Transform)
            {
                _SampleHeightHelper.Init(transform.position, 2f * _MinimumWavelength);
                if (!_SampleHeightHelper.Sample(water, out var height)) return;
                distance = transform.position.y - height;
            }

            var isAboveSurface = distance > 0;

            // Has the below/above water surface state changed?
            if (_IsAboveSurface != isAboveSurface || _IsFirstUpdate)
            {
                _IsAboveSurface = isAboveSurface;
                _IsFirstUpdate = false;

                if (_IsAboveSurface)
                {
                    _OnAboveWater?.Invoke();
                }
                else
                {
                    _OnBelowWater?.Invoke();
                }
            }

            // Save some processing when not being used.
            if (_DistanceFromWater.GetPersistentEventCount() > 0)
            {
                // Normalise distance so we can use the curve.
                var normalizedDistance = _DistanceCurve.Evaluate(1f - Mathf.Abs(distance) / _MaximumDistance);

                // Restore raw distance if desired.
                if (!_NormaliseDistance)
                {
                    normalizedDistance = _MaximumDistance - normalizedDistance * _MaximumDistance;
                }

                _DistanceFromWater.Invoke(normalizedDistance);
            }
        }
    }
}
