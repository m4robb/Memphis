// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

// NOTE: DWP2 depends on this file. Any API changes need to be communicated to the DWP2 authors in advance.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Interface for an object that returns water surface displacement and height.
    /// </summary>
    public interface ICollisionProvider
    {
        internal static NoneProvider None { get; } = new();

        /// <summary>
        /// Gives a flat, still water.
        /// </summary>
        internal sealed class NoneProvider : ICollisionProvider
        {
            public int Query(int _0, float _1, Vector3[] _2, Vector3[] result0, Vector3[] result1, Vector3[] result2)
            {
                if (result0 != null) System.Array.Clear(result0, 0, result0.Length);
                if (result1 != null) System.Array.Clear(result1, 0, result1.Length);
                if (result2 != null) System.Array.Clear(result2, 0, result2.Length);
                return 0;
            }

            public int Query(int _0, float _1, Vector3[] _2, float[] result0, Vector3[] result1, Vector3[] result2)
            {
                if (result0 != null) System.Array.Clear(result0, 0, result0.Length);
                if (result1 != null) System.Array.Clear(result1, 0, result1.Length);
                if (result2 != null) System.Array.Clear(result2, 0, result2.Length);
                return 0;
            }

            public bool RetrieveSucceeded(int _) => true;
            public void UpdateQueries(WaterRenderer _) { }
            public void CleanUp() { }
        }

        /// <summary>
        /// Query water physical data at a set of points. Pass in null to any out parameters that are not required.
        /// </summary>
        /// <param name="ownerHash">Unique ID for calling code. Typically acquired by calling GetHashCode().</param>
        /// <param name="minimumSpatialLength">The min spatial length of the object, such as the width of a boat. Useful for filtering out detail when not needed. Set to 0 to get full available detail.</param>
        /// <param name="queryPoints">The world space points that will be queried.</param>
        /// <param name="resultHeights">Float array of water heights at the query positions. Pass null if this information is not required.</param>
        /// <param name="resultNormals">Water normals at the query positions. Pass null if this information is not required.</param>
        /// <param name="resultVelocities">Water surface velocities at the query positions. Pass null if this information is not required.</param>
        int Query(int ownerHash, float minimumSpatialLength, Vector3[] queryPoints, float[] resultHeights, Vector3[] resultNormals, Vector3[] resultVelocities);

        /// <summary>
        /// Query water physical data at a set of points. Pass in null to any out parameters that are not required.
        /// </summary>
        /// <param name="ownerHash">Unique ID for calling code. Typically acquired by calling GetHashCode().</param>
        /// <param name="minimumSpatialLength">The min spatial length of the object, such as the width of a boat. Useful for filtering out detail when not needed. Set to 0 to get full available detail.</param>
        /// <param name="queryPoints">The world space points that will be queried.</param>
        /// <param name="resultDisplacements">Displacement vectors for water surface points that will displace to the XZ coordinates of the query points. Water heights are given by sea level plus the y component of the displacement.</param>
        /// <param name="resultNormals">Water normals at the query positions. Pass null if this information is not required.</param>
        /// <param name="resultVelocities">Water surface velocities at the query positions. Pass null if this information is not required.</param>
        int Query(int ownerHash, float minimumSpatialLength, Vector3[] queryPoints, Vector3[] resultDisplacements, Vector3[] resultNormals, Vector3[] resultVelocities);

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
