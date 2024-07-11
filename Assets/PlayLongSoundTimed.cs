using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;
using Klak.Wiring;

public class PlayLongSoundTimed : MonoBehaviour
{
    public AudioSource AS;

    public bool AutoStart;

    public bool OneTime;

    public bool IsPersistent;

    public float DelayBeat;

    public UnityEvent AudioBegin;
    public UnityEvent AudioComplete;
    public float VolumeChangeModifier = 1;

    bool CanMakeNoise;

    double NextEventTime;

    float TimeElapsed = 0;

    float Interval;

    bool Trigger;

    private Tween FadeTween;


    public void TransistionFade(float _Duration)
    {
        AS.DOFade(0, _Duration);
    }

    IEnumerator TimedEnding(float _Decay = .25f)
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        FadeTween.Kill();
        FadeTween = AS.DOFade(0, _Decay * 60 / (float)AudioGridTick.AudioGridInstance.bpm).OnComplete(() =>
        {
            AS.Stop();
            TimeElapsed = AS.time;
            AS.enabled = false;
            if (OneTime) CanMakeNoise = false;
        });
    }

    IEnumerator TimedEndingSharp(float _Decay = .25f)
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        AS.Stop();
        TimeElapsed = 0;
 

    }
    public void EndLongSound()
    {
        StartCoroutine(TimedEnding((float)AudioGridTick.AudioGridInstance.GetDelayTime()));
    }

    public void EndLongSoundSharp()
    {
        StartCoroutine(TimedEndingSharp((float)AudioGridTick.AudioGridInstance.GetDelayTime()));
    }

    public void EndLongSoundLong(float _Decay)
    {
        StartCoroutine(TimedEnding(_Decay));
    }

    IEnumerator TimedVolumeChange(float _TargetVolume)
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        FadeTween.Kill();
        FadeTween = AS.DOFade(_TargetVolume, VolumeChangeModifier *  2 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);


    }
    public void VolumeChange(float _TargetVolume)
    {

        StartCoroutine(TimedVolumeChange(_TargetVolume));
    }
    public void QuickFade()
    {
        FadeTween.Kill();
        FadeTween =  AS.DOFade(0, .01379999f);
    }


    int Counter = 0;

    private void OnDisable()
    {
        AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;
        AudioGridTick.AudioGridInstance.LongSounds.Remove(this);
    }

    public void DoClick()
    {


        Counter++;
        DoClickTrigger = true;
    }

    bool DoClickTrigger;



    IEnumerator DoAudioBegin()
    {

        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime() + Interval * DelayBeat);

        TriggerComplete = false;



        AudioBegin.Invoke();
    }

    public void PlayLongSoundFade(float _Fade)
    {
        if (AS.isPlaying) return;
        if (!CanMakeNoise) return;
        FadeTween.Kill();
        AS.enabled = true;
        AS.volume = 0;
        AS.DOFade(1, Interval * _Fade).SetDelay((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        if (TimeElapsed > AS.clip.length) TimeElapsed = 0;
        AS.time = TimeElapsed;


        AS.PlayDelayed((float)AudioGridTick.AudioGridInstance.GetDelayTime() + Interval * DelayBeat);

       //if (AudioBegin != null) StartCoroutine(DoAudioBegin());
        if (OneTime) CanMakeNoise = false;
    }


    public void PlayLongSound()
    {
        if (AS.isPlaying) return;
        if (!CanMakeNoise) return;

        FadeTween.Kill();
        AS.enabled = true;
        AS.DOFade(1, Interval).SetDelay((float)AudioGridTick.AudioGridInstance.GetDelayTime());
       
        if (TimeElapsed > AS.clip.length) TimeElapsed = 0;
        AS.time = TimeElapsed;

        AS.PlayDelayed((float)AudioGridTick.AudioGridInstance.GetDelayTime() + Interval * DelayBeat);

       // if (AudioBegin != null) StartCoroutine(DoAudioBegin());

        if (OneTime) CanMakeNoise = false;
    }


    public void ResetTriggers()
    {
        AS.enabled = false;
        AS.time = 0;
        TriggerComplete = false;
        TriggerBegin = false; 
    }

    public void PlayLongSoundVarVolume(float _FadeVolume)
    {
        if (AS.isPlaying) return;
        if (!CanMakeNoise) return;

        FadeTween.Kill();
        AS.enabled = true;
        AS.DOFade(_FadeVolume, Interval).SetDelay((float)AudioGridTick.AudioGridInstance.GetDelayTime());

        if (TimeElapsed > AS.clip.length) TimeElapsed = 0;
        AS.time = TimeElapsed;


        AS.PlayDelayed((float)AudioGridTick.AudioGridInstance.GetDelayTime() + Interval * DelayBeat);

        if (AudioBegin != null) StartCoroutine(DoAudioBegin());

        if (OneTime) CanMakeNoise = false;
    }

    bool TriggerComplete, TriggerBegin, TriggerPersistent;

    void Update()
    {


        if (IsPersistent && FallSceneManager.FallSceneManagerInstance && !TriggerPersistent)
        {
            TriggerPersistent = true;
            FallSceneManager.FallSceneManagerInstance.AddAudioToPersistent(this);

        }


        if (AudioBegin != null && !TriggerBegin && AS.time > 0)
        {



            Debug.Log("AudioBegin");
            TriggerBegin = true;
            AudioBegin.Invoke();
        }

            if (!TriggerComplete && AS.time >= AS.clip.length - .1f && AudioComplete != null)      
        {
            Debug.Log("End");
            TriggerComplete = true;
            AudioComplete.Invoke();
        }

        if (AudioGridTick.AudioGridInstance != null && !Trigger )

        {
            AudioGridTick.AudioGridInstance.LongSounds.Add(this);

            Interval = 60 / (float)AudioGridTick.AudioGridInstance.bpm;

            CanMakeNoise = true;

            Trigger = true;

            AudioGridTick.AudioGridInstance.storedAudioAction += DoClick;

            AS.enabled = false;

            if (AutoStart && DelayBeat == 0) PlayLongSound();

            NextEventTime = AudioSettings.dspTime;

        }

        if (AutoStart && DelayBeat != 0 && DoClickTrigger && Counter >= DelayBeat)
        {
            CanMakeNoise = false;
            AS.enabled = true;
            AS.Play();
            Counter = 0;
            DoClickTrigger = false;
            AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;
        
        }


    }
}
