using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TitlePlacer : MonoBehaviour
{
    Transform MainCamera;

    public float Duration;

    public UnityEvent OnTitleClose;

    float Timer = 0;

    void Start()
    {
        MainCamera = Camera.main.transform;
    }


    bool TimerTrigger;
     
    void Update()
    {

        if (OnTitleClose != null)
        {
            Timer += Time.deltaTime;

            if(Timer > Duration)
            {
                TimerTrigger = true;
                OnTitleClose.Invoke();
            }
        }

        Vector3 NewPosTarget = MainCamera.position + MainCamera.forward * 20;
        Vector3 NewPos = Vector3.Lerp(transform.position, NewPosTarget, Time.deltaTime * 1);
        transform.position = NewPos;
        transform.LookAt(MainCamera.position);
    }
}
