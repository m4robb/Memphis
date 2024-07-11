using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;


public class HandBridge : MonoBehaviour
{

    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor ConnectToInteractor;
    public NearFarInteractor ConnectToNearFarInteractor;
    public HandAnimator ConnectToHandAnimator;

}
