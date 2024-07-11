using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSoundMaker : MonoBehaviour
{
    
    public LayerMask layerMask;

    public AudioSource AS;

    public AudioClip[] AudioClipArray;

    bool CanMakeNoise, Trigger;

    int ClipIndex = 0;

    Vector3 StoredVelocity, CurrentVelocity;

    Rigidbody RB;

    IEnumerator PlayOneShot(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        AS.PlayOneShot(AudioClipArray[ClipIndex]);
       
    }

    IEnumerator FinishSound(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        Trigger = true;
        AS.enabled = false;
    }

    float KineticEnergy(Rigidbody rb)
    {
        // mass in kg, velocity in meters per second, result is joules
        return 0.5f * rb.mass * Mathf.Pow(rb.linearVelocity.magnitude, 2);
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (!Trigger) return;

        if (!CanMakeNoise) return;

        if ((layerMask.value & (1 << collision.transform.gameObject.layer)) != 0)
        {
            AS.enabled = true;

            Trigger = false;
            ClipIndex = Random.Range(0, AudioClipArray.Length);
            AS.volume = collision.impulse.sqrMagnitude + .1f;
            AS.clip = AudioClipArray[ClipIndex];
            AS.PlayOneShot(AS.clip);
            StartCoroutine(PlayOneShot((float)AudioGridTick.AudioGridInstance.GetDelayTime()));
            StartCoroutine(FinishSound((float)AudioGridTick.AudioGridInstance.GetDelayTime() + AS.clip.length));

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Trigger = true;
    }


    private void FixedUpdate()
    {

        if (RB == null) return;
        CurrentVelocity = StoredVelocity;
        StoredVelocity = RB.linearVelocity;
    }

    private void Update()
    {
        if (AudioGridTick.AudioGridInstance != null && !CanMakeNoise)
        {
            AS.enabled = false;
            RB = GetComponent<Rigidbody>();
            CanMakeNoise = true;
            Trigger = true;
        }
    }

}
