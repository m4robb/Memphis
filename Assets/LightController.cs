using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class LightController : MonoBehaviour
{
    public HDAdditionalLightData HDALD;
    public int UpdateFrequency = 10;

    int FrameCounter = 0;



    void Start()
    {
        HDALD.shadowUpdateMode = ShadowUpdateMode.OnDemand;

        HDALD.RequestShadowMapRendering();
    }


    void Update()
    {
        if(FrameCounter > UpdateFrequency)
        {
            FrameCounter = 0;
            HDALD.RequestShadowMapRendering();
        }

        FrameCounter++;
    }
}
