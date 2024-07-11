using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class RecordPlayerController : MonoBehaviour
{

    public Transform Arm;

    public UnityEvent OnRecordPlay;
    public UnityEvent OnRecordStop;

    public Vector3 ArmStartPosition;
    public Vector3 ArmPlayPosition;

    public Rigidbody RB;

    public XRGrabHand GrabRecord; 

    public bool IsPlaying;

    public bool IsMoving;

    public void SetConstraints(Rigidbody _RB)
    {
        RB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public void FreeConstraints(Rigidbody _RB)
    {
        RB.constraints = RigidbodyConstraints.None;
    }

    public void TurnOff()
    {
       
        //if (!IsPlaying) return;

        Debug.Log("Turn OFFFFRFFFFFFFF");

        Arm.DOLocalRotate(ArmStartPosition, 3).OnComplete(() =>
        {
            IsMoving = false;
        });
        IsPlaying = false;

        if (OnRecordStop != null) OnRecordStop.Invoke();

    }
    public void TurnOnOff()
    {
        Debug.Log("TurnOnOff");
        if (IsMoving) return;
        IsMoving = true;
        Debug.Log("TurnOnOff2");

        if (!IsPlaying)
        {
            Arm.DOLocalRotate(ArmPlayPosition, 3).OnComplete(() =>
            {
                IsMoving = false;
               
                if (OnRecordPlay != null) OnRecordPlay.Invoke();
               
            });
            IsPlaying = true;
            return;
        }
        else
        {

            Debug.Log("TurnOnOff3");
            if (OnRecordStop != null) OnRecordStop.Invoke();
            Arm.DOLocalRotate(ArmStartPosition, 3).OnComplete(() =>
            {
                IsMoving = false;
            });
            IsPlaying = false;
        }
    }

    private void Update()
    {

        //if (GrabRecord)
        //{
        //    if (IsPlaying || IsMoving) GrabRecord.enabled = false;
        //    if (!IsPlaying || !IsMoving) GrabRecord.enabled = true;
        //}


    }


}
