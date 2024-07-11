using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRPuppetInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable
{

    public Rigidbody RB;

    public Vector3 RotationOffset = Vector3.zero;

    public Vector3 PositionOffset = Vector3.zero;
 
    UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor ThisInteractor;

    GameObject StoredHand;

    HandBridge HB;

    bool IsRightHand = false;

    public bool IsPuppet;

    public PuppetHandController PHC;
    public HandPuppetMaster HPM;

    public bool IsHoverAction;

    public bool IsHovering;

    public bool Ignore;

    public bool IsGrabbed;

    bool IsFlying;

    Vector3 FlightDestination;


    public void FlyPuppet()
    {
        IsPuppet = false;

        if (StoredHand)
            StoredHand.SetActive(true);

        IsFlying = true;
        ThisInteractor = null;

        RB.useGravity = false;
        RB.isKinematic = false;
    }

    public void DropPuppet()
    {

        IsPuppet = false;

        if(StoredHand)
            StoredHand.SetActive(true);

        RB.useGravity = true;
        ThisInteractor = null;

    }

    IEnumerator RemoveHover ()
    {
        yield return new WaitForSeconds(2);

        IsHovering = false;
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (Ignore) return;

        if (IsFlying) return;

        if (IsGrabbed) return;
 

        if (gameObject != HPM.SignalPuppet) {
            //HPM.WrongPuppet();
            return;
        }
        IsHovering = true;
        ConnectPuppet(args.interactorObject);
        StartCoroutine(RemoveHover());
        base.OnHoverEntered(args);

    }
 

    protected override void OnHoverExited(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
       
        //IsHovering = false;

    }

    void ConnectPuppet(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
    {

        ThisInteractor = interactor;
        HB = interactor.transform.GetComponent<HandBridge>();
        StoredPosition = interactor.transform.position;
        IsPuppet = true;
        PHC.XRPI = this;

        IsGrabbed = true;


        if (HB && HB.ConnectToNearFarInteractor)
        {
            if (HB.ConnectToNearFarInteractor.handedness == InteractorHandedness.Right)
                PHC.IsRightHand = true;
            else
                PHC.IsRightHand = false;


            PHC.IsPuppet = true;

            PHC.PuppetController.Play("MainAction");

            //if (StoredHand && StoredHand != HB.ConnectToHandAnimator.gameObject)
            //    StoredHand.SetActive(true);

            //StoredHand = HB.ConnectToHandAnimator.gameObject;
            //StoredHand.SetActive(false);
        }
        RB.useGravity = false;
        RB.isKinematic = false;
    }

    protected override void OnSelectEntered(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        if (IsGrabbed) return;
        if (Ignore) return;
        if (IsFlying) return;

        if (gameObject != HPM.SignalPuppet)
        {
            //HPM.WrongPuppet();
            return;
        }

        IsHovering = false;
        ConnectPuppet(interactor);
       
    }

    Vector3 StoredPosition;


    float ScaleFactor = 1;
    void FixedUpdate()
    {

        if (IsFlying)
        {
            if (!RB) return;

            RB.useGravity = false;
            RB.isKinematic = false;
            Vector3 CurrentPosition = transform.position;
            CurrentPosition.y += Time.deltaTime * .25f;
            RB.MovePosition(CurrentPosition);

            Vector3 Scale = transform.localScale;

            Scale *= ScaleFactor;
            transform.localScale = Scale;

            if (ScaleFactor > 0)
            {
                ScaleFactor -= Time.deltaTime * .0003f;
            }

            if (transform.position.y > 3) IsFlying = false;

        }
        if (IsPuppet)
        {

            if (!RB) return;
            RB.MovePosition(Vector3.Lerp(transform.position,ThisInteractor.transform.position, Time.deltaTime * 20f));
            RB.MoveRotation(Quaternion.Lerp(transform.rotation,ThisInteractor.transform.rotation * Quaternion.Euler(RotationOffset), Time.deltaTime * 20));
            
        }


    }
}
