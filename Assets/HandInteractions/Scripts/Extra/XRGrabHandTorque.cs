using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[CanSelectMultiple(true)]

public class XRGrabHandTorque : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{

    public float ClenchValue = .3f;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor selectInteractor = null;

    Vector3 PreviousRotationVector;

    public Transform TargetTranform;


    HandBridge HB;





    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        selectInteractor = args.interactorObject;

        PreviousRotationVector = selectInteractor.transform.eulerAngles;

        HB =  args.interactorObject.transform.GetComponent<HandBridge>();

        if(HB != null)
        {
            HB.ConnectToHandAnimator.GripModifier= ClenchValue;
        }

        base.OnSelectEntered(args);


    }



    protected override void OnSelectExited(SelectExitEventArgs args)
    {


        selectInteractor = null;

        if (HB != null)
        {
            HB.ConnectToHandAnimator.GripModifier = 1;
            HB = null;
        }

        base.OnSelectExited(args);

    }

    private void LateUpdate()
    {
        if (selectInteractor!= null)
        {

            ;
            float TwistValue = (PreviousRotationVector.z - selectInteractor.transform.eulerAngles.z);

            Debug.Log(selectInteractor.transform.eulerAngles.z);

            float Angle = transform.localEulerAngles.z - TwistValue;

           // Angle = (Angle > 180) ? Angle - 360 : Angle;

            //if (Angle > 0)
            //{
            //    Angle = 0;
            //    if (Closed != null) Closed.Invoke();
            //}

            //if (Angle < Limit)
            //{
            //    Angle = Limit;
            //    if (Closed != null) Opened.Invoke();
            //}

            Vector3 NewAngle = new Vector3(0, 0, 0);

            transform.localEulerAngles = NewAngle;

            PreviousRotationVector = selectInteractor.transform.eulerAngles;
        }
    }

}
