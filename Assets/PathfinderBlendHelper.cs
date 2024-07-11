using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using DG.Tweening;

public class PathfinderBlendHelper : MonoBehaviour
{
    public Animator AnimationController;
    public RichAI RAI;
    public float SpeedModifier;
    public float AnimationOffset = 0;

    Vector3 relVelocity;

    Vector3 StoredVelocity, CurrentVelocity;

    public float OriginalMaxSpeed = .5f;

    private void Start()
    {
       AnimationController.SetFloat("Offset", AnimationOffset);
       StoredVelocity = transform.InverseTransformDirection(RAI.velocity);
    }

    bool IsAtDestination, IsStopped, CoTrigger;

    IEnumerator StartwalkingAgain()
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());

         IsAtDestination = false;
    }

    void OnTargetReached()
    {
        //float _Speed = RAI.maxSpeed;

        //DOTween.To(() => _Speed, x => _Speed = x, 0, 1).OnUpdate(() =>
        //{
        //    RAI.maxSpeed = _Speed;
        //});

        CoTrigger = false;

        IsStopped = true;


    }

    Quaternion PreviousRotation;



    float CharacterTurnAngle;

    void Update()
    {

        if (RAI.reachedEndOfPath)
        {

            if (!IsAtDestination) OnTargetReached();
            IsAtDestination = true;
        }
        else
        {
            if (!CoTrigger)
            {
                StartCoroutine(StartwalkingAgain());
                CoTrigger = true;
            }



            //if (!IsAtDestination)
            //    if (RAI.maxSpeed < OriginalMaxSpeed && RAI.canMove) RAI.maxSpeed += Time.deltaTime * .5f;
            
            //IsStopped = false;
        }


        relVelocity = transform.InverseTransformDirection(RAI.velocity);

        CurrentVelocity = Vector3.Lerp(StoredVelocity, CurrentVelocity, Time.deltaTime * 2f);

        AnimationController.SetFloat("Speed", relVelocity.magnitude);
        //AnimationController.SetFloat("CrossSpeed", CurrentVelocity.x);

        //Quaternion deltaRotation = RAI.rotation * Quaternion.Inverse(PreviousRotation);

        //Debug.Log(RAI.rotation.y - PreviousRotation.y);

        ////    if (IsGuide && IsWalking) LAIK.solver.IKPositionWeight = 0;

        //deltaRotation.ToAngleAxis(out var angle, out var axis);

        //angle *= Mathf.Deg2Rad;

        //AngularVelocity = (Time.deltaTime * .5f) * angle * axis;

        //CharacterTurnAngle = Mathf.Clamp(AngularVelocity.y, -2f, 2f);

        //PreviousRotation = RAI.rotation;

       

        //AnimationController.SetFloat("CrossSpeed", Mathf.Lerp(AnimationController.GetFloat("CrossSpeed"), CharacterTurnAngle, Time.deltaTime));


        //if (!IsStopped)
        //{
        //    relVelocity = transform.InverseTransformDirection(RAI.velocity);
        //    AnimationController.SetFloat("Speed", relVelocity.magnitude * SpeedModifier);
        //    //AnimationController.SetFloat("SpeedMultix",  relVelocity.magnitude * SpeedModifier);
        //} else {

        //    float _ThisSpeed = AnimationController.GetFloat("SpeedMultix");
        //    if (_ThisSpeed < OriginalMaxSpeed * SpeedModifier) AnimationController.SetFloat("SpeedMultix", _ThisSpeed + Time.deltaTime);

        //}

    }
}
