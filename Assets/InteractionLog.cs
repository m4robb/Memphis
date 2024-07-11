using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class InteractionLog : MonoBehaviour
{
    public XRBaseController RightHand;
    public XRBaseController LeftHand;

    void Start()
    {
        
    }

    public void RightHandHaptic()
    {
        RightHand.SendHapticImpulse(.5f, .25f);
    }

    public void LeftHandHaptic()
    {
        RightHand.SendHapticImpulse(.5f, .25f);
    }

     void Update()
    {
        
    }
}
