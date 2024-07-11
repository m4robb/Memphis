using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LadderInteraction : MonoBehaviour
{
    public ActionBasedContinuousMoveProvider ContinuousLocomotion;

    public float ClimbSpeed = .1f;

    public float DisengageThreshold = 1;

    float StartSpeed;

    float Timer;

    private void Start()
    {
        StartSpeed = ContinuousLocomotion.moveSpeed;
    }


    private void Update()
    {
        Timer += Time.deltaTime;
        if (Timer > DisengageThreshold) ContinuousLocomotion.moveSpeed = StartSpeed;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {

        if(hit.transform.tag == "Vertical")
        {
        Timer = 0;
        ContinuousLocomotion.moveSpeed = ClimbSpeed;
        }

    }

 }
