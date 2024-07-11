using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorSlam : MonoBehaviour
{
    public Rigidbody Door;
    public HingeJoint DoorHinge;
    public AudioSource Slam;
    public AudioSource Creak;
    public JointMotor  HingeMotor;
    public float MotorForce = -100;
    public UnityEvent StartClose;
    public UnityEvent FinishClose;
    public float SlamTime = 2.8f;
    void Start()
    {
        
    }

    bool HasShut, HasSlammed;

   public void ResetDoor()
    {
        HasShut = false; 
        HasSlammed = false;
    }

    IEnumerator DoSlamTimed()
    {
        yield return new WaitForSeconds(.5f);
        if (StartClose != null) StartClose.Invoke();
        yield return new WaitForSeconds(SlamTime);

        if (!HasSlammed)
        {
            //if(Slam) Slam.Play();
            Creak.Stop();
            Door.isKinematic = true;
            if (FinishClose != null) FinishClose.Invoke();
            HasSlammed = true;
        }
       


    }
    public void  DoSlam()
    {
        if (HasShut) return;
        HasShut = true;
        Door.isKinematic = false;
        StartCoroutine(DoSlamTimed());
        Creak.Play();
        HingeMotor = DoorHinge.motor;
        HingeMotor.targetVelocity = MotorForce;
        DoorHinge.motor = HingeMotor;
        DoorHinge.useMotor = true;
    }

}
