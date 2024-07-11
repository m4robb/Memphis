using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SyncSounds : MonoBehaviour
{
    public AudioSource MasterTrack;
    public AudioSource SyncTrack;

    double SyncTime = 0;

    double NextEventTime;

    bool IsLooping, DoClickTrigger, IsPaused;
    float SyncCounter = 4.5f;
    int BeatCounter = 0;
    float ElapsedTime;

    IEnumerator TimedVolumeChange(float _TargetVolume)
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());

            MasterTrack.DOFade(_TargetVolume, 2 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);
            //SyncTrack.DOFade(_TargetVolume, 2 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);

    }

    public void VolumeChange(float _TargetVolume)
    {
        StartCoroutine(TimedVolumeChange(_TargetVolume));
    }

    public void SwitchOn()
    {
        IsPaused = false;
        MasterTrack.Play();
        //SyncTrack.Play();
    }

    public void SwitchOff()
    {
        IsPaused = true;
        MasterTrack.Pause();
        //SyncTrack.Pause();
    }

    public void DoClick()
    {
        if (IsPaused) return;
        BeatCounter++;
        DoClickTrigger = true;
    }

    private void OnDisable()
    {
        AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;
    }
    void Update()
    {
        if(AudioGridTick.AudioGridInstance != null && !IsLooping)
        {
            AudioGridTick.AudioGridInstance.storedAudioAction += DoClick;
            NextEventTime = AudioSettings.dspTime;
            IsLooping = true;
        }

        if (DoClickTrigger && BeatCounter == 1)
        {
            // Debug.Log(NextEventTime + (8 * 60 / AudioGridTick.AudioGridInstance.bpm));

            //MasterTrack.Play();
            //MasterTrack.PlayScheduled( (8 * 60 / AudioGridTick.AudioGridInstance.bpm));
            MasterTrack.PlayScheduled(NextEventTime + (8 * 60 / AudioGridTick.AudioGridInstance.bpm));
            DoClickTrigger = false;
        }

        if (!IsLooping) return;
        if (IsPaused) return;

        ElapsedTime = MasterTrack.time;


        if (DoClickTrigger && BeatCounter > 8)
        {
          
            SyncTime = Mathf.Abs(SyncTrack.timeSamples - MasterTrack.timeSamples) * .001;

            if (SyncTime > .2)
            {
                //SyncTrack.time = MasterTrack.time;
            }
        }


    }
}
