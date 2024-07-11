using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


public class SpinnerComponent : UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable
{
    // Start is called before the first frame update


    bool IsPulling;

    UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor ThisInteractor;

    Vector3 StoredPosition = Vector3.zero, CurrentVector = Vector3.zero, DrawerPosition;

    public float MaxDistance = 1;

    public AudioSource SFX;

    public float ClenchValue = .3f;

    HandBridge HB;

    Rigidbody RB;

    bool IsOpening;

    //protected override void OnHoverEntered(XRBaseInteractor interactor)

    //{

    //    ThisInteractor = interactor;
    //    StoredPosition = ThisInteractor.transform.position;

    //    if (SFX != null) SFX.Play();
    //    IsOpening = true;
    //}
    //protected override void OnHoverExited(XRBaseInteractor interactor)
    //{
    //    ThisInteractor = null;
    //    if (SFX != null) SFX.Stop();
    //    IsOpening = false;
    //}







    protected override void OnSelectEntered(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {

        HB = interactor.GetComponent<HandBridge>();

        if (HB != null)
        {
            HB.ConnectToHandAnimator.GripModifier = ClenchValue;
        }
        if (SFX != null) SFX.Play();
        IsOpening = true;



        ThisInteractor = interactor;
        StoredPosition = ThisInteractor.transform.position;
    }

    protected override void OnSelectExited(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        if (SFX != null) SFX.Stop();
        IsOpening = false;
        ThisInteractor = null;

    }

    //private void LateUpdate()
    //{
    //    if(RB == null)
    //    {
    //        RB = GetComponent<Rigidbody>();
    //    }

    //    if (!IsOpening) return;
    //    if (SFX != null) SFX.volume = Mathf.Abs(RB.velocity.magnitude);
    //}

    void FixedUpdate()
    {

        if (RB == null)
        {
            RB = GetComponent<Rigidbody>();
        }


        if (ThisInteractor != null)
        {


            CurrentVector = ThisInteractor.transform.position;

            RB.MovePosition(CurrentVector);

            //transform.position = CurrentVector;


            //CurrentVector = transform.InverseTransformDirection(ThisInteractor.transform.position - StoredPosition);
            //DrawerPosition.z += CurrentVector.z;

            //if (DrawerPosition.z < 0) DrawerPosition.z = 0;
            //if (DrawerPosition.z > MaxDistance) DrawerPosition.z = MaxDistance;

            //transform.localPosition = DrawerPosition;
            //StoredPosition = ThisInteractor.transform.position;
        }

    }
}
