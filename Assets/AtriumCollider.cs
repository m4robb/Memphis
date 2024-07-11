using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AtriumCollider : MonoBehaviour
{
    public Rigidbody Door;
    public HingeJoint DoorHinge;
    public GameObject Apt;
    public AudioSource Slam;
    public AudioSource Creak;
    public JointMotor  HingeMotor;
    public float MotorForce = -100;
    public UnityEvent Slammit;
    void Start()
    {
        
    }

    bool HasShut, HasSlammed;

    IEnumerator DoSlam()
    {
        yield return new WaitForSeconds(.5f);
 
        yield return new WaitForSeconds(2.8f);

        if (!HasSlammed)
        {
            Apt.SetActive(false);
            Slam.Play();
            if (Slammit != null) Slammit.Invoke();
            HasSlammed = true;
        }
       


    }
    private void OnTriggerEnter(Collider other)
    {
        if (HasShut) return;
        HasShut = true;
        StartCoroutine(DoSlam());
        Creak.Play();
        HingeMotor = DoorHinge.motor;
        HingeMotor.targetVelocity = MotorForce;
        DoorHinge.motor = HingeMotor;
        DoorHinge.useMotor = true;
    }

}
