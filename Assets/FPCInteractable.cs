using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]

public class FPCInteractable : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody RB;

    void Start()
    {
        RB = GetComponent<Rigidbody>();
        StoredAngles = transform.eulerAngles;
    }


    Vector3 StoredAngles = Vector3.zero;

    bool ReachedHoldPosition;

    public void DropObject()
    {
        ReachedHoldPosition = false;
    }

    public void HoldObject(FPSLookGrab _FPSLG)
    {

        _FPSLG.RightHand.position = transform.position;

        if (!ReachedHoldPosition)
        {

            float TriggerPressedValue = _FPSLG.SelectTrigger.action.ReadValue<float>();

            transform.eulerAngles = Vector3.zero;

            RB.MovePosition(Vector3.Lerp(_FPSLG.ReturnToPosition.position, _FPSLG.HoldPosition.position, TriggerPressedValue));

            if (TriggerPressedValue == 1) ReachedHoldPosition = true;
        }

        Vector3 ThisAngles = transform.eulerAngles;
        if(StoredAngles != Vector3.zero)
        {
 
            Vector3 ControllerInertia = StoredAngles - _FPSLG.ControllerRotation;
            
            ThisAngles += ControllerInertia;


        }

#if UNITY_EDITOR

        ThisAngles.x = 0;
        ThisAngles.z = 0;
#endif
        transform.eulerAngles = ThisAngles;

        StoredAngles = _FPSLG.ControllerRotation;


    }

}
