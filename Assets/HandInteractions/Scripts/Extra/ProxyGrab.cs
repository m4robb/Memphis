using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[CanSelectMultiple(true)]

public class ProxyGrab : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private Vector3 interactorPosition = Vector3.zero;
    private Quaternion interactorRotation = Quaternion.identity;



    bool mm_WasKinematic;
    bool mm_UsedGravity;
    float mm_OldDrag;
    float mm_OldAngularDrag;



    Rigidbody mm_Rigidbody;


    public float ClenchValue = .3f;




    HandBridge HB;





    protected override void OnSelectEntered(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {

        HB = interactor.GetComponent<HandBridge>();

        if(HB != null)
        {
            HB.ConnectToHandAnimator.GripModifier= ClenchValue;
        }

       // base.selectEntered(interactor);

        StoreInteractor(interactor);
        MatchAttachmentPoints(interactor);
   

        if (mm_Rigidbody != null)
             mm_Rigidbody.isKinematic = true;
    }

    private void StoreInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        interactorPosition = interactor.attachTransform.localPosition;
        interactorRotation = interactor.attachTransform.localRotation;
    }

    private void MatchAttachmentPoints(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        if (useDynamicAttach) return;
        bool hasAttach = attachTransform != null;
        interactor.attachTransform.position = hasAttach ? attachTransform.position : transform.position;
        interactor.attachTransform.rotation = hasAttach ? attachTransform.rotation : transform.rotation;
    }



    protected override void OnSelectExited(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
 
        if (HB != null)
        {
            HB.ConnectToHandAnimator.GripModifier = 1;
            HB = null;
        }


       
    }

}
