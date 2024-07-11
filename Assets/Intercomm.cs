using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Intercomm : MonoBehaviour
{
    public Renderer TargetRenderer;

    public Color EmissiveColor;

    public float EmissiveValue;

    public AudioSource AS;



    public int LoopLength;

    int NumberOfRings = 0;



    private void OnDisable()
    {
        AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;
    }

    void FadeUp()
    {
        float Value = 0;

        AS.PlayOneShot(AS.clip);

        DOTween.To(() => Value, x => Value = x, EmissiveValue, 15 / (float) AudioGridTick.AudioGridInstance.bpm).OnUpdate(() => {
            TargetRenderer.material.SetColor("_EmissiveColor", EmissiveColor * Value);
        }).OnComplete(()=> {
            FadeDown();
            });


    }

    void FadeDown()
    {
        float Value = EmissiveValue;

        DOTween.To(() => Value, x => Value = x, 0, 15 / (float)AudioGridTick.AudioGridInstance.bpm).OnUpdate(() => {
            TargetRenderer.material.SetColor("_EmissiveColor", EmissiveColor * Value);
        }).SetDelay(30 / (float)AudioGridTick.AudioGridInstance.bpm);


    }


    bool DoClickTrigger;
    int Counter = 0;
    void DoClick()
    {

        Counter++;
        DoClickTrigger = true;


    }

    bool IsLooping;
    void Update()
    {
        if (AudioGridTick.AudioGridInstance != null && !IsLooping)
        {

            AudioGridTick.AudioGridInstance.storedAudioAction += DoClick;

            IsLooping = true;

        }

        if (DoClickTrigger && Counter >= LoopLength && NumberOfRings < 5)
        {
            FadeUp();
            Counter = 0;
            NumberOfRings++;
            DoClickTrigger = false;
        }
    }

}
