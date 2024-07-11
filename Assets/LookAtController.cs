using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LookAtController : MonoBehaviour
{
    public UnityEvent LookToward;
    public UnityEvent LookAway;

    public float Threshold = 45;
    public Camera MainCamera;

    public float _Angle;

    bool Trigger;

    void Update()
    {
        //if (!MainCamera)
        //{
        //    MainCamera = Camera.main.transform;
        //    return;
        //}


        Vector3 targetDir = transform.position - MainCamera.transform.position;


        _Angle = Vector3.Angle(targetDir, MainCamera.transform.forward);

        Debug.Log(_Angle);

        if(!Trigger && _Angle < Threshold)
        {
            Trigger = true;
            if (LookToward != null) LookToward.Invoke();
        }

        if (Trigger && _Angle > Threshold)
        {
            Trigger = false;
            if (LookAway != null) LookAway.Invoke();
        }
    }
}
