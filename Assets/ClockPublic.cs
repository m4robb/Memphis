using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockPublic : MonoBehaviour
{

    int Counter = 0, LoopLength = 1;

    bool DoClickTrigger, IsLooping;

    public Transform SecondHand;

    Vector3 SecondHandAngle;

    public bool IsActive;

    public void DoClick()
    {
        Counter++;
        DoClickTrigger = true;
       
    }

    public void SetPlayState(bool _State)
    {
        IsActive = _State; 
    }

    private void OnDisable()
    {
        AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;
    }
    void Update()
    {
        if (AudioGridTick.AudioGridInstance != null && !IsLooping)
        {
            SecondHandAngle = SecondHand.localEulerAngles;
            AudioGridTick.AudioGridInstance.storedAudioAction += DoClick;
            IsLooping = true;

        }

        if (DoClickTrigger && Counter >= LoopLength && IsActive)
        {
            SecondHandAngle.z += 6;
            SecondHand.localEulerAngles = SecondHandAngle;
            Counter = 0;
            DoClickTrigger = false;

        }
    }
}
