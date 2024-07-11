using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonkeyController : MonoBehaviour
{
    public UnityEvent LookToward;
    public UnityEvent LookAway;

    public float Threshold = 45;
    Transform MainCamera;

    bool Trigger;

    void Update()
    {
        if (!MainCamera)
        {
            MainCamera = Camera.main.transform;
            return;
        }
        float _Angle = Vector3.Angle(transform.position, MainCamera.forward);

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
