using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using DG.Tweening;

public class PathfinderHelper : MonoBehaviour
{
    public Animator AnimationController;
    public RichAI RAI;
    public float SpeedModifier;

    Vector3 relVelocity;

    public float OriginalMaxSpeed = .5f;

    

    bool IsAtDestination, IsStopped;

    void OnTargetReached()
    {
        float _Speed = RAI.maxSpeed;

        DOTween.To(() => _Speed, x => _Speed = x, 0, 1).OnUpdate(() =>
        {
            RAI.maxSpeed = _Speed;
        });

        IsStopped = true;

        Debug.Log("Stopping");


    }
        void Update()
    {

        if (RAI.reachedEndOfPath)
        {

            if (!IsAtDestination) OnTargetReached();
            IsAtDestination = true;
        }
        else
        {
            if (RAI.maxSpeed < OriginalMaxSpeed && RAI.canMove) RAI.maxSpeed += Time.deltaTime * 2f;
            IsAtDestination = false;
            //IsStopped = false;
        }

        relVelocity = transform.InverseTransformDirection(RAI.velocity);
        AnimationController.SetFloat("Speed", relVelocity.magnitude * SpeedModifier);


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
