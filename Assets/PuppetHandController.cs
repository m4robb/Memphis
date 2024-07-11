using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

using DG.Tweening;
//using UnityEngine.InputSystem;



public class PuppetHandController : MonoBehaviour
{

    List<InputDevice> RightDeviceList = new List<InputDevice>();
    List<InputDevice> LeftDeviceList = new List<InputDevice>();
    public Animator PuppetController;
    public XRPuppetInteractable XRPI;

    public Transform HangUpPoint;
    public Transform ConnectionPoint;
    public Transform PickUpPoint;

    public UnityEvent TriggerPulled;
    public UnityEvent TriggerReleased;
    public UnityEvent Signalled;

    public UnityEngine.InputSystem.InputActionReference TriggerValueRight;
    public UnityEngine.InputSystem.InputActionReference TriggerValueLeft;

    public bool IsRightHand = true;
    public bool IsPuppet;
    bool IsOpened, RHHasOpened, LHHasOpened;

    Vector3 StartPos;
    Vector3 StartRotation;


    public void ActivatePuppet(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor _Interactor)
    {
       
    }

    public void SlidePuppetOn()
    {
        XRPI.IsPuppet = true;
    }

    public void Start()
    {
        StartPos = transform.localPosition;
        StartRotation = transform.localEulerAngles;

        if (!XRPI) XRPI = gameObject.GetComponent<XRPuppetInteractable>();
    }

    void Update()


    {

        if (RightDeviceList.Count == 0)
        {
            Debug.Log("left");
            InputDeviceCharacteristics desiredCharacteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, RightDeviceList);
            return;
        }

        if (LeftDeviceList.Count == 0)
        {
            Debug.Log("right");
            InputDeviceCharacteristics desiredCharacteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, LeftDeviceList);
            return;
        }

        float PressValue = 0;

        if (RightDeviceList.Count > 0 && XRPI.IsPuppet && IsRightHand)
        {
           
            foreach (var device in RightDeviceList)
            {
                if (device.TryGetFeatureValue(CommonUsages.trigger, out float _TriggerValue))
                {
                    PressValue = _TriggerValue;
                    Debug.Log(_TriggerValue);
                }

            }



            PuppetController.SetFloat("MotionTime", PressValue);

            if(PressValue == 1 && !IsOpened)
            {
                IsOpened = true;
                if (TriggerPulled != null && RHHasOpened) TriggerPulled.Invoke();
            }

            if (PressValue < .1f && !XRPI.IsHovering)
            {
                Collider[] hitColliders = Physics.OverlapSphere(ConnectionPoint.position, .3f);
                RHHasOpened = true;
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.transform == HangUpPoint)
                    {

                        //Debug.Log("Drop in Hook");
                        //XRPI.IsPuppet = false;
                        //IsPuppet = false;
                        //gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        //transform.DOLocalMove(StartPos, 1f);
                        //transform.DOLocalRotate(StartRotation, 1f).OnComplete(() =>
                        //{
                        //});
                        //XRPI.DropPuppet();
                        ////transform.DORotateQuaternion(StartRotation, .5f);
                    }
                }
            }

            if (PressValue < .1f && IsOpened)
            {
                IsOpened = false;
                if (TriggerReleased != null) TriggerReleased.Invoke();
            }
            return;

        }

        if (LeftDeviceList.Count > 0 && XRPI.IsPuppet && !IsRightHand)
        {

            foreach (var device in LeftDeviceList)
            {
                if (device.TryGetFeatureValue(CommonUsages.trigger, out float _TriggerValue))
                {
                    PressValue = _TriggerValue;
                }


            }

            if (PressValue == 1 && !IsOpened && LHHasOpened)
            {
                IsOpened = true;
                if (TriggerPulled != null) TriggerPulled.Invoke();
            }

            if (PressValue < .1f && !XRPI.IsHovering)
            {
                Collider[] hitColliders = Physics.OverlapSphere(ConnectionPoint.position, .3f);
                LHHasOpened = true;
                foreach (var hitCollider in hitColliders)
                {
                    //if (hitCollider.transform == HangUpPoint)
                    //{

                    //    Debug.Log(HangUpPoint);
                    //    XRPI.IsPuppet = false;
                    //    IsPuppet = false;
                    //    gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    //    transform.DOLocalMove(StartPos, 1f);
                    //    transform.DOLocalRotate(StartRotation, 1f);
                    //    XRPI.DropPuppet();
                    //}
                }
            }

            if (PressValue < .1f && IsOpened)
            {
                IsOpened = false;
                if (TriggerReleased != null) TriggerReleased.Invoke();
            }

            PuppetController.SetFloat("MotionTime", PressValue);
            return;
           
        }

        if (TriggerValueRight!= null && XRPI.IsPuppet)
        {
 
            if (IsRightHand)
            PressValue = TriggerValueRight.action.ReadValue<float>();
            else
            {
                PressValue = TriggerValueLeft.action.ReadValue<float>();
            }
            if (PressValue == 0) return;
            PuppetController.SetFloat("MotionTime", PressValue);
        }


    }
}
