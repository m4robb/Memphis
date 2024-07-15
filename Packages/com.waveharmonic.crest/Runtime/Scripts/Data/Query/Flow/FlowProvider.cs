// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Interface for an object that returns water surface displacement and height.
    /// </summary>
    public interface IFlowProvider
    {
        internal static NoneProvider None { get; } = new();

        /// <summary>
        /// Gives a stationary water (no horizontal flow).
        /// </summary>
        internal sealed class NoneProvider : IFlowProvider
        {
            public int Query(int _0, float _1, Vector3[] _2, Vector3[] result)
            {
                if (result != null) System.Array.Clear(result, 0, result.Length);
                return 0;
            }

            public bool RetrieveSucceeded(int _) => true;
            public void UpdateQueries(WaterRenderer _) { }
            public void CleanUp() { }
        }

        /// <summary>
        /// Query water flow data (horizontal motion) at a set of points.
        /// </summary>
        /// <param name="ownerHash">Unique ID for calling code. Typically acquired by calling GetHashCode().</param>
        /// <param name="minimumSpatialLength">The min spatial length of the object, such as the width of a boat. Useful for filtering out detail when not needed. Set to 0 to get full available detail.</param>
        /// <param name="queryPoints">The world space points that will be queried.</param>
        /// <param name="resultFlows">Water surface flow velocities at the query positions.</param>
        int Query(int ownerHash, float minimumSpatialLength, Vector3[] queryPoints, Vector3[] resultFlows);

        /// <summary>
        /// Check if query results could be retrieved successfully using return code from Query() function
        /// </summary>
        bool RetrieveSucceeded(int queryStatus);

        /// <summary>
        /// Per frame update callback
        /// </summary>
        void UpdateQueries(WaterRenderer water);

        /// <summary>
        /// On destroy, to cleanup resources
        /// </summary>
        void CleanUp();
    }
}
