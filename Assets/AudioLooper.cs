using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;




public class AudioLooper : MonoBehaviour
{
    public AudioSource _AS;

    public AudioClip[] AudioClipArray;

    public double LoopLength = 1; // In beats

    public bool IsLinear;

    public bool Randomize;

    public bool FadeOut;

    public int NumberOfLoops = 0;

    public UnityEvent LoopAction;
    public UnityEvent EachLoopAction;

    float Timer = 0;

    

    int ClipIndex = 0;
    int NumberOfLoopsCounter = 0;

    double StartLoopLength = 0;




    bool DoLoop = true, IsLooping, Trigger;

    IEnumerator TimedVolumeChange(float _TargetVolume)
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());

        _AS.DOFade(_TargetVolume, 2 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);
       

    }

    IEnumerator TimedKillLoop()
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());

        AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;

        _AS.enabled = false;




    }
    public void VolumeChange(float _TargetVolume)
    {

        StartCoroutine(TimedVolumeChange(_TargetVolume));
    }

    public void QuickFade()
    {
        _AS.DOFade(0, 0.01379999f);
    }

    public void KillLoop()
    {
        StartCoroutine(TimedKillLoop());
    }


    int Counter = 0;

    public void DoClick()
    {
        Counter++;
        DoClickTrigger = true;

    }

    bool DoClickTrigger;

    private void OnDisable()
    {
        IsLooping = false;
        AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;
        if (AudioGridTick.AudioGridInstance.AudioLoops.IndexOf(this) != -1)
            AudioGridTick.AudioGridInstance.AudioLoops.Remove(this);
    }

    void Update()
    {
        if(AudioGridTick.AudioGridInstance != null && !IsLooping)
        {
        
            AudioGridTick.AudioGridInstance.storedAudioAction += DoClick;

            AudioGridTick.AudioGridInstance.AudioLoops.Add(this);


            StartLoopLength = LoopLength;

            if (NumberOfLoops != 0 && FadeOut)

               
            {
                 double _FadeTime = (NumberOfLoops + 1) * 60 / AudioGridTick.AudioGridInstance.bpm * LoopLength;
                _AS.DOFade(0, (float)_FadeTime);
            }
            IsLooping = true;

        }

        if (DoClickTrigger && Counter >= LoopLength )
        {
            if (LoopAction != null) LoopAction.Invoke();

            if (!IsLinear)
            {
                ClipIndex = Random.Range(0, AudioClipArray.Length);
                _AS.PlayOneShot(AudioClipArray[ClipIndex]);
            }

            else
            {
                _AS.PlayOneShot(AudioClipArray[ClipIndex]);
                ClipIndex++;
                if (ClipIndex > AudioClipArray.Length - 1) ClipIndex = 0;
            }

            Counter = 0;

            if(Randomize)
            {
                int Variance = Random.Range((int)StartLoopLength, (int)StartLoopLength * 2);


                LoopLength =   (double)Variance;
            }

            if (NumberOfLoops != 0)
            {
                NumberOfLoopsCounter++;

                if(NumberOfLoopsCounter >= NumberOfLoops)
                {
                    AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;
                    gameObject.SetActive(false);
                }
            }

            DoClickTrigger = false;
        }



 
    }
}
