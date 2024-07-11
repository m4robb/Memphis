using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuppetPickUpActions : MonoBehaviour
{
    public UnityEvent OnPickMeUp;
    public UnityEvent OnPutMeDown;

    public bool RepeatActions;

    public void PickMeUP()
    {
        if (OnPickMeUp != null) OnPickMeUp.Invoke();
    }

    bool HasPutDown;

    public void PutMeDown()
    {
        if (OnPutMeDown != null && !HasPutDown)
        {
            OnPutMeDown.Invoke();
            if(!RepeatActions) HasPutDown = true;
        }
    }
}
