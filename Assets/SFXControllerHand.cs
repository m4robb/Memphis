using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SFXControllerHand : MonoBehaviour
{

    public float KineticEnergy = 0; 
    
    public bool IsBeingHeld;


    //private List<Hand> holdingHands = new List<Hand>();

   


    //private void HandHoverUpdate(Hand hand)
    //{
    //    GrabTypes startingGrabType = hand.GetGrabStarting();

    //    if (startingGrabType != GrabTypes.None)
    //    {
    //        holdingHands.Add(hand);
    //    }
    //}

    void Start()
    {
        
    }


    void Update()
    {
        //KineticEnergy = 0;
        //for (int i = 0; i < holdingHands.Count; i++)
        //{

        //    KineticEnergy += Mathf.Round(holdingHands[i].GetTrackedObjectVelocity().magnitude * 1000f) / 1000f;

        //    if (holdingHands[i].IsGrabEnding(this.gameObject))
        //    {
        //        Util.FastRemove(holdingHands, i);
        //    }
        //}

        //if (holdingHands.Count == 0) IsBeingHeld = false;
        //else IsBeingHeld = true;
    }
}
