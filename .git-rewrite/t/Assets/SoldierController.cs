using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoldierController : MonoBehaviour
{
    public UnityEvent ToggleTouchOn;
    public UnityEvent ToggleTouchOff;

    public float LifeSpan = 100;

    public bool AutoStop;

    public float Timer;

    bool IsActive;

    public void StopItem()
    {
        if (!IsActive) return;
        if (ToggleTouchOff != null) ToggleTouchOff.Invoke();
        IsActive = false;
    }

    public void TouchItem()
    {

        Timer = 0;

        if (!IsActive)
        {

           
            if (ToggleTouchOn != null) ToggleTouchOn.Invoke();
            IsActive = true;
            return;
        }
        else
        {
            if (ToggleTouchOff != null) ToggleTouchOff.Invoke();
            IsActive = false;
        }
    }

    private void Update()
    {

        Timer += Time.deltaTime;

        if(AutoStop && Timer > LifeSpan)
        {
            StopItem();
        }
    }
}
