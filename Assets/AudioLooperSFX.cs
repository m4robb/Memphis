using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;




public class AudioLooperSFX : MonoBehaviour
{
    public AudioSource _AS;

    public AudioClip[] AudioClipArray;

    public double LoopLength = 1; // In beats

    public bool IsLinear;

    public bool Randomize;

    public bool FadeOut;

    public int NumberOfLoops = 0;

    public float StartVolume = 1;

    float Timer = 0;

    int ClipIndex = 0;

    int NumberOfLoopsCounter = 0;

    double BaseLoopLength;

    public int Delay = 0;

    public bool DestroyAfterLoop;

    bool IsStopped;




    bool IsLooping;

    IEnumerator TimedVolumeChange(float _TargetVolume)
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());

        _AS.DOFade(_TargetVolume, 2 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);
       

    }

    public void QuickFade()
    {
        _AS.DOFade(0, 30 / (float)AudioGridTick.AudioGridInstance.bpm);
    }
    public void VolumeChange(float _TargetVolume)
    {

        StartCoroutine(TimedVolumeChange(_TargetVolume));
    }


    int Counter = 0;

    private void OnEnable()
    {
        IsStopped = false;
        Timer = 0;
        NumberOfLoopsCounter = 0;
        Counter = (int)LoopLength;
        ClipIndex = 0;
        DoClickTrigger = false;
        if(_AS)
          _AS.volume = StartVolume;
        IsLooping = false;
    }

    public void StopLoop()
    {
        IsStopped = true;
    }

    public void StartLoop()
    {
        IsStopped = false;
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

    public void SwitchOutAudioClip(AudioClip audioClip)
    {
        if (AudioClipArray.Length > 1) return;
        AudioClipArray[0] = audioClip;
    }

    void Update()
    {

        if (!_AS) return;

        if(AudioGridTick.AudioGridInstance != null && !IsLooping)
        {
            _AS.volume = StartVolume;

            AudioGridTick.AudioGridInstance.storedAudioAction += DoClick;

            if (NumberOfLoops != 0 && FadeOut)

               
            {
                 double _FadeTime = (NumberOfLoops + 1) * 60 / AudioGridTick.AudioGridInstance.bpm * LoopLength;
                _AS.DOFade(0, (float)_FadeTime);
            }
            BaseLoopLength = LoopLength;

            IsLooping = true;

        }

        if (DoClickTrigger && Counter >= LoopLength + Delay )
        {
            if (IsStopped)
            {
                this.enabled = false;
                return;
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


            if (Randomize)
            {
                LoopLength = Mathf.Floor(Random.Range((float)LoopLength, (float)LoopLength * 2));
            }
            Counter = 0;
            Delay = 0;

            if (NumberOfLoops != 0)
            {
                NumberOfLoopsCounter++;

                if(NumberOfLoopsCounter >  NumberOfLoops)
                {
                  
                    gameObject.SetActive(false);
                    if (DestroyAfterLoop) Destroy(gameObject);
                }
            }

            DoClickTrigger = false;
        }



 
    }
}
