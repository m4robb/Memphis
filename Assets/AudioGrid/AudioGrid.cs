using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioGrid : MonoBehaviour
{
    // Start is called before the first frame update

    public bool Running;
    public float BPM = 108.0f;
    public double NextEventTime16;
    public double NextEventTime4;

    public AudioSource Click;
    public AudioClip ClickClip;
    public bool PlayClick = true;

    void Start()
    {
        NextEventTime4 = AudioSettings.dspTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Running)
        {
            NextEventTime4 = AudioSettings.dspTime;
            return;
        }

        double Time = AudioSettings.dspTime;

        if (Time > NextEventTime4)
        {
            Click.PlayScheduled(NextEventTime4);

            //if (PlayClick && Click != null) Click.PlayOneShot(ClickClip);
            NextEventTime4 += 60.0f / BPM / 4;
        }

    }
}
