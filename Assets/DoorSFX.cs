
using UnityEngine;
using UnityEngine.Events;

public class DoorSFX : MonoBehaviour
{


    public string DoorID;

    public Rigidbody RB;

    public HingeJoint HJ;

    public AudioSource Creak;

    public UnityEvent OnOpenDoor;
    public UnityEvent OnCloseDoor;

    float OpeningSpeed;

    bool IsCreaking, HasOpened;



    public void CloseDoor()
    {
        HJ.useMotor = true;
    }

    bool DoorHasHasClosed;
    void Update()
    {

        if (Mathf.Abs(HJ.angle) == 0 && IsCreaking && OnCloseDoor != null && !DoorHasHasClosed)
        {
            DoorHasHasClosed = true;
            OnCloseDoor.Invoke();
        }

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
            
            if (OnOpenDoor != null && !HasOpened)
            {
                Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                HasOpened = true;
                OnOpenDoor.Invoke();
            }


            IsCreaking = true;
            Creak.enabled = true;
            Creak.Play();
        }

        if (OpeningSpeed == 0)
        {
            IsCreaking = false;
            Creak.enabled = false;
            Creak.Stop();
        }

        Creak.pitch = Mathf.Clamp(OpeningSpeed + 0.5f, 0f, 1f);
        Creak.volume = Mathf.Clamp(OpeningSpeed, 0f, 1f);

    }
}
