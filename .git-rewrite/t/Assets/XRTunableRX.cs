using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class XRTunableRX : MonoBehaviour
{
    // Start is called before the first frame update

    //public Transform North;
    //public Transform East;
    //public Transform South;
    //public Transform West;
    public Transform Earpiece;
    public Transform Headpiece;
    //public AudioClip AudioClipNorth;
    //public AudioClip AudioClipEast;
    //public AudioClip AudioClipSouth;
    //public AudioClip AudioClipWest;

    public AudioSource ASFarRight;
    public AudioSource ASNearRight;
    public AudioSource ASNearLeft;
    public AudioSource ASFarLeft;



    public GameObject AudioSourceGO;

    GameObject AudioGONorth, AudioGOEast, AudioGOSouth, AudioGOWest;

    float AngleNorth, AngleEast, AngleSouth, AngleWest;

    bool IsInHand;


    void Start()
    {
        
    }

    public void StartTuning()
    {
        //MainAudioController.MainAudioControllerInstance.FadeDuckable("Duckable", -80f);
        InitAudioSources();
    }

    public void EndTuning()
    {
        //MainAudioController.MainAudioControllerInstance.FadeDuckable("Duckable", 0f);
        //DestroyAudioSources();
    }

    Vector3 EarPosition;
    public void InitAudioSources()
    {

  



        ASFarRight.Play();
        ASNearRight.Play();
        ASNearLeft.Play();
        ASFarLeft.Play();

        IsInHand = true;
    }

    public void DestroyAudioSources()
    {
        //IsInHand = false;
        //Destroy(AudioGONorth);
        //Destroy(AudioGOEast);
        //Destroy(AudioGOSouth);
        //Destroy(AudioGOWest);
    }

    // Update is called once per frame

    float BasicAngle = 0, FarRight, NearRight, NearLeft,FarLeft;
    void Update()
    {
        if (!IsInHand) return;

        EarPosition = Headpiece.position;
        EarPosition.y = transform.position.y;
        Earpiece.position = EarPosition;


        BasicAngle = Mathf.Abs(Vector3.Angle(Earpiece.position - transform.position, Earpiece.right));

        FarRight = 1 - 4 * Mathf.Abs(175 - BasicAngle)/180;

        NearRight = 1 - 4 * Mathf.Abs(90 - BasicAngle)/180;

        NearLeft = 1 - 4 * Mathf.Abs(75 - BasicAngle) / 180;

        FarLeft = 1 - 4 * Mathf.Abs(15- BasicAngle) / 180;

        ASFarRight.volume = Mathf.Clamp(FarRight, 0, 1);
        ASNearRight.volume = Mathf.Clamp(NearRight, 0, 1);
        ASNearLeft.volume = Mathf.Clamp(NearLeft, 0, 1);
        ASFarLeft.volume = Mathf.Clamp(FarLeft, 0, 1);
    }
}
