using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class MerryGoRoundController : MonoBehaviour
{
   public Rigidbody MerryGoRoundRB;
   public GameObject PunchGroup;
   public AudioSource Creeak;
    public UnityEvent OnSpin;


    bool Trigger;
    Camera MainCamera;
    void Start()
    {
        MainCamera = Camera.main;
    }


void FixedUpdate()
    {

        Creeak.volume = Mathf.Abs(MerryGoRoundRB.angularVelocity.magnitude);

        if (Mathf.Abs(MerryGoRoundRB.angularVelocity.magnitude) > 0.03f && !Trigger)
        {

            Trigger = true;
            //PunchPosition = MainCamera.transform.position + MainCamera.transform.forward * .5f;
            //PunchPosition.y = 5.53f;
            //PunchGroup.transform.position = PunchPosition;
            if (OnSpin != null)
            {
                OnSpin.Invoke();
            }
            //PunchGroup.SetActive(true);
        }
    }
}
