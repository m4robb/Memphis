using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DreamVoice : MonoBehaviour
{
    public AudioSource AS;
    public float Zone;
    public GameObject DreamLine;

    public UnityEvent AudioEnded;
    public UnityEvent AudioStarted;

    Camera MainCamera;




    bool HasDreamt;
    void Start()
    {
        MainCamera = Camera.main;
    }

    void Update()
    {

        if (AS.time >= AS.clip.length && AudioEnded != null)
        {
            AudioEnded.Invoke();
        }
        if(Vector3.Distance(MainCamera.transform.position, transform.position) < Zone && !HasDreamt)
        {
            HasDreamt = true;

            if (AudioStarted != null) AudioStarted.Invoke();
            else
            {
               AS.Play();
               if(DreamLine!= null) DreamLine.SetActive(true);
            }

        }

        if (Vector3.Distance(MainCamera.transform.position, transform.position) > Zone && DreamLine != null)
        {
            DreamLine.SetActive(false);
        }
    }
}
