using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioTrigger : MonoBehaviour
{

    public UnityEvent TriggerEnter;
    public UnityEvent TriggerExit;



    private void OnTriggerEnter(Collider other)
    {

        if (TriggerEnter != null)
        {
            Debug.Log("TRIGGGEERRR");
            TriggerEnter.Invoke();

        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (TriggerExit != null) TriggerExit.Invoke();
    }

}
