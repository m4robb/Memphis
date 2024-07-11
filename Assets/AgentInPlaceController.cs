using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AgentInPlaceController : MonoBehaviour
{
    public NavMeshAgent Agent;
    public Animator AgentAnimator;
    public Transform FollowTarget;
    public UnityEvent OnReachedDestination;
    private Vector2 SmoothDeltaPosition;
    private float Velocity;
    Vector3 CurrentTarget;

    void Start()
    {
        CurrentTarget = FollowTarget.position;
        Agent.SetDestination(FollowTarget.position);
    }
    public bool HasReachedTarget;
    bool CanMoveToggle ;

    public Vector3 RandomNavmeshLocation(float radius, Vector3 _StartPosition)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    public void SelectDestination(Vector3 destination)
    {
        FollowTarget.position = destination;
    }

    public void ChooseRandomDestination(float _Distance, Vector3 _StartPosition)
    {
        FollowTarget.position = RandomNavmeshLocation(_Distance, _StartPosition);
    }

    void ResetHasReachedTrigger()
    {
        HasReachedTarget = false;
    }
    void Update()
    {


        Velocity = Agent.velocity.magnitude;

        if (Velocity == 0 && !CanMoveToggle)
        {
            CanMoveToggle = true;
            AgentAnimator.SetBool("CanMove", false);
        }

        if (Velocity != 0 && CanMoveToggle)
        {
            CanMoveToggle = false;
            AgentAnimator.SetBool("CanMove", true);
        }

        AgentAnimator.SetFloat("VelY", Agent.velocity.magnitude);  


       if(CurrentTarget != FollowTarget.position)
        {
            CurrentTarget = FollowTarget.position;
            Agent.SetDestination(FollowTarget.position);
            Invoke("ResetHasReachedTrigger", .1f);
        }

       if(Agent.remainingDistance < Agent.stoppingDistance)
        {
            
            if (OnReachedDestination != null && !HasReachedTarget)
            {   Debug.Log("at destination");
                OnReachedDestination.Invoke();
                HasReachedTarget = true;
            }
            
        }
       

    }
}
