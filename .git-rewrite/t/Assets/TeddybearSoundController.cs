using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TeddybearSoundController : MonoBehaviour
{

    public UnityEvent ActivateTeddybear;
    public Transform CameraPosition;

    bool Trigger;
    void Update()


        
    {

  
        if (Vector3.Distance(transform.position, CameraPosition.position) < .4f && !Trigger)
        {

            Debug.Log("Booooooooooooooooooooooooooooooo");
            ActivateTeddybear.Invoke();
            Trigger = true;
        }
    }
}
