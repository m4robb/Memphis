using UnityEngine;
using UnityEngine.Events;
using System;

/*
 * This class is attached to a door handle. The door handle is child of a door.
 */
public class OpenDoor : MonoBehaviour
{
    public UnityEvent OnOpenDoor;
    public UnityEvent OnOpenDoorCompleted;
    public Rigidbody RB;
    public HingeJoint HJ;
    public AudioSource Creak;
    public float AngleLimit;

    public float MotorForce = 20;

    public float MotorVelocity = 50;

    public JointMotor HingeMotor;

    public LayerMask layerMask;

    float StartAngle;
    float CurrentAngle;

    bool AngleTrigger, HasCreaked;

    private void Start()
    {
        if (OnOpenDoor == null) OnOpenDoor = new UnityEvent();
        StartAngle =  HJ.angle;
        RB.isKinematic = true;
    }

    bool CreakDoor;

    public void ResetDoor()
    {
        AngleTrigger = false; 
        HasCreaked = false;

    }

    private void OnCollisionEnter(Collision collision)
    {

      
    }

    public void AutoClose(float CloseSpeed)
    {

        Debug.Log("close door");
        RB.isKinematic = false;
        HingeMotor = HJ.motor;
        HingeMotor.force = MotorForce;
        HingeMotor.targetVelocity = CloseSpeed;
        HJ.motor = HingeMotor;
        HJ.useMotor = true;
        HasCreaked = false;

        Invoke("ResetDoor", 4);
        if (!HasCreaked && Creak)
        {
            Creak.Play();
            HasCreaked = true;
        }
    }

    public void AutoOpen()
    {
        AngleTrigger = false;
        RB.isKinematic = false;
        HingeMotor = HJ.motor;
        HingeMotor.force = MotorForce;
        HingeMotor.targetVelocity = MotorVelocity;
        HJ.motor = HingeMotor;
        HJ.useMotor = true;
        Debug.Log("Hello");
        StartAngle = HJ.angle;
        Invoke("ResetDoor", 4);

        //AngleTrigger = true;
        OnOpenDoor.Invoke();

        if (!HasCreaked && Creak)
        {
            Creak.Play();
            HasCreaked = true;
        }
    }

    void Update()
    {
        if ( float.IsNaN(StartAngle))
        {
           
            StartAngle = HJ.angle;
        }

        CurrentAngle = Mathf.Abs(StartAngle - HJ.angle);



       

        if (CurrentAngle > AngleLimit && !AngleTrigger && OnOpenDoorCompleted != null)
        {
           
            AngleTrigger = true;
            OnOpenDoorCompleted.Invoke();
        }

    }


}
