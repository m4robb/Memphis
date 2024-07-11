using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSound : MonoBehaviour
{

    public GameObject CollisionSoundEmitter;
    public AudioClip SFXClip;
    public float SoundThreshold = -0.3f;
    public float SoundVolume = 0;

    Rigidbody RB;


    float KillDelay = 0;

    List<float> AveragedVelocity = new List<float>();

    bool SoundTrigger = true;

    // Start is called before the first frame update
    void Start()
    {   
        KillDelay = SFXClip.length;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (SoundTrigger) return;
        SoundTrigger = true;
        float rounded = Mathf.Round((collision.relativeVelocity.magnitude) * 1000.0f) / 1000.0f + SoundThreshold;
        Rigidbody _RB = collision.gameObject.GetComponent<Rigidbody>();
        SFXControllerHand SFXCH = collision.gameObject.GetComponent<SFXControllerHand>();
        if(SFXCH != null && SFXCH.IsBeingHeld)
        {
            rounded = SFXCH.KineticEnergy - 0.3f;
        }
        GameObject GO = Instantiate(CollisionSoundEmitter, transform);
        GO.GetComponent<AudioSource>().volume = Mathf.Clamp(rounded, 0, 1);//collision.relativeVelocity.magnitude * .1f;
        SoundVolume = GO.GetComponent<AudioSource>().volume;
        GO.GetComponent<AudioSource>().clip = SFXClip;
        GO.GetComponent<AudioSource>().Play();
        StartCoroutine(KillSoundGO(GO, KillDelay));
        StartCoroutine(ReleaseSound(KillDelay * .5f));

        if(rounded > 0 && MainAudioController.MainAudioControllerInstance)
            MainAudioController.MainAudioControllerInstance.DoMelody(Mathf.Clamp(rounded * 20, 5, 20));
    }
     IEnumerator ReleaseSound(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        SoundTrigger = false;
    }
    IEnumerator KillSoundGO (GameObject _GO, float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        
        Destroy(_GO);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
