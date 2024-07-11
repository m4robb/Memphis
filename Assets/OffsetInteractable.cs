using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OffsetInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    protected override void Awake()
    {
        base.Awake();
        CreateAttachTransform();
    }
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        MatchAttachPoint(args.interactorObject);
    }

    protected void MatchAttachPoint(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
    {
        if (IsFirstSelecting(interactor))
        {

            
            attachTransform.position = interactor.transform.position;
            attachTransform.rotation = interactor.GetAttachTransform(this).rotation;


            //bool isDirect = interactor is XRDirectInteractor;
            //Debug.Log(isDirect);
            //attachTransform.position = isDirect ? interactor.GetAttachTransform(this).position : transform.position;
            //attachTransform.rotation = isDirect ? interactor.GetAttachTransform(this).rotation : transform.rotation;
        }
    }

    private bool IsFirstSelecting(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
    {
        return interactor == firstInteractorSelecting;
    }

    private void CreateAttachTransform()
    {
        if (attachTransform == null)
        {
            GameObject createdAttachTransform = new GameObject();
            createdAttachTransform.transform.parent = this.transform;
            attachTransform = createdAttachTransform.transform;
        }
    }
}