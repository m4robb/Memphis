using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class CharacterControllerFloater : MonoBehaviour
{

    List<UnityEngine.XR.InputDevice> RightHandDevices;
    List<UnityEngine.XR.InputDevice> LeftHandDevices;

    public CharacterController CC;

    public Transform RHandTransform;
    public Transform LHandTransform;
    public Transform CenterHandTransform;

    Vector3 CCCurrentVelocity, CCStoredVelocity;

    float Speed = 0;

    Vector3 StoredRightHandPosition, StoredPosition;

    void Awake()
    {

        RightHandDevices = new List<UnityEngine.XR.InputDevice>();
        LeftHandDevices = new List<UnityEngine.XR.InputDevice>();
        //CCStoredVelocity = CC.velocity;

        StoredRightHandPosition = RHandTransform.position;
        StoredPosition = transform.position;
    }


    float Multix = 1;

    //private void LateUpdate()
    //{
    //    if (Speed > 0.0f) Speed -= Speed/2;
    //    if (Speed < 0.0f) Speed = 0;

       
    //}

    float InertiaStore ;

    Vector2 RightJoystickValue;

    
    private void LateUpdate()
    {

        //CenterHandTransform.position = Vector3.Lerp(RHandTransform.position, LHandTransform.position, 0.5f);

        if (RightHandDevices.Count == 0)
        {
            RightHandDevices = new List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, RightHandDevices);
        }
        else
        {

        }

        if (LeftHandDevices.Count == 0)
        {
            LeftHandDevices = new List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, LeftHandDevices);
        }

        bool LeftTriggerIsPulled = false, RightTriggerIsPulled = false, RightTriggerValue = false, LeftTriggerValue = false;

        foreach (UnityEngine.XR.InputDevice device in LeftHandDevices)
        {
            LeftTriggerIsPulled = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out LeftTriggerValue) && LeftTriggerValue;
        }


        Vector2 RightJoystickTest = new Vector2(0, 0);

        foreach (UnityEngine.XR.InputDevice device in RightHandDevices)
        {

           device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out RightJoystickValue);
            RightTriggerIsPulled = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out RightTriggerValue) && RightTriggerValue;
        }

        Vector3 CalcVelocity = Vector3.zero;

        if (RHandTransform.localEulerAngles.z >= 250) CalcVelocity = (RHandTransform.position - StoredRightHandPosition) / Time.deltaTime;
        else
            CalcVelocity = (StoredRightHandPosition - RHandTransform.position) / Time.deltaTime;


        //if (RightTriggerIsPulled) InertiaStore = 1f;
        //if (LeftTriggerIsPulled) InertiaStore = 1f;


        
        if(Mathf.Abs(RightJoystickValue.x)  < .5f && Mathf.Abs(RightJoystickValue.y)  < .5f)
        {
              if (InertiaStore > 0) InertiaStore -= Time.deltaTime * .5f;
             //CCStoredVelocity = transform.position - StoredPosition ;
            CC.Move(CCStoredVelocity * Time.deltaTime * InertiaStore);
            
        } else
        {

            CCStoredVelocity = CC.velocity;
            InertiaStore = 1f;
        }


        //CC.Move(CC.velocity * Time.deltaTime);




        

        








    }

}
