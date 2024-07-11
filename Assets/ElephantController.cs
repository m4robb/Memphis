using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using RootMotion.FinalIK;

public class ElephantController : MonoBehaviour
{
    public RichAI RAI;
    public LookAtIK LAIK;
    public Transform Target;
    public float Distance;
    public Animator Anim;

    public Transform HeadBone;
    public Transform LeftEar;
    public Transform RightEar;


    bool HasSeen;

    float TimeInterval = 10;

    float Timer = 0;

    int TouchCount = 0;

    Vector3 RightEarAngles, LeftEarAngles;

    IEnumerator ElephantWalk() {

        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());

        RAI.canMove = true;
        //LAIK.enabled = true;
        //LAIK.solver.IKPositionWeight = 1;
        Anim.CrossFadeInFixedTime("MainTree", (float)AudioGridTick.AudioGridInstance.bpm / 60);
        RightEarAngles = RightEar.localEulerAngles;
        LeftEarAngles = LeftEar.localEulerAngles;
    }

    bool Trigger;

    float UpdateAnimClipTimes( string ClipName)
    {

        float ReturnValue = 3;
        AnimationClip[] clips = Anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == ClipName) ReturnValue = clip.length;


        }

        return ReturnValue;
    }
    IEnumerator ResetTrigger(float WaitTime)
    {
        yield return new WaitForSeconds(WaitTime + 1f);

        Trigger = false;

    }


    public void Trumpet()
    {
        if (Trigger) return;
        Trigger = true;
 

        string ClipName = "";
        if (TouchCount == 0) ClipName = "Trumpet";
        if (TouchCount == 1) ClipName = "Trumpet2";
        if (TouchCount == 2) ClipName = "Trumpet3";

        TouchCount++;

        if (TouchCount == 3) TouchCount = 0;


        Anim.CrossFadeInFixedTime(ClipName, (float)AudioGridTick.AudioGridInstance.bpm / 60);
        StartCoroutine(ResetTrigger(UpdateAnimClipTimes(ClipName)));

    }

    private void LateUpdate()
    {

        if(Headstate == 1)
        {

            LeftEarAngles = LeftEar.localEulerAngles;
            LeftEarAngles.y -= LDiff;
            LeftEarAngles.x += LDiff;
            LeftEar.localEulerAngles = LeftEarAngles;
            //Debug.Log("Head turned left " + LeftEar.localEulerAngles.y);
        }

        if (Headstate == 2)
        {

            RightEarAngles = RightEar.localEulerAngles;
            RightEarAngles.y += RDiff;
            RightEarAngles.x += RDiff;
            RightEar.localEulerAngles = RightEarAngles;
           // Debug.Log("Head turned right " + RightEar.localEulerAngles.y);
        }

    }


    int Headstate = 0; float RDiff, LDiff;
    void Update()
    {
        Headstate = 0;

        RAI.endReachedDistance = 6;

        if (HeadBone.localEulerAngles.y > 10 && HeadBone.localEulerAngles.y < 180)
        {
            Headstate = 1;
            LDiff = Mathf.Clamp((HeadBone.localEulerAngles.y - 10) * 2, 0, 30);

            if(HeadBone.localEulerAngles.y > 40)
            {
                RAI.endReachedDistance = 1;
            }
            else
            {
                RAI.endReachedDistance = 6;
            }
        }

        if (HeadBone.localEulerAngles.y > 180 && HeadBone.localEulerAngles.y < 340)
        {
            Headstate = 2;
            
            RDiff = Mathf.Clamp((340 - HeadBone.localEulerAngles.y) * 2, 0, 30);

            if (HeadBone.localEulerAngles.y < 320)
            {
                RAI.endReachedDistance = .5f;
            }
            else
            {
                RAI.endReachedDistance = 6;
            }

        }
        if (RAI.maxSpeed == 0 )
            Timer += Time.deltaTime;

        if(Timer > TimeInterval )
        {
            TimeInterval = Random.Range(5, 15);
            Timer = 0;
        }


        if(Vector3.Distance(transform.position, Target.position) < Distance && !HasSeen)
        {
            StartCoroutine(ElephantWalk());
            HasSeen = true;
        }
    }
}
