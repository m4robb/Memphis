using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ReflectionProbeController : MonoBehaviour
{
    public ReflectionProbe RP;
    public HDAdditionalReflectionData HDARD;
    void Start()
    {
        //Invoke("DoProbe", 1f);
    }

   public void DoRenderProbe()
    {
        HDARD.RequestRenderNextUpdate();
    }

    public void SetModeEveryFrame()
    {
        HDARD.realtimeMode = ProbeSettings.RealtimeMode.EveryFrame;
    }

    public void SetModeOnDemand()
    {
        HDARD.realtimeMode = ProbeSettings.RealtimeMode.OnDemand;
        HDARD.RequestRenderNextUpdate();
    }
    void Update()
    {
        
    }
}
