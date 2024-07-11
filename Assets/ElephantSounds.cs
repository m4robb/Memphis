using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElephantSounds : MonoBehaviour
{

    public AudioSource FootstepAS;
    public AudioClip FootClip;

    public void Footstep()
    {

        Debug.Log("Stomp");
        FootstepAS.PlayOneShot(FootClip);
    }
}
