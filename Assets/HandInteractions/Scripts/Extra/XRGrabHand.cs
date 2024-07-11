using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[CanSelectMultiple(true)]

public class XRGrabHand : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{

    public float ClenchValue = .3f;
    public UnityEvent TwoHands;



    HandBridge HB;



    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {


        if (interactorsSelecting.Count > 1 && TwoHands != null) TwoHands.Invoke();

        //HB = args.interactorObject.transform.GetComponent<HandBridge>();

        //if(HB != null)
        //{
        //    HB.ConnectToHandAnimator.GripModifier= ClenchValue;

        //    SphereCollider[] ColliderArray = HB.ConnectToInteractor.gameObject.GetComponentsInChildren<SphereCollider>();

        //    foreach (SphereCollider _Sphere in ColliderArray) _Sphere.isTrigger = true;
        //}

        base.OnSelectEntered(args);


    }



    protected override void OnSelectExited(SelectExitEventArgs args)
    {
 
        if (HB != null)
        {
            HB.ConnectToHandAnimator.GripModifier = 1;

            SphereCollider[] ColliderArray = HB.ConnectToInteractor.gameObject.GetComponentsInChildren<SphereCollider>();

            foreach (SphereCollider _Sphere in ColliderArray) _Sphere.isTrigger = false;

            HB = null;
        }

        base.OnSelectExited(args);

    }

}
