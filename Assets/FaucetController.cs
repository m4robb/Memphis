using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class FaucetController : MonoBehaviour
{
    public UnityEvent FaucetOpen;
    public UnityEvent FaucetClose;
    public Rigidbody RB;



    public float TwistMax;
    public float TapWillOpen;
    public float OpenedValue = 0;

    public InputDeviceCharacteristics HandType;

    Vector3 LocalRotation;
    Vector3 StartRotation;

    List<InputDevice> DeviceList = new List<InputDevice>();



    bool IsOpen, IsPointing, IsPointingValue;


    void LateUpdate()
    {
        OpenedValue = transform.localEulerAngles.y;
        LocalRotation = transform.localEulerAngles;


        if (OpenedValue < 0)
        {
            LocalRotation.y = 0;
        }

        if (OpenedValue > TapWillOpen && !IsOpen)
        {

            IsOpen = true;
            FaucetOpen.Invoke();
        }

        if (OpenedValue < TapWillOpen && IsOpen)
        {
            IsOpen = false;
            FaucetClose.Invoke();
        }

        if (OpenedValue > TwistMax)
        {
            LocalRotation.y = TwistMax;

        }

        transform.localEulerAngles = LocalRotation;

        if (DeviceList.Count == 0)
        {
            InputDeviceCharacteristics desiredCharacteristics = InputDeviceCharacteristics.HeldInHand | HandType | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, DeviceList);
            return;
        }

        foreach (var device in DeviceList)
        {
            IsPointing = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out IsPointingValue) && IsPointingValue;
        }

        if (!IsPointing) RB.isKinematic = true;
        if (IsPointing) RB.isKinematic = false;
    }

}
