using System.Net.NetworkInformation;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;
using Klak.Wiring;

[System.Serializable]
public struct Destination
    {
        public Transform DestinationTransform;
        public PlayLongSoundTimed DestinationSoundObject;

    }

// agent will reach destination, will playReachedTarget(). soundobject will call CompleteTargetActions onAudio End

public class RavenInPoolManager : MonoBehaviour
{
    public Destination[] Destinations;
    public AgentInPlaceController AIPC;
    public Transform PlayerRig;
    public UnityEvent OnPlayerInZone;
    public UnityEvent OnPlayerOutZone;
    public PlayLongSoundTimed PLST;
    public AudioSource ASFlap;
    public AudioSource ASStartle;

    public float LookThreshold;

    Transform NextDestination;

    float StartY;
    void Start()
    {
        StartY = transform.position.y;

        Vector3 TempPosition = transform.position;
        TempPosition.x = PlayerRig.position.x;
        TempPosition.z = PlayerRig.position.z;
        //transform.position = TempPosition;
        Debug.Log("show raven");
        AIPC.Agent.Warp(TempPosition);
       // transform.position = PlayerRig.position;
       // transform.position = AIPC.RandomNavmeshLocation(7, PlayerRig.position);
        AIPC.SelectDestination(Destinations[0].DestinationTransform.position);
    }

    bool TargetTrigger, AudioTrigger, IsFlying, LookTrigger;
    int TargetIndex = 0;

    public void CompleteTargetActions()
    {
        Debug.Log("Completed target");
        
        TargetIndex++;
        if (TargetIndex == Destinations.Length) TargetIndex = 0;
        AIPC.SelectDestination(Destinations[TargetIndex].DestinationTransform.position);
        StartCoroutine(DoReset());
    }

    IEnumerator DoReset()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("do Reset");
        TargetTrigger = false;
    }
    public void ReachedTarget()
    {

        Debug.Log("Reached target 1");

        if (TargetTrigger) return;

        Debug.Log("Reached target 2");
        TargetTrigger = true;
        AudioTrigger = true;

       


    }

    // Update is called once per frame

    public Transform MainCamera;

    float RavenSpeed = 1;

    float RavenDistance;

    float Altitude = 0;

    float HeightMultix = 1;
    void Update()
    {

        


        RavenDistance = Vector3.Distance(transform.position, PlayerRig.position);


        if (IsFlying && !AIPC.HasReachedTarget)
        {

            AIPC.Agent.baseOffset = Mathf.Lerp(AIPC.Agent.baseOffset,3, HeightMultix * Time.deltaTime);
             
        }
        else
        {
            AIPC.Agent.baseOffset = Mathf.Lerp(AIPC.Agent.baseOffset, 0, HeightMultix * Time.deltaTime * 3f);
        }





        if (AudioTrigger && AIPC.HasReachedTarget && RavenDistance < 4)
        {
            IsFlying = false;
            AudioTrigger = false;
            PLST.PlayLongSound();
            Destinations[TargetIndex].DestinationSoundObject.PlayLongSound();
        }


        if (RavenDistance > 4)
        {
            IsFlying = false;
        }

        if (RavenDistance < 3 && !IsFlying && !AIPC.HasReachedTarget)
        {
            Debug.Log("Fly!!!");
            IsFlying = true;
            ASFlap.PlayOneShot(ASFlap.clip);
            ASStartle.PlayOneShot(ASStartle.clip);
        }



        if (Mathf.Abs(AIPC.Agent.baseOffset) <= .1f) RavenSpeed = 1;
        if (AIPC.Agent.baseOffset > .11f) RavenSpeed = 3;


        if (RavenDistance > 8)
        {
            IsFlying = false;
      
        }

        if(!AIPC.HasReachedTarget) AIPC.Agent.speed = RavenSpeed;

            //if (ZoneTrigger && Vector3.Distance(transform.position, PlayerRig.position) > 6.5f)
            //{
            //    OnPlayerInZone.Invoke();
            //    ZoneTrigger = false;
            //}

    }
}
