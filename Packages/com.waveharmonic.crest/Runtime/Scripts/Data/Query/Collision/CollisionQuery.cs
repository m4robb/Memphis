// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Samples water surface shape - displacement, height, normal, velocity.
    /// </summary>
    sealed class CollisionQuery : QueryBase, ICollisionProvider
    {
        static class ShaderIDs
        {
            public static readonly int s_ResultDisplacements = Shader.PropertyToID("_Crest_ResultDisplacements");
        }

        public CollisionQuery(WaterRenderer water) : base(water)
        {
        }

        protected override ComputeShader QueriesCompute => WaterResources.Instance.Compute._QueryDisplacements;
        protected override string QueryKernelName => "CrestExecute";

        protected override void BindInputsAndOutputs(PropertyWrapperComputeStandalone wrapper, ComputeBuffer resultsBuffer)
        {
            ShaderProcessQueries.SetBuffer(_KernelHandle, ShaderIDs.s_ResultDisplacements, resultsBuffer);
        }

        public int Query(int ownerHash, float minimumSpatialLength, Vector3[] queryPoints, float[] resultHeights, Vector3[] resultNormals, Vector3[] resultVelocities)
        {
            var result = (int)QueryStatus.OK;

            if (!UpdateQueryPoints(ownerHash, minimumSpatialLength, queryPoints, resultNormals != null ? queryPoints : null))
            {
                result |= (int)QueryStatus.PostFailed;
            }

            if (!RetrieveResults(ownerHash, null, resultHeights, resultNormals))
            {
                result |= (int)QueryStatus.RetrieveFailed;
            }

            if (resultVelocities != null)
            {
                result |= CalculateVelocities(ownerHash, resultVelocities);
            }

            return result;
        }
    }
}
