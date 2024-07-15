// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Helper to trace a ray against the water surface, by sampling at a set of points along the ray and interpolating the
    /// intersection location.
    /// </summary>
    sealed class RayTraceHelper
    {
        readonly Vector3[] _QueryPosition;
        readonly Vector3[] _QueryResult;

        readonly float _RayLength;
        readonly float _RayStepSize;

        float _MinimumLength = 0f;

        Rect _Rect;

        /// <summary>
        /// Constructor. The length of the ray and the step size must be given here. The smaller the step size, the greater the accuracy.
        /// </summary>
        public RayTraceHelper(float rayLength, float rayStepSize = 2f)
        {
            _RayLength = rayLength;
            _RayStepSize = rayStepSize;

            Debug.Assert(_RayLength > 0f);
            Debug.Assert(_RayStepSize > 0f);

            var stepCount = Mathf.CeilToInt(_RayLength / _RayStepSize) + 1;

            var maxStepCount = 128;
            if (stepCount > maxStepCount)
            {
                stepCount = maxStepCount;
                _RayStepSize = _RayLength / (stepCount - 1f);
                Debug.LogWarning($"Crest: RayTraceHelper: ray steps exceed maximum ({maxStepCount}), step size increased to {_RayStepSize} to reduce step count.");
            }

            _QueryPosition = new Vector3[stepCount];
            _QueryResult = new Vector3[stepCount];
        }

        /// <summary>
        /// Call this each frame to initialize the trace.
        /// </summary>
        /// <param name="origin">World space position of ray origin</param>
        /// <param name="direction">World space ray direction</param>
        public void Init(Vector3 origin, Vector3 direction)
        {
            for (var i = 0; i < _QueryPosition.Length; i++)
            {
                _QueryPosition[i] = origin + i * _RayStepSize * direction;
            }

            _Rect = new()
            {
                xMin = Mathf.Min(_QueryPosition[0].x, _QueryPosition[^1].x),
                yMin = Mathf.Min(_QueryPosition[0].z, _QueryPosition[^1].z),
                xMax = Mathf.Max(_QueryPosition[0].x, _QueryPosition[^1].x),
                yMax = Mathf.Max(_QueryPosition[0].z, _QueryPosition[^1].z),
            };

            // Waves go max double along min length. Thats too much - only allow half of a wave per step.
            _MinimumLength = _RayStepSize * 4f;
        }

        /// <summary>
        /// Call this once each frame to do the query, after calling Init().
        /// </summary>
        /// <param name="water">The water renderer.</param>
        /// <param name="distance">The distance along the ray to the first intersection with the water surface.</param>
        /// <returns>True if the results have come back from the GPU, and if the ray intersects the water surface.</returns>
        public bool Trace(WaterRenderer water, out float distance)
        {
            distance = -1f;

            var status = water.CollisionProvider.Query(GetHashCode(), _MinimumLength, _QueryPosition, _QueryResult, null, null);

            if (!water.CollisionProvider.RetrieveSucceeded(status))
            {
                return false;
            }

            // Now that data is available, compare the height of the water to the height of each point of the ray. If
            // the ray crosses the surface, the distance to the intersection is interpolated from the heights.
            for (var i = 1; i < _QueryPosition.Length; i++)
            {
                var height0 = _QueryResult[i - 1].y + water.SeaLevel - _QueryPosition[i - 1].y;
                var height1 = _QueryResult[i].y + water.SeaLevel - _QueryPosition[i].y;

                if (Mathf.Sign(height0) != Mathf.Sign(height1))
                {
                    var prop = Mathf.Abs(height0) / (Mathf.Abs(height0) + Mathf.Abs(height1));
                    distance = (i - 1 + prop) * _RayStepSize;
                    break;
                }
            }

            return distance >= 0f;
        }
    }
}
