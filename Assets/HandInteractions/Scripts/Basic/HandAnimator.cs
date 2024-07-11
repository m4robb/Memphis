using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public struct FingerGripValues
{
    public float Thumb;
    public float IndexV;
    public float Middle;
    public float Ring;
    public float Pinky;
}

public class HandAnimator : MonoBehaviour
{

    public float Speed = 5.0f;
    public  InputDevice Controller;
    public InputDeviceCharacteristics HandType;
    public float GripModifier = 1f;

    private Rigidbody RB;

   public Animator AnimatorController = null;

    private readonly List<Finger>StartFingers = new List<Finger>()
    {
        new Finger(FingerType.Index),
        new Finger(FingerType.Thumb),
        new Finger(FingerType.Middle),
        new Finger(FingerType.Ring),
        new Finger(FingerType.Pinky)
    };

    private readonly List<Finger> GripFingers = new List<Finger>()
    {
        new Finger(FingerType.Index),
        new Finger(FingerType.Thumb),
        new Finger(FingerType.Middle),
        new Finger(FingerType.Ring),
        new Finger(FingerType.Pinky)
    };

    private readonly List<Finger> PointFingers = new List<Finger>()
    {
        new Finger(FingerType.Middle),
        new Finger(FingerType.Ring),
        new Finger(FingerType.Pinky),
        new Finger(FingerType.Thumb)
    };

    bool IsPointing, IsGripping;

    private void Awake()
    {
       
        RB = GetComponent<Rigidbody>();
        //StartFingerPosition(StartFingers);
    }

    List<InputDevice> DeviceList = new List<InputDevice>();

    private void Update()
    {


        if (AnimatorController == null)
        {
            AnimatorController = GetComponentInChildren<Animator>();
            StartFingerPosition(StartFingers);
        }

        if (DeviceList.Count == 0)
        {
            InputDeviceCharacteristics desiredCharacteristics =InputDeviceCharacteristics.HeldInHand | HandType | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, DeviceList);
            return;
        }

        //CheckGrip();
        CheckTrigger();

        //if (IsPointing)
        //{
        //SmoothFinger(PointFingers);
        //AnimateFinger(PointFingers);
        //}

        if (IsGripping)
        {
            SmoothFinger(GripFingers);
            AnimateFinger(GripFingers);
        }

    }

    private void CheckTrigger()
    {
        foreach (var device in DeviceList)
        {
            if (device.TryGetFeatureValue(CommonUsages.trigger, out float _GripValue))
            {
               // Debug.Log(_GripValue);
                //if (_GripValue < .001f)
                //{
                //    IsGripping = false;
                //    return;
                //}
                IsGripping = true;
                SetFingerTargets(GripFingers, _GripValue);
                if (_GripValue > 0)
                    RB.isKinematic = true;
                else
                    RB.isKinematic = false;
            }  
           
        }

    }

    private void CheckGrip()
    {
        foreach (var device in DeviceList)
        {
            if (device.TryGetFeatureValue(CommonUsages.grip, out float _PointerValue))
            {
                if (_PointerValue < .001f)
                {
                    IsGripping  = false;
                    return;
                }
                IsGripping = true;
                SetFingerTargets(GripFingers, _PointerValue);
                if (_PointerValue > 0)
                    RB.isKinematic = true;
                else
                    RB.isKinematic = false;
                // SetFingerTargets(PointFingers, _PointerValue);
            } 
                
        }
    }

    private void SetFingerTargets(List<Finger> fingers, float value)
    {
        
        foreach(Finger F in fingers)
        {
           
            F.Target = value;
        }
    }

    private void SmoothFinger(List<Finger> fingers)
    {
        foreach (Finger F in fingers)
        {
            float _Time = Speed * Time.unscaledDeltaTime;
            F.Current = Mathf.MoveTowards(F.Current, F.Target, _Time);
        }
        }

    private void AnimateFinger(List<Finger> fingers)
    {
        foreach (Finger F in fingers)
            AnimateFinger(F.Type.ToString(), F.Current);
    }

    private void StartFingerPosition(List<Finger> fingers)
    {
       
        foreach (Finger F in fingers)
        AnimatorController.SetFloat(F.Type.ToString(), 0);
    }

    private void AnimateFinger(string finger, float blend)
    {
 
        AnimatorController.SetFloat(finger, blend);
    }
}