using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;




public enum DoorSide
{
    Left,
    Right
}

[RequireComponent(typeof(Rigidbody))]

public class DoorComponent : UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable
{


    // Start is called before the first frame update


    bool IsPulling;

    UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor ThisInteractor;

    Vector3 StoredPosition = Vector3.zero, CurrentVector = Vector3.zero, DoorRotation;

    public float MaxAngle = 90;

    public Rigidbody RB;

    public AudioSource SFX;

    public float ClenchValue = .3f;

    bool IsOpening;

    protected override void OnHoverEntered(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        if (SFX != null) SFX.Play();
        IsOpening = true;
    }
    protected override void OnHoverExited(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        if (SFX != null) SFX.Stop();
        IsOpening = false;
    }

    protected override void OnSelectEntered(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
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


    private void LateUpdate()
    {
        if (RB == null)
        {
            RB = GetComponent<Rigidbody>();
        }

        if (!IsOpening) return;
        if (SFX != null) SFX.volume = Mathf.Abs(RB.linearVelocity.magnitude);
    }

    void FixedUpdate()
    {
        //DoorRotation = transform.localEulerAngles;


        if (ThisInteractor != null)
        {

            RB.MovePosition(ThisInteractor.transform.position);

        }

    }
}
