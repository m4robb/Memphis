// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Helper to obtain the water surface height at a single location per frame. This is not particularly efficient to sample a single height,
    /// but is a fairly common case.
    /// </summary>
    sealed class SampleHeightHelper
    {
        readonly Vector3[] _QueryPosition = new Vector3[1];
        readonly Vector3[] _QueryResult = new Vector3[1];
        readonly Vector3[] _QueryResultNormal = new Vector3[1];
        readonly Vector3[] _QueryResultVelocity = new Vector3[1];

        float _MinimumLength = 0f;

#if UNITY_EDITOR
        int _LastFrame = -1;
#endif

        /// <summary>
        /// Call this to prime the sampling. The SampleHeightHelper is good for one query per frame - if it is called multiple times in one frame
        /// it will throw a warning. Calls from FixedUpdate are an exception to this - pass true as the last argument to disable the warning.
        /// </summary>
        /// <param name="position">World space position to sample</param>
        /// <param name="minimumSpatialLength">The smallest length scale you are interested in. If you are sampling data for boat physics,
        /// pass in the boats width. Larger objects will ignore small wavelengths.</param>
        /// <param name="allowMultipleCallsPerFrame">Pass true if calling from FixedUpdate(). This will omit a warning when there on multipled-FixedUpdate frames.</param>
        /// <param name="context">The context for warning/error logging.</param>
        public void Init(Vector3 position, float minimumSpatialLength = 0f, bool allowMultipleCallsPerFrame = false, Object context = null)
        {
            _QueryPosition[0] = position;
            _MinimumLength = minimumSpatialLength;

#if UNITY_EDITOR
            if (!allowMultipleCallsPerFrame && _LastFrame >= WaterRenderer.FrameCount)
            {
                Debug.LogWarning($"Crest: SampleHeightHelper.Init() called multiple times in one frame which is not expected. Each SampleHeightHelper object services a single height query per frame. To perform multiple queries, create multiple SampleHeightHelper objects or use the CollProvider.Query() API directly. (_lastFrame = {_LastFrame})", context);
            }
            _LastFrame = WaterRenderer.FrameCount;
#endif
        }

        /// <summary>
        /// Call this to do the query. Can be called only once after Init().
        /// </summary>
        public bool Sample(WaterRenderer water, int id, out float height)
        {
            var collProvider = water.CollisionProvider;
            if (collProvider == null)
            {
                height = 0f;
                return false;
            }

            var status = collProvider.Query(id, _MinimumLength, _QueryPosition, _QueryResult, null, null);

            if (!collProvider.RetrieveSucceeded(status))
            {
                height = water.SeaLevel;
                return false;
            }

            height = _QueryResult[0].y + water.SeaLevel;

            return true;
        }

        public bool Sample(WaterRenderer water, int id, out float height, out Vector3 normal)
        {
            var collProvider = water.CollisionProvider;
            if (collProvider == null)
            {
                height = 0f;
                normal = Vector3.up;
                return false;
            }

            var status = collProvider.Query(id, _MinimumLength, _QueryPosition, _QueryResult, _QueryResultNormal, null);

            if (!collProvider.RetrieveSucceeded(status))
            {
                height = water.SeaLevel;
                normal = Vector3.up;
                return false;
            }

            height = _QueryResult[0].y + water.SeaLevel;
            normal = _QueryResultNormal[0];

            return true;
        }

        public bool Sample(WaterRenderer water, int id, out float height, out Vector3 normal, out Vector3 velocity)
        {
            var collProvider = water.CollisionProvider;
            if (collProvider == null)
            {
                height = 0f;
                normal = Vector3.up;
                velocity = Vector3.zero;
                return false;
            }

            var status = collProvider.Query(id, _MinimumLength, _QueryPosition, _QueryResult, _QueryResultNormal, _QueryResultVelocity);

            if (!collProvider.RetrieveSucceeded(status))
            {
                height = water.SeaLevel;
                normal = Vector3.up;
                velocity = Vector3.zero;
                return false;
            }

            height = _QueryResult[0].y + water.SeaLevel;
            normal = _QueryResultNormal[0];
            velocity = _QueryResultVelocity[0];

            return true;
        }

        public bool Sample(WaterRenderer water, int id, out Vector3 displacementToPoint, out Vector3 normal, out Vector3 velocity)
        {
            var collProvider = water.CollisionProvider;
            if (collProvider == null)
            {
                displacementToPoint = Vector3.zero;
                normal = Vector3.up;
                velocity = Vector3.zero;
                return false;
            }
            var status = collProvider.Query(id, _MinimumLength, _QueryPosition, _QueryResult, _QueryResultNormal, _QueryResultVelocity);

            if (!collProvider.RetrieveSucceeded(status))
            {
                displacementToPoint = Vector3.zero;
                normal = Vector3.up;
                velocity = Vector3.zero;
                return false;
            }

            displacementToPoint = _QueryResult[0];
            normal = _QueryResultNormal[0];
            velocity = _QueryResultVelocity[0];

            return true;
        }

        public bool Sample(WaterRenderer water, out float height) => Sample(water, GetHashCode(), out height);
        public bool Sample(WaterRenderer water, out float height, out Vector3 normal) => Sample(water, GetHashCode(), out height, out normal);
        public bool Sample(WaterRenderer water, out float height, out Vector3 normal, out Vector3 velocity) => Sample(water, GetHashCode(), out height, out normal, out velocity);
        public bool Sample(WaterRenderer water, out Vector3 displacementToPoint, out Vector3 normal, out Vector3 velocity) => Sample(water, GetHashCode(), out displacementToPoint, out normal, out velocity);
    }

    /// <summary>
    /// Helper to obtain the flow data (horizontal water motion) at a single location. This is not particularly efficient to sample a single height,
    /// but is a fairly common case.
    /// </summary>
    sealed class SampleFlowHelper
    {
        readonly Vector3[] _QueryPosition = new Vector3[1];
        readonly Vector3[] _QueryResult = new Vector3[1];

        float _MinimumLength = 0f;

        /// <summary>
        /// Call this to prime the sampling
        /// </summary>
        /// <param name="position">World space position to sample</param>
        /// <param name="minimumSpatialLength">The smallest length scale you are interested in. If you are sampling data for boat physics,
        /// pass in the boats width. Larger objects will filter out detailed flow information.</param>
        public void Init(Vector3 position, float minimumSpatialLength)
        {
            _QueryPosition[0] = position;
            _MinimumLength = minimumSpatialLength;
        }

        /// <summary>
        /// Call this to do the query. Can be called only once after Init().
        /// </summary>
        public bool Sample(WaterRenderer water, out Vector2 flow)
        {
            var flowProvider = water.FlowProvider;
            if (flowProvider == null)
            {
                flow = Vector2.zero;
                return false;
            }
            var status = flowProvider.Query(GetHashCode(), _MinimumLength, _QueryPosition, _QueryResult);

            if (!flowProvider.RetrieveSucceeded(status))
            {
                flow = Vector2.zero;
                return false;
            }

            // We don't support float2 queries unfortunately, so unpack from float3
            flow.x = _QueryResult[0].x;
            flow.y = _QueryResult[0].z;

            return true;
        }
    }
}
