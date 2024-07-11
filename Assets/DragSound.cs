using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragSound : MonoBehaviour
{
    // Start is called before the first frame update
    public string CheckColliderTag = "floor";
    Collider DragOject;
    AudioSource DragAudio;
    Rigidbody RB;
    bool InContact, IsPlaying;
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        DragAudio = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == CheckColliderTag)
        {
            InContact = true;
        }
       
    }

    private void OnCollisionExit(Collision collision)
    {
        InContact = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.tag == CheckColliderTag)
        {
            InContact = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (InContact && RB.linearVelocity.magnitude > 0.01f)
        {
            DragAudio.Play();
            DragAudio.volume = RB.linearVelocity.magnitude;
        }
        else
        {
            DragAudio.Pause();
        }
        
    }
}
