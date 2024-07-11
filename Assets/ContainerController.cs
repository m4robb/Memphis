using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Events;

public class ContainerController : MonoBehaviour
{

    public float PourAngle = -45;
    public UnityEvent OnPourStart;
    public UnityEvent OnPourEnd;

    Vector3 StartAngles;

    private void Start()
    {
        StartAngles = transform.up;
    }

    bool PourToggle;

    void Update()
    {


        Vector3 CurrentDelta = StartAngles - transform.up;

   

        if (CurrentDelta.magnitude > 1 && !PourToggle)
        {
            PourToggle = true;
            if (OnPourStart != null) OnPourStart.Invoke();
        }
        if (CurrentDelta.magnitude < 1 && PourToggle)
        {
            PourToggle = false;
            if (OnPourEnd != null) OnPourEnd.Invoke();
        }
    }
}
