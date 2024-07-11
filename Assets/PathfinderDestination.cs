using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using DG.Tweening;
using UnityEngine.Events;

public class PathfinderDestination : MonoBehaviour
{

    public UnityEvent TargetReached;
    public RichAI RAI;
    bool IsAtDestination, IsStopped;

    public void SetMaxSpeed(float _MaxSpeed)
    {
        RAI.maxSpeed = _MaxSpeed;
    }

    public void CanMove(bool _CanMove)
    {
        RAI.canMove = _CanMove;
    }

    void OnTargetReached()
    {
        if(!IsStopped)
        {
            IsStopped = true;
            if (TargetReached != null) TargetReached.Invoke();
        }
       

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
            IsAtDestination = false;
            IsStopped = false;
        }


    }
}
