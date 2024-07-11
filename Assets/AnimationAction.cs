using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationAction : MonoBehaviour
{
    public UnityEvent AnimationEvent;
    public UnityEvent AnimationEventFinal;


    public void ExecuteAnimationEvent()
    {
        if(AnimationEvent != null)
        {
            AnimationEvent.Invoke();
        }
    }

    public void ExecuteAnimationEventFinal()
    {
        if (AnimationEventFinal != null)
        {
            AnimationEventFinal.Invoke();
        }
    }
}
