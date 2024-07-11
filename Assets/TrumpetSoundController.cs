using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrumpetSoundController : MonoBehaviour
{
    Camera MainCamera;

    public AudioSource SoundSource;
    public Transform MouthPiece;
    public Transform MouthCamera;
    public void StartTrumpet()
    {
        CanPlay = true;
        SoundSource.time = ElapsedTime;
        SoundSource.enabled = true;
    }

    public void StopTrumpet()
    {
        CanPlay = false;
        SoundSource.enabled = false;
    }

    bool CanPlay;

    float ElapsedTime = 0;

    void Update()
    {

        if (!MainCamera)
        {
            MainCamera = Camera.main;
            return;
        }

        if (!CanPlay) return;

        float dotProd = Vector3.Distance(MouthCamera.position,  MouthPiece.position);

        Debug.Log(dotProd);

        if (SoundSource.isPlaying)
        {
            ElapsedTime = SoundSource.time;
        }

        if (Mathf.Abs(dotProd) <  0.1f)
        {
            if(!SoundSource.isPlaying) SoundSource.Play();
        } else
        {
            SoundSource.Pause();
        }
    }
}
