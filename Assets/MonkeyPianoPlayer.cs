using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeyPianoPlayer : MonoBehaviour
{
    public Animator MonkeyPlayer;

    public AudioSource AS;

    public void PlayChord(AudioClip _AC)
    {
        //MonkeyPlayer.Play("MainAction", 0, 0);
        //AS.PlayOneShot(_AC);
    }

    public void PlayerPiano()
    {
   
        MonkeyPlayer.Play("MainAction", 0,0);

    }
}
