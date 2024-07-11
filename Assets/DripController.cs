using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DripController : MonoBehaviour
{
    public AudioSource _AS;

    public AudioClip[] AudioClipArray;

    float Timer = 0;
    int ClipIndex = 0;

    public double LoopLength = 1; // In beats

    bool DoLoop = true, IsLooping;

    public void StartLoop(int _ClipIndex, float _loopLength)
    {
        LoopLength = _loopLength;
        DoLoop = true;
        StartCoroutine(Loop(AudioGridTick.AudioGridInstance.GetDelayTime())); // start loop ASAP
    } 
    public void StopLoop()
    {
        DoLoop = false;
    }

    IEnumerator Loop(double _Delay)
    {
        yield return new WaitForSeconds((float) _Delay);
        _AS.PlayOneShot(AudioClipArray[0]);
        if(DoLoop) StartCoroutine(Loop(AudioGridTick.AudioGridInstance.GetDelayTime()* LoopLength));

    }
    void Update()
    {
        if(AudioGridTick.AudioGridInstance != null && !IsLooping)
        {
            IsLooping = true;
            StartLoop(0, 1);
        }

 
    }
}
