using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RotationSound : MonoBehaviour
{
    Rigidbody RB;
    public AudioSource InUtero;
    float OpeningSpeed;
    AudioSource Creak;
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        Creak = GetComponent<AudioSource>();
        Creak.enabled = false;
    }

    bool ClearMute, IsCreaking;

    float ReturnAudio = 0;

    IEnumerator InUteroStart()
    {
        yield return new WaitForSeconds(2);
        InUtero.DOFade(1, 8);
        InUtero.Play();
    }

    IEnumerator UnmuteMainAudio()
    {
        yield return new WaitForSeconds(20);
        MainAudioController.MainAudioControllerInstance.FadeDuckable("Duckable", ReturnAudio);
        ClearMute = false;
    }



    void Update()
    {



        OpeningSpeed = Mathf.Round(RB.angularVelocity.magnitude * 1000f) / 1000f;

        if (OpeningSpeed > 0 && !IsCreaking)
        {
            IsCreaking = true;
            Creak.enabled = true;
            Creak.Play();
            MuteMainAudio();
        }

        if (OpeningSpeed == 0)
        {
            IsCreaking = false;
            Creak.enabled = false;
            Creak.Pause();
        }
        Creak.volume = Mathf.Clamp(OpeningSpeed, 0f, 1f);

    }


    public void MuteMainAudio()
    {

        if (!ClearMute)
        {
            ClearMute = true;
            ReturnAudio = MainAudioController.MainAudioControllerInstance.GetCurrentLevel("Duckable");
            StartCoroutine(UnmuteMainAudio());
            StartCoroutine(InUteroStart());
            MainAudioController.MainAudioControllerInstance.FadeDuckable("Duckable", -80f);
        }



    }

}

   


