using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;

public class CheckHMD : MonoBehaviour
{

    public UnityEvent HMDEvent;
 
    List<UnityEngine.XR.InputDevice> HeadDevices;

    bool HMDOn, HMDOnTrigger, HeadTriggerValue;

    private void Start()
    {

        HeadDevices = new List<UnityEngine.XR.InputDevice>();
    }

    void Update()
    {
        if (HeadDevices.Count == 0)
        {
            HeadDevices = new List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.Head, HeadDevices);
        }

        foreach (UnityEngine.XR.InputDevice device in HeadDevices)
        {
            HMDOn = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out HeadTriggerValue) && HeadTriggerValue;
        }

        if (HMDOn && !HMDOnTrigger)
        {
           if(HMDEvent != null)
            {
                HMDOnTrigger = true;
                HMDEvent.Invoke();
            }
        }
    }
}
