using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Soundscape : MonoBehaviour
{
    public AudioMixerSnapshot Snapshot;
    public float TransitionDuration = 1;


    private void OnTriggerEnter(Collider other)
    {

        //Snapshot.TransitionTo(TransitionDuration);
    }
    void Update()
    {
        
    }
}
