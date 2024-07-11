using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EarTrumpetController : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform North;
    public Transform East;
    public Transform South;
    public Transform West;
    public Transform Earpiece;
    public AudioClip AudioClipNorth;
    public AudioClip AudioClipEast;
    public AudioClip AudioClipSouth;
    public AudioClip AudioClipWest;

    AudioSource ASNorth;
    AudioSource ASEast;
    AudioSource ASSouth;
    AudioSource ASWest;



    public GameObject AudioSourceGO;

    GameObject AudioGONorth, AudioGOEast, AudioGOSouth, AudioGOWest;

    float AngleNorth, AngleEast, AngleSouth, AngleWest;

    bool IsInHand;


    void Start()
    {
        
    }

    //protected virtual void OnAttachedToHand(Hand hand)
    //{
    //    MainAudioController.MainAudioControllerInstance.FadeDuckable("Duckable", -80f);
    //    InitAudioSources();
    //}

    //protected virtual void OnDetachedFromHand(Hand hand)
    //{
    //    MainAudioController.MainAudioControllerInstance.FadeDuckable("Duckable", 0f);
    //    DestroyAudioSources();
    //}


    public void InitAudioSources()
    {

        Debug.Log("hello hand");
 
        AudioGONorth = Instantiate(AudioSourceGO, transform);
        AudioGOEast = Instantiate(AudioSourceGO, transform);
        AudioGOSouth = Instantiate(AudioSourceGO, transform);
        AudioGOWest = Instantiate(AudioSourceGO, transform);

        ASNorth = AudioGONorth.GetComponent<AudioSource>();
        ASSouth = AudioGOSouth.GetComponent<AudioSource>();
        ASEast = AudioGOEast.GetComponent<AudioSource>();
        ASWest = AudioGOWest.GetComponent<AudioSource>();

        ASNorth.clip = AudioClipNorth;
        ASEast.clip = AudioClipEast;
        ASSouth.clip = AudioClipSouth;
        ASWest.clip = AudioClipWest;

        ASNorth.Play();
        ASEast.Play();
        ASSouth.Play();
        ASWest.Play();

        IsInHand = true;
    }

    public void DestroyAudioSources()
    {
        IsInHand = false;
        Destroy(AudioGONorth);
        Destroy(AudioGOEast);
        Destroy(AudioGOSouth);
        Destroy(AudioGOWest);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsInHand) return;
        AngleNorth = Vector3.Angle(North.position - transform.position, transform.forward) -90;
        ASNorth.volume = Mathf.Clamp(AngleNorth * 0.1f, 0, 1);

        AngleEast = Vector3.Angle(East.position - transform.position, transform.forward) - 90;
        ASEast.volume = Mathf.Clamp(AngleEast * 0.1f, 0, 1);

        AngleSouth = Vector3.Angle(South.position - transform.position, transform.forward) - 90;
        ASSouth.volume = Mathf.Clamp(AngleSouth * 0.1f, 0, 1);

        AngleWest = Vector3.Angle(West.position - transform.position, transform.forward) - 90;
        ASWest.volume = Mathf.Clamp(AngleWest * 0.1f, 0, 1);
    }
}
