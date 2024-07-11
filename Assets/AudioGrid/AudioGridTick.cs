using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The code example shows how to implement a metronome that procedurally generates the click sounds via the OnAudioFilterRead callback.
// While the game is paused or the suspended, this time will not be updated and sounds playing will be paused. Therefore developers of music scheduling routines do not have to do any rescheduling after the app is unpaused



[RequireComponent(typeof(AudioSource))]
public class AudioGridTick : MonoBehaviour
{

    public Queue AudioQ = new Queue();
    public delegate void StoredAudioAction();
    public event StoredAudioAction storedAudioAction;

    public List<PlayLongSoundTimed> LongSounds = new List<PlayLongSoundTimed>();
    public List<AudioLooper> AudioLoops = new List<AudioLooper>();

    public double bpm = 108.0F;
    public float gain = 0F;
    public int signatureHi = 4;
    public int signatureLo = 4;
    double TriggerTick = 0.0F;
    private double SecondsDelay;
    private double nextTick = 0.0F;
    private float amp = 0.0F;
    private float phase = 0.0F;
    private double sampleRate = 0.0F;
    private int accent;
    private bool running = false;

    public static AudioGridTick AudioGridInstance;



    void Start()
    {
        AudioGridInstance = this;
        accent = signatureHi;
        double startTick = AudioSettings.dspTime;
        sampleRate = AudioSettings.outputSampleRate;
        nextTick = startTick * sampleRate;
        running = true;
    }


    int Counter = 0;
    void BeatExecute()
    {

        Counter++;

        if (storedAudioAction != null)
        {
            storedAudioAction();
           
        }


       
        //storedAudioAction = null;

        //while (AudioQ.Count > 0)
        //    ((StoredAudioAction)AudioQ.Dequeue())();
    }

    public double GetDelayTime()
    {
        SecondsDelay = TriggerTick - AudioSettings.dspTime;
        return SecondsDelay;
    }


    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!running)
            return;

        double samplesPerTick = sampleRate * 60.0F / bpm * 4.0F / signatureLo;
        double sample = AudioSettings.dspTime * sampleRate;
        int dataLen = data.Length / channels;
        int n = 0;
        while (n < dataLen)
        {
            float x = gain * amp * Mathf.Sin(phase);
            int i = 0;
            while (i < channels)
            {
                data[n * channels + i] += x;
                i++;
            }
            while (sample + n >= nextTick)
            {
                nextTick += samplesPerTick;
                amp = 1.0F;
                if (++accent > signatureHi)
                {
                    accent = 1;
                    amp *= 2.0F;
                }
               TriggerTick = nextTick / sampleRate;
                BeatExecute();

            }
            phase += amp * 0.3F;
            amp *= 0.993F;
            n++;
        }
    }
}