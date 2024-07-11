using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;




public class AudioLooperSFXFadeSingle : MonoBehaviour
{
    public AudioSource _AS;

    public AudioClip[] AudioClipArray;

    public double LoopLength = 1; // In beats

    public bool IsLinear;

    public bool Randomize;

    public double FadeTimeLength;



    float Timer = 0;

    int ClipIndex = 0;






    bool IsLooping;

    IEnumerator TimedVolumeChange(float _TargetVolume)
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());

        _AS.DOFade(_TargetVolume, 2 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);
       

    }
    public void VolumeChange(float _TargetVolume)
    {

        StartCoroutine(TimedVolumeChange(_TargetVolume));
    }


    int Counter = 0;

    private void OnEnable()
    {

        Timer = 0;
        Counter = (int)LoopLength;
        ClipIndex = 0;
        DoClickTrigger = false;
        _AS.volume = 1;
        IsLooping = false;
    }

    private void OnDisable()
    {
        AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;
    }

    public void DoClick()
    {
        Counter++;
        DoClickTrigger = true;
    }

    bool DoClickTrigger;

    void Update()
    {
        if(AudioGridTick.AudioGridInstance != null && !IsLooping)
        {

            AudioGridTick.AudioGridInstance.storedAudioAction += DoClick;

            IsLooping = true;

        }

        if (DoClickTrigger && Counter >= LoopLength )
        {
            _AS.volume = 1;

            if (FadeTimeLength != 0)
            {
             double _FadeTime = FadeTimeLength * 60 / AudioGridTick.AudioGridInstance.bpm;
            _AS.DOFade(0, (float)_FadeTime / 2).SetDelay((float)_FadeTime);
            }


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

            DoClickTrigger = false;
        }



 
    }
}
