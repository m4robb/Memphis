using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[CanSelectMultiple(true)]

public class OffsetGrab : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private Vector3 interactorPosition = Vector3.zero;
    private Quaternion interactorRotation = Quaternion.identity;

    //public void DropSelected()
    // {
    //     base.OnSelectExited(interactor);
    //     ResetAttachmentPoint(interactor);
    //     ClearInteractor(interactor);
    // }

    bool mm_WasKinematic;
    bool mm_UsedGravity;
    float mm_OldDrag;
    float mm_OldAngularDrag;



    Rigidbody mm_Rigidbody;


    public float ClenchValue = .3f;

    public bool MakeKinematic;



    void Start()
    {
        mm_Rigidbody = GetComponent<Rigidbody>();
        mm_WasKinematic = mm_Rigidbody.isKinematic;
        mm_UsedGravity = mm_Rigidbody.useGravity;
        mm_OldDrag = mm_Rigidbody.linearDamping;
        mm_OldAngularDrag = mm_Rigidbody.angularDamping;
    }


    HandBridge HB;


    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (MakeKinematic && mm_Rigidbody != null) mm_Rigidbody.isKinematic = true;
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        if (MakeKinematic && mm_Rigidbody != null) mm_Rigidbody.isKinematic = false;
    }



    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {

        HB = args.interactorObject.transform.GetComponent<HandBridge>();

        if(HB != null)
        {
            HB.ConnectToHandAnimator.GripModifier= ClenchValue;
        }

        base.OnSelectEntered(args);

        StoreInteractor(args.interactorObject);
        MatchAttachmentPoints(args.interactorObject);
   

        if (mm_Rigidbody != null)
             mm_Rigidbody.isKinematic = true;
    }

    private void StoreInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
    {
        interactorPosition = interactor.transform.localPosition;
        interactorRotation = interactor.transform.localRotation;
    }

    private void MatchAttachmentPoints(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
    {
        if (useDynamicAttach) return;
        bool hasAttach = attachTransform != null;
        interactor.transform.position = hasAttach ? attachTransform.position : transform.position;
        interactor.transform.rotation = hasAttach ? attachTransform.rotation : transform.rotation;
    }



    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        if (MakeKinematic && mm_Rigidbody != null) mm_Rigidbody.isKinematic = false;
        if (HB != null)
        {
            HB.ConnectToHandAnimator.GripModifier = 1;
            HB = null;
        }
        if (interactorsSelecting.Count == 0)
        {
            //mm_Rigidbody.isKinematic =mm_WasKinematic;
            //mm_Rigidbody.useGravity =  mm_UsedGravity;
            //mm_Rigidbody.drag = mm_OldDrag;
            //mm_Rigidbody.angularDrag = mm_OldAngularDrag;
            base.OnSelectExited(args);
            ResetAttachmentPoint(args.interactorObject);
            ClearInteractor(args.interactorObject);
        }
        //base.OnSelectExited(interactor);

       
    }

    private void ResetAttachmentPoint(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
    {
        interactor.transform.localPosition = interactorPosition;
        interactor.transform.localRotation = interactorRotation;
    }

    private void ClearInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
    {
        interactorPosition = Vector3.zero;
        interactorRotation = Quaternion.identity;
    }
}
