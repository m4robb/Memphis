// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Samples horizontal motion of water volume
    /// </summary>
    sealed class FlowQuery : QueryBase, IFlowProvider
    {
        static class ShaderIDs
        {
            public static readonly int s_ResultFlows = Shader.PropertyToID("_Crest_ResultFlows");
        }

        public FlowQuery(WaterRenderer water) : base(water)
        {
        }

        protected override ComputeShader QueriesCompute => WaterResources.Instance.Compute._QueryFlow;
        protected override string QueryKernelName => "CrestExecute";

        protected override void BindInputsAndOutputs(PropertyWrapperComputeStandalone wrapper, ComputeBuffer resultsBuffer)
        {
            ShaderProcessQueries.SetBuffer(_KernelHandle, ShaderIDs.s_ResultFlows, resultsBuffer);
        }

        public int Query(int ownerHash, float minimumSpatialLength, Vector3[] queryPoints, Vector3[] resultFlows)
        {
            return Query(ownerHash, minimumSpatialLength, queryPoints, resultFlows, null, null);
        }
    }
}
