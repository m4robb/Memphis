using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFXLooper : MonoBehaviour
{

    public AudioSource _AS;

    public AudioClip   Clip;

    public int Repeat;

    int Counter = 0;

    bool DoClickTrigger;

    public void DoClick()
    {
        Counter++;
        DoClickTrigger = true;

    }
    void Update()
    {
        //if (AudioGridTick.AudioGridInstance != null && !IsLooping)
        //{

        //    AudioGridTick.AudioGridInstance.storedAudioAction += DoClick;

        //    if (NumberOfLoops != 0 && FadeOut)


        //    {
        //        double _FadeTime = (NumberOfLoops + 1) * 60 / AudioGridTick.AudioGridInstance.bpm * LoopLength;
        //        _AS.DOFade(0, (float)_FadeTime);
        //    }
        //    IsLooping = true;

        //}
    }
}
