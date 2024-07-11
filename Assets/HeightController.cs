using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HeightController : MonoBehaviour
{
    public float TargetDistance = 1.3f;

    public float CrouchHeight = -0.3f;
    //int layerMask;

    public LayerMask mask = -1;

    public float Threshold = 0.05f;

    public float Margin = .5f;

    public bool IsStandalone;

    public float StandaloneHeight = 1.8f;

    public float CrouchThreshold = .9f;

    public float VerticalMultiplier = 1;



    RaycastHit hit;

    Vector3 CurrentPosition;

    Transform HeadPosition;

    float HeadHeight;
    public float StartHeight;

    bool OnLadder;

    private void Start()
    {
        StartHeight = TargetDistance;
    }

    public void EnterLadder(float _LadderHeight)
    {

        Debug.Log("Ladder enter");
        TargetDistance = _LadderHeight;
        OnLadder = true;
    }

    public void ExitLadder()
    {
        TargetDistance = StartHeight;
        OnLadder = false;
    }

    private bool Lock = false;
    void Update()
    {

        //if (!HeadPosition)
        //{
           
           
        //    return;
        //}


        // HeadPosition = Camera.main.transform;
        //
        //
        // HeadHeight = HeadPosition.localPosition.y;
        //
        //
        //  if(HeadHeight < CrouchThreshold && !OnLadder)
        //     TargetDistance = 0;
        //
        // if (HeadHeight > CrouchThreshold && !OnLadder)
        //     TargetDistance = StartHeight;


        if (IsStandalone) TargetDistance = StandaloneHeight;


        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, mask.value))
        {

 
         

            if (hit.distance > TargetDistance + Time.deltaTime)
            {
            
                CurrentPosition = transform.position;
                CurrentPosition.y -=  Time.deltaTime * VerticalMultiplier;
                transform.position = CurrentPosition;
            }
            
            if (hit.distance < TargetDistance - Time.deltaTime)
            {
                CurrentPosition = transform.position;
                CurrentPosition.y +=  Time.deltaTime * .5f;
                transform.position = CurrentPosition;
            }

        }
    }
}
