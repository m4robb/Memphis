using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class ActionTriggerLayers : MonoBehaviour
{
    public UnityEvent TriggerEvent;
    public UnityEvent TriggerExitEvent;

    public LayerMask layerMask;

    public bool OneTime;

    public bool IsHaptic;

    public bool Trigger;

    private void OnCollisionEnter(Collision other)
    {




        if ((layerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            if (OneTime && Trigger) return;

            Trigger = true;

            if (TriggerEvent != null) TriggerEvent.Invoke();
            XRBaseController XRBC = other.gameObject.GetComponentInParent<XRBaseController>();
            if (XRBC  && IsHaptic)
            {
                XRBC.SendHapticImpulse(.5f, .25f);
            }
        }

           
    }

    public void RemoteTrigger()
    {
        if (TriggerEvent != null) TriggerEvent.Invoke(); 
    }

    private void OnCollisionExit(Collision other)
    {
        if ((layerMask.value & (1 << other.gameObject.layer)) != 0) 

            if (TriggerEvent != null) TriggerExitEvent.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {



        if ((layerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            if (OneTime && Trigger) return;

            Trigger = true;

            if (TriggerEvent != null) TriggerEvent.Invoke();
            XRBaseController XRBC = other.gameObject.GetComponentInParent<XRBaseController>();
            if (XRBC && IsHaptic)
            {
                XRBC.SendHapticImpulse(.5f, .25f);
            }
        }

           
    }

    private void OnTriggerExit(Collider other)
    {
        if ((layerMask.value & (1 << other.gameObject.layer)) != 0)


            if (TriggerEvent != null) TriggerExitEvent.Invoke();

    }

}
