using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PhoneController : MonoBehaviour
{

    public AudioSource VoiceMail;
    public AudioSource PhoneSignal;

    bool CanMakeNoise , HasPlayed;

    IEnumerator HangUp(float _Delay, AudioSource _AS)
    {
        yield return new WaitForSeconds(_Delay);
        _AS.enabled = false;


    }

    public void FadePhone(AudioSource _AS)
    {
        _AS.DOFade(0, 1);
    }

    public void PlayPhone(AudioSource _AS)
    {

        _AS.enabled = true;
         HasPlayed = true;
        _AS.PlayDelayed((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        StartCoroutine(HangUp((float)AudioGridTick.AudioGridInstance.GetDelayTime() + _AS.clip.length, _AS));
    }

   
    void Update()
    {
        if (AudioGridTick.AudioGridInstance != null && !CanMakeNoise)
        {
            VoiceMail.enabled = false;
            PhoneSignal.enabled = false;
            CanMakeNoise = true;

        }
    }
}
