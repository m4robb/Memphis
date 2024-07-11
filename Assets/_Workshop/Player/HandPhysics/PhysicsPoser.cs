using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsPoser : MonoBehaviour
{
    public float physicsRange = 0.1f;
    public LayerMask physicsMask = ~0;
    public LayerMask InteractionMask = ~0;

    [Range(0, 1)] public float slowDownVelocity = 0.75f;
    [Range(0, 1)] public float slowDownAngularVelocity = 0.75f;

    [Range(0, 100)] public float maxPositionChange = 75.0f;
    [Range(0, 100)] public float maxRotationChange = 75.0f;

    private Rigidbody rigidBody = null;
    SphereCollider[] HandColliderArray;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor interactor = null;
    private ActionBasedController controller = null;
    public ActionBasedController controller2 = null;
    public HeightController HC;

    public bool IsFloating;


    float StartHeight;
    float HandHeight;

    private Vector3 targetPosition = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        interactor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();

        controller = GetComponent<ActionBasedController>();
    }

    private void Start()
    {
        XRControllerState controllerState = controller.currentControllerState;
        //controller.GetControllerState(out XRControllerState controllerState);

        controllerState.inputTrackingState = InputTrackingState.None;
        //controllerState.poseDataFlags = PoseDataFlags.NoData;
        MoveUsingTransform();
        RotateUsingTransform();

        if (HC != null) StartHeight = HC.TargetDistance;

    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if ((InteractionMask.value & (1 << other.transform.gameObject.layer)) > 0)
    //    {
           
    //      foreach(SphereCollider HandCollider in HandColliderArray )
    //        HandCollider.isTrigger = true;
    //    }

    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    foreach (SphereCollider HandCollider in HandColliderArray)
    //        HandCollider.isTrigger = false;
    //}



    private void UpdateTracking(ActionBasedController controller)
    {
        targetPosition = controller.positionAction.action.ReadValue<Vector3>();
        targetRotation = controller.rotationAction.action.ReadValue<Quaternion>();
    }



    private void Update()
    {

        UpdateTracking(controller);


        //if(HC!=null)
        //{
        //    HandHeight = transform.localPosition.y - StartHeight;
   
        //}

        //if(HC != null && HandHeight < -1 * (StartHeight *.75f))
        //{
        //    HC.TargetDistance = 0;
        //}


        //if (HC != null && HandHeight > 0.1f)

        //    //if (HC != null && HandHeight > -.5 * (StartHeight * .95f))
        //{
        //    HC.TargetDistance = StartHeight;
        //}
             

        if (IsHoldingObject() || !WithinPhysicsRange())
        {
            //controller.enableInputTracking = true;
            MoveUsingTransform();
            RotateUsingTransform();
        }
        else
        {
            //controller.enableInputTracking = false;
            MoveUsingPhysics();
            RotateUsingPhysics();
        }

        if (WithinPhysicsRange())
        {
            controller.enableInputTracking = false;
        }
        else
        {
            controller.enableInputTracking = true;
        }

        if (WithinInteractioRange())
        {
            controller.enableInputTracking = true;
        }

        if (IsHoldingObject())
        {
            controller.enableInputTracking = true;
        }
    }

    public void DoSelectEnter()
    {
        controller.enableInputTracking = true;
        HandColliderArray = GetComponentsInChildren<SphereCollider>();
        foreach (SphereCollider HandCollider in HandColliderArray)
            HandCollider.isTrigger = true;
    }

    public void DoSelectExit()
    {
        HandColliderArray = GetComponentsInChildren<SphereCollider>();
        foreach (SphereCollider HandCollider in HandColliderArray)
            HandCollider.isTrigger = false;
    }

    public bool IsHoldingObject() => interactor.hasSelection;

    public bool WithinPhysicsRange() => Physics.CheckSphere(controller2.transform.position, physicsRange, physicsMask, QueryTriggerInteraction.Ignore);
    public bool WithinInteractioRange() => Physics.CheckSphere(controller2.transform.position, physicsRange + 0.01f, InteractionMask, QueryTriggerInteraction.Ignore);

    private void MoveUsingPhysics()
    {
        rigidBody.linearVelocity *= slowDownVelocity;
        Vector3 velocity = FindNewVelocity();

        if (IsValidVelocity(velocity.x))

            
        {
           
            float maxChange = maxPositionChange * Time.deltaTime;
            rigidBody.linearVelocity = Vector3.MoveTowards(rigidBody.linearVelocity, velocity, maxChange);
        }
    }

    private Vector3 FindNewVelocity()
    {
        Vector3 worldPosition = transform.root.TransformPoint(targetPosition);
        Vector3 difference = worldPosition - rigidBody.position;
        return difference / Time.deltaTime;
    }

    private void RotateUsingPhysics()
    {
        rigidBody.angularVelocity *= slowDownAngularVelocity;
        Vector3 angularVelocity = FindNewAngularVelocity();

        if (IsValidVelocity(angularVelocity.x))
        {
            float maxChange = maxRotationChange * Time.deltaTime;
            rigidBody.angularVelocity = Vector3.MoveTowards(rigidBody.angularVelocity, angularVelocity, maxChange);
        }
    }

    private Vector3 FindNewAngularVelocity()
    {
        Quaternion worldRotation = transform.root.rotation * targetRotation;
        Quaternion difference = worldRotation * Quaternion.Inverse(rigidBody.rotation);
        difference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180)
            angleInDegrees -= 360;

        return (rotationAxis * angleInDegrees * Mathf.Deg2Rad) / Time.deltaTime;
    }

    private bool IsValidVelocity(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private void MoveUsingTransform()
    {
        if (rigidBody.isKinematic) return;
        rigidBody.linearVelocity = Vector3.zero;
        transform.localPosition = targetPosition;
    }

    private void RotateUsingTransform()
    {

        if (rigidBody.isKinematic) return;
        rigidBody.angularVelocity = Vector3.zero;
        transform.localRotation = targetRotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, physicsRange);
    }

    private void OnValidate()
    {
        if (TryGetComponent(out Rigidbody rigidBody))
            rigidBody.useGravity = false;
    }
}
