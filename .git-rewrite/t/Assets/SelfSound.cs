using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class SelfSound : MonoBehaviour
{

    public AudioSource ASDryStep;
    public AudioSource ASWetStep;
    public AudioSource ASSoftStep;

    public AudioClip[] DrySounds;
    public AudioClip[] WetSounds;

    public AnimationCurve StepVolumeMapping;

    public CharacterController CC;

    public float FootstepsFrequency;
    public float DryValue, AmbientValue;

    public float Threshold = .01f;
    
    Vector3 StoredPosition;
    public float SelfSpeed;
    float  FootstepsTimer, FootStepTimingPadding;

    int Counter = 0;

    void Start()
    {
    }



    public void Step2(int _Foot)
    {
        //DistanceToSound = Vector3.Distance(transform.position, RealTarget.transform.position) + .001f;
        //FootStepTimingPadding = Random.Range(0, 0.2f);
        if(SelfSpeed < Threshold) return;
        float FootVolume = StepVolumeMapping.Evaluate(SelfSpeed);
        ASDryStep.volume = FootVolume * DryValue; ;
        ASWetStep.volume = FootVolume * AmbientValue;
        FootStepTimingPadding = Random.Range(0, 0.2f);
        ASWetStep.pitch = Random.Range(.9f, 1.1f) ;
        ASDryStep.pitch = Random.Range(.9f, 1.1f);
        int StepIndex = Random.Range(0, DrySounds.Length);
        
        //ASWetStep.volume = (Mathf.Clamp((DistanceToSound - 6), 0.2f, 1f));
        //ASDryStep.volume = .5f - Mathf.Clamp(DistanceToSound / 12, 0f, .4f);

        ASDryStep.PlayOneShot(DrySounds[StepIndex]);
        //ASSoftStep.Play();
        ASWetStep.PlayOneShot(WetSounds[StepIndex]);



    }
    
    void Update()
    {

        SelfSpeed = CC.velocity.sqrMagnitude;
        
        if(SelfSpeed < Threshold) return;

        FootstepsTimer += Time.deltaTime;

        if (FootstepsTimer > FootstepsFrequency / SelfSpeed + FootStepTimingPadding)
        {
            Step2(0);
            FootstepsTimer = 0;
        }

    }
}
