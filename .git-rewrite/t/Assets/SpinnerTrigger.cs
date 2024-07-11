using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpinnerTrigger : MonoBehaviour
{

    public LayerMask layerMask;

    public Rigidbody RB;

    public ConstantForce CF;

    List<UnityEngine.XR.InputDevice> RightHandDevices;
    List<UnityEngine.XR.InputDevice> LeftHandDevices;

    Vector3 RightControllerVelocity;
    void Start()
    {


        RightHandDevices = new List<UnityEngine.XR.InputDevice>();
        LeftHandDevices = new List<UnityEngine.XR.InputDevice>();

    }

    private void Update()
    {

       

        if (TVelocity.y > 0) TVelocity.y -= Time.deltaTime * .1f;

        if (RightHandDevices.Count == 0)
        {
            var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, RightHandDevices);


        }

        if (LeftHandDevices.Count == 0)
        {
            var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, LeftHandDevices);
        }

        foreach (UnityEngine.XR.InputDevice device in LeftHandDevices)
        {

            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out RightControllerVelocity);
            
        }

    }


    //private void OnTriggerEnter(Collider other)
    //{
    //    if ((layerMask.value & (1 << other.gameObject.layer)) != 0)
    //    {
    //        XRBaseController XRBC = other.gameObject.GetComponentInParent<XRBaseController>();

    //        if (XRBC != null && RightControllerVelocity != null)
    //        {

    //            Vector3 TVelocity = RightControllerVelocity;
    //            TVelocity.x = 0;
    //            TVelocity.z = 0;
    //            Debug.Log(TVelocity);
    //            RB.AddTorque(TVelocity);
    //        }

    //    }
    //}


    //private void OnCollisionStay(Collision other)
    //{
    //    if ((layerMask.value & (1 << other.gameObject.layer)) != 0)
    //    {
    //        XRBaseController XRBC = other.gameObject.GetComponentInParent<XRBaseController>();

    //        if (XRBC != null && RightControllerVelocity != null)
    //        {

    //            Vector3 TVelocity = RightControllerVelocity;
    //            TVelocity.x = 0;
    //            TVelocity.y = 0;
    //            TVelocity.z *= -10;
    //            Debug.Log(TVelocity);
    //            RB.AddTorque(TVelocity);
    //        }

    //    }
    //}

    Vector3 TVelocity = Vector3.zero;

   

    private void OnCollisionStay(Collision other)
    {




        if ((layerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            XRBaseController XRBC = other.gameObject.GetComponentInParent<XRBaseController>();

            if (XRBC != null && RightControllerVelocity != null)
            {

                TVelocity = RightControllerVelocity;
                TVelocity.x = 0;
                TVelocity.z = 0;
                TVelocity.y *= 10;
                RB.AddTorque(TVelocity);
                //
            }

        }


    }
}
