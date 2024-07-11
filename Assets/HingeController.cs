using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HingeController : MonoBehaviour
{
    public HingeJoint HJ;

    public UnityEvent OnOpenHingeStart;
    public UnityEvent OnOpenHingeEnd;

    public bool IsOpen, HingeTrigger;

    public float AngleLimit;

    public float MotorForce = 20;

    public float MotorVelocity = 50;

    public JointMotor HingeMotor;

    public bool IsReverse;

    float StartAngle, CurrentAngle;



    private void Start()
    {
      
        StartAngle = HJ.angle;
    }

    public void AutoOpenClose()
    {
        if (HingeTrigger) return;

        HingeTrigger = true;

        if (IsOpen)
        {
            MotorVelocity *= -1;
        }

        HingeMotor = HJ.motor;
        HingeMotor.force = MotorForce;
        HingeMotor.targetVelocity = MotorVelocity;
        HJ.motor = HingeMotor;
        HJ.useMotor = true;
        OnOpenHingeStart.Invoke();

    }

    void FixedUpdate()
    {


        if (HJ.angle == HJ.limits.min)
        {
            if (!IsReverse) IsOpen = false;
            if (IsReverse) IsOpen = true;
            HingeTrigger = false;
        }
        if (HJ.angle == HJ.limits.max)
        {

            if (IsReverse) IsOpen = false;
            if (!IsReverse) IsOpen = true;
            HingeTrigger = false;
        }


    }
}
