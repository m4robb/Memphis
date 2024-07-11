using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PSVRSettings : MonoBehaviour
{

    List<XRDisplaySubsystem> xrDisplays = new List<XRDisplaySubsystem>();

    void Update()
    {
        
        SubsystemManager.GetSubsystems(xrDisplays);
        if (xrDisplays.Count == 1)
        {
            // Enable Foveated Rendering
            //xrDisplays[0].foveatedRenderingLevel = 1.0f;
            // Enable eye-tracking for Foveated Rendering
            xrDisplays[0].foveatedRenderingFlags = XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
        }
    }

}
