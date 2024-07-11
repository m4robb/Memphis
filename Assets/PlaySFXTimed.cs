using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFXTimed : MonoBehaviour
{
    public AudioSource AS;
    public AudioClip ExternalClip;


    bool CanMakeNoise;
    AudioClip ClipToPlay;


    IEnumerator CleanUp(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        AS.enabled = false ;
    }
    IEnumerator PlayOneShot(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        if (ExternalClip != null) { ClipToPlay = ExternalClip; } else { ClipToPlay = AS.clip; }
        AS.enabled = true;
        AS.PlayOneShot(AS.clip);

    }

    public void PlaySFX()
    {

        if (!AS) return;

        if (AS.clip == null) return;
        AS.enabled = true;
        AS.PlayOneShot(AS.clip);
        //StopAllCoroutines();
        //StartCoroutine(PlayOneShot((float)AudioGridTick.AudioGridInstance.GetDelayTime()));
        StartCoroutine(CleanUp(AS.clip.length));

   
    }

    void Update()
    {
        if (AudioGridTick.AudioGridInstance != null && !CanMakeNoise)
        {
            AS.enabled = false;
            CanMakeNoise = true;
        }
    }
}
