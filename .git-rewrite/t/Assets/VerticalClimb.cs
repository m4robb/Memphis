using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VerticalClimb : MonoBehaviour
{

    public ActionBasedContinuousMoveProvider ABCMP;
    float StartMoveSpeed;
    private void Start()
    {
        StartMoveSpeed = ABCMP.moveSpeed;
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
       

        //if(hit.transform.tag == "Vertical") 
    }

}
