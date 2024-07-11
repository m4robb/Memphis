using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{


    public string DoorID;

    Rigidbody RB;

    public HingeJoint HJ;

    AudioSource Creak;

    float OpeningSpeed;

    bool IsCreaking;

    void Start()
    {
        HJ = GetComponent<HingeJoint>();
        RB = GetComponent<Rigidbody>();
        Creak = GetComponent<AudioSource>();
        Creak.enabled = false;
    }

    public void CloseDoor()
    {
        HJ.useMotor = true;
    }


    void Update()
    {
        if (Mathf.Abs(HJ.angle) < 1) HJ.useMotor = false;

        if (Mathf.Abs(HJ.angle) < 5)
        {
            Creak.volume = 0;
            Creak.Pause();
            return;
        }

        OpeningSpeed = Mathf.Round(RB.angularVelocity.magnitude * 1000f) / 1000f;

        if(OpeningSpeed > 0 && !IsCreaking)
        {
            IsCreaking = true;
            Creak.enabled = true;
            Creak.Play();
        }

        if (OpeningSpeed == 0)
        {
            IsCreaking = false;
            Creak.enabled = false;
            Creak.Pause();
        }
        Creak.volume = Mathf.Clamp(OpeningSpeed, 0f, 1f);

    }
}
