using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class DiscController : MonoBehaviour
{
    public AudioClip TrackRightSide;

    public AudioClip TrackLeftSide;

    public UnityEvent OnSnap;

    public AudioSource SpeakerRightSide;

    public AudioSource SpeakerLeftSide;

    public Animator DiscAnim;

    public double BPM;

    bool Tirgger;

    private void Start()
    {
        DiscAnim.speed = 0;
    }

    public void SetupTracks()
    {
        SpeakerLeftSide.time = 0;
        SpeakerRightSide.time = 0;
        SpeakerRightSide.clip = TrackRightSide;
        SpeakerLeftSide.clip = TrackLeftSide;
        AudioGridTick.AudioGridInstance.bpm = BPM;
    }



    bool Trigger;

    private void Update()
    {


        if  (SpeakerRightSide.clip == TrackRightSide && SpeakerRightSide.time > 0 && !Trigger)
        {
            if (OnSnap != null) OnSnap.Invoke();
            Trigger = true;
        }
    }


}
