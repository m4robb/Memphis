using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrackHittingPower : MonoBehaviour
{
    // Start is called before the first frame update

    Rigidbody RB;
    public float KineticEnergy = 0;

    bool DisplayKineticEnergy;

    //private List<Hand> holdingHands = new List<Hand>();

    void Start()
    {
        RB = GetComponent<Rigidbody>();
    }

    //private void HandHoverUpdate(Hand hand)
    //{
    //    GrabTypes startingGrabType = hand.GetGrabStarting();

    //    if (startingGrabType != GrabTypes.None)
    //    {
    //        DisplayKineticEnergy = true;
    //        holdingHands.Add(hand);
    //    }
    //}

    // Update is called once per frame
    void FixedUpdate()
    {

        //for (int i = 0; i < holdingHands.Count; i++)
        //{
        //    if (holdingHands[i].IsGrabEnding(this.gameObject))
        //    {
        //        DisplayKineticEnergy = false;
        //    }
        //}

        if (DisplayKineticEnergy)
            KineticEnergy = Mathf.Round(0.5f * RB.mass * RB.linearVelocity.sqrMagnitude * 1000f) / 1000f;
        else
            KineticEnergy = 0;
    }
}
