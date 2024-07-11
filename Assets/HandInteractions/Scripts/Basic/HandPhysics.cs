using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPhysics : MonoBehaviour
{

    public float SmoothingAmount = 15f;
    public Transform Target = null;

    public Rigidbody RB;
    Vector3 TargetPosition = Vector3.zero;
    Quaternion TargetRotation = Quaternion.identity;
    private void Awake()
    {
        
    }

    private void Start()
    {
TeleportToTarget();
    }

    //private void FixedUpdate()
    //{

    //}

    private void SetTargetPosition()
    {
        float _Time = SmoothingAmount * Time.unscaledDeltaTime;
        TargetPosition = Vector3.Lerp(TargetPosition, Target.position, _Time);
    }

    private void SetTargetRotation()
    {
        float _Time = SmoothingAmount * Time.unscaledDeltaTime;
        TargetRotation = Quaternion.Slerp(TargetRotation, Target.rotation, _Time);
    }

    private void FixedUpdate()
    {
        SetTargetPosition();
        SetTargetRotation();
        MoveToController();
        RotateToController();
    }

    private void MoveToController()
    {
        Vector3 _PositionDelta = TargetPosition - transform.position;
        RB.linearVelocity = Vector3.zero;
        RB.MovePosition(transform.position + _PositionDelta);
    }

    private void RotateToController()
    {
        RB.angularVelocity = Vector3.zero;
        RB.MoveRotation(Target.rotation);
    }

    public void TeleportToTarget()
    {
        TargetPosition = Target.position;
        TargetRotation = Target.rotation;
        transform.position = TargetPosition;
        transform.rotation = TargetRotation;
    }
}
