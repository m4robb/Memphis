using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRDoorController : UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable
{

    public Rigidbody RB;
    public AudioSource Creak;

    private Vector3 interactorPosition = Vector3.zero;
    private Quaternion interactorRotation = Quaternion.identity;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable selectInteractor = null;
    private const float ForceMultiplier = 50f;

    bool IsGrabbing;

    private Vector3 Cross;
    private float Angle;

    Vector3 Force;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        //IsGrabbing = true;
        selectInteractor = args.interactableObject;
        //MatchPullAction(interactor);
        //MatchAttachmentPoints(interactor);
    }

    private void StoreInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        interactorPosition = interactor.attachTransform.localPosition;
        interactorRotation = interactor.attachTransform.localRotation;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (selectInteractor!= null)
        {

           
           // Vector3 newHandHeight =  hoverInteractor.transform.position;

           
            Vector3 doorPivotToHand = selectInteractor.transform.position - transform.parent.position;
            doorPivotToHand.y = 0;
            Force = selectInteractor.transform.position - transform.position;
            Cross = Vector3.Cross(doorPivotToHand, Force);
            Angle = Vector3.Angle(doorPivotToHand, Force);
            RB.angularVelocity = Cross * Angle * ForceMultiplier * 4;

            Debug.Log(RB.angularVelocity);
            //float handDifference = previousHandHeight - newHandHeight;
        }
    }

        private void MatchPullAction(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        //bool hasAttach = attachTransform != null;

        //force = hand.transform.position - transform.position;

        //// Cross product between force and direction. 
        //cross = Vector3.Cross(doorPivotToHand, force);
        //angle = Vector3.Angle(doorPivotToHand, force);

        //interactor.attachTransform.position = hasAttach ? attachTransform.position : transform.position;
        //interactor.attachTransform.rotation = hasAttach ? attachTransform.rotation : transform.rotation;
    }

    private void MatchAttachmentPoints(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        //bool hasAttach = attachTransform != null;
        //interactor.attachTransform.position = hasAttach ? attachTransform.position : transform.position;
        //interactor.attachTransform.rotation = hasAttach ? attachTransform.rotation : transform.rotation;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
         selectInteractor = null;
        //ResetAttachmentPoint(interactor);
        //ClearInteractor(interactor);
    }

    private void ResetAttachmentPoint(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        interactor.attachTransform.localPosition = interactorPosition;
        interactor.attachTransform.localRotation = interactorRotation;
    }

    private void ClearInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        interactorPosition = Vector3.zero;
        interactorRotation = Quaternion.identity;
    }
}
