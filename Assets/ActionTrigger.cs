using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionTrigger : MonoBehaviour
{
    public UnityEvent TriggerEvent;
    public UnityEvent TriggerExitEvent;
    public string TagName ="";

    private void OnTriggerEnter(Collider other)
    {
        if (TagName != "" && TagName != other.transform.tag) return;

        if (TriggerEvent != null) TriggerEvent.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (TagName != "" && TagName != other.transform.tag) return;

        if (TriggerEvent != null) TriggerExitEvent.Invoke();
    }
}
