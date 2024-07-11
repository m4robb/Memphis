using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class RoomMixer : MonoBehaviour
{

    public AudioMixerSnapshot HomeMain;
    public AudioMixerSnapshot OnThePhone;
    // Start is called before the first frame update

    public void PickUpPhone()
    {
        OnThePhone.TransitionTo(1f);
    }

    public void PutDownPhone()
    {
        HomeMain.TransitionTo(1f);
    }
}
