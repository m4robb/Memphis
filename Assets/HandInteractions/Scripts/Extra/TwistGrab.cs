using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;


public enum TwistAxis
{
    XAxis,
    YAxis,
    ZAxis
};

public class TwistGrab : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{

    public TwistAxis RotationAxis = new TwistAxis();

    public UnityEvent Opened;
    public UnityEvent Closed;
    public float Limit = -20f;
   
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

    private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable selectInteractor = null;

    Rigidbody mm_Rigidbody;

    float PreviousRotationValue = 0;

    Vector3 PreviousRotationVector;

    protected override void Awake()
    {
        base.Awake();
       selectEntered.AddListener(StartTwist);
       selectExited.AddListener(EndTwist);
    }

    void StartTwist(SelectEnterEventArgs args)
    {
       
        selectInteractor = args.interactableObject;
        PreviousRotationVector =selectInteractor.transform.eulerAngles;
        PreviousRotationValue = GetRotation(selectInteractor.transform.eulerAngles);
    }
    void EndTwist(SelectExitEventArgs args)
    {
        selectInteractor = null;
    }

    void Start()
    {
        mm_Rigidbody = GetComponent<Rigidbody>();
        mm_WasKinematic = mm_Rigidbody.isKinematic;
        mm_UsedGravity = mm_Rigidbody.useGravity;
        mm_OldDrag = mm_Rigidbody.linearDamping;
        mm_OldAngularDrag = mm_Rigidbody.angularDamping;



    }

    private void Update()
    {
        if (selectInteractor != null)
        {



             Vector3 NewAngle = Vector3.zero;
             float Angle = 0;

            if (RotationAxis == TwistAxis.YAxis)
            {
                float TwistValue = (PreviousRotationVector.y - selectInteractor.transform.eulerAngles.y);

                Angle = transform.localEulerAngles.y - TwistValue;

                Angle = (Angle > 180) ? Angle - 360 : Angle;

                if (Angle > 0)
                {
                    Angle = 0;
                    if (Closed != null) Closed.Invoke();
                }

                if (Angle < Limit)
                {
                    Angle = Limit;
                    if (Closed != null) Opened.Invoke();
                }

                NewAngle = new Vector3(0, Angle, 0);
            }

            if (RotationAxis == TwistAxis.ZAxis)
            {
                float TwistValue = (PreviousRotationVector.z - selectInteractor.transform.eulerAngles.z);

                Angle = transform.localEulerAngles.z - TwistValue;

                Angle = (Angle > 180) ? Angle - 360 : Angle;

                if (Angle > 0)
                {
                    Angle = 0;
                    if (Closed != null) Closed.Invoke();
                }

                if (Angle < Limit)
                {
                    Angle = Limit;
                    if (Closed != null) Opened.Invoke();
                }

                NewAngle = new Vector3(0, 0,Angle);
            }
            if (RotationAxis == TwistAxis.XAxis)
            {
                float TwistValue = (PreviousRotationVector.x - selectInteractor.transform.eulerAngles.x);

                Angle = transform.localEulerAngles.x - TwistValue;

                Angle = (Angle > 180) ? Angle - 360 : Angle;

                if (Angle > 0)
                {
                    Angle = 0;
                    if (Closed != null) Closed.Invoke();
                }

                if (Angle < Limit)
                {
                    Angle = Limit;
                    if (Closed != null) Opened.Invoke();
                }

                NewAngle = new Vector3(Angle, 0, 0);
            }

            transform.localEulerAngles = NewAngle;

            PreviousRotationVector = selectInteractor.transform.eulerAngles;
        }
    }

    //public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    //{
    //    if (hoverInteractor)
    //    {
    //        float newHandRotation = GetRotation(hoverInteractor.transform.eulerAngles);
    //        float TwistValue = (PreviousRotationValue - newHandRotation) * 2;
    //        PreviousRotationValue = newHandRotation;

    //        float Angle = transform.localEulerAngles.y - TwistValue;

    //        Angle = (Angle > 180) ? Angle - 360 : Angle;

    //        if (Angle > 0) Angle = 0;

    //        if (Angle < -30) Angle = 30;

    //        Vector3 NewAngle = new Vector3(0, Angle, 0);
 
    //        transform.localEulerAngles = NewAngle;
    //    }
    //    }


    private float GetRotation(Vector3 position)
    {
        Vector3 localRotation = transform.root.InverseTransformPoint(position);

        if (RotationAxis == TwistAxis.YAxis)
            return localRotation.y;

        if (RotationAxis == TwistAxis.XAxis)
            return localRotation.x;

        if (RotationAxis == TwistAxis.ZAxis)
            return localRotation.z;

        return localRotation.y;
    }



}
