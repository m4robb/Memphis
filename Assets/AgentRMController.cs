using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MxMGameplay;
using UnityEngine.Events;
//using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
//using FIMSpace.Basics;
//using BrainFailProductions.PolyFew.AsImpL.MathUtil;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]

public class AgentRMController : MonoBehaviour
{
    private NavMeshAgent Agent;
    private Animator Animator;
    private Vector2 Velocity;
    private Vector2 SmoothDeltaPosition;
    public UnityEvent OnReachedDestination;

    [SerializeField]
    private AgentLookAt LookAt;


    [SerializeField] private bool Active;

    [SerializeField]
    private AIDestinationSetter AIDS;

    [SerializeField]
    [Range(0f, 3f)]
    private float WaitDelay = 1f;

    private Rigidbody RB;

    public Transform DestinationTransform = null;
    public Transform DestinationDestination = null;

    //private NavMeshTriangulation Triangulation;
    private Vector3 LastDestination = Vector3.zero;
    private NavMeshTriangulation Triangulation;
    private Vector3 StartingPosition = new Vector3();
    void Start()
    {
        StartingPosition = transform.position;
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        Triangulation = NavMesh.CalculateTriangulation();
        Animator.applyRootMotion = true;
        Agent.updatePosition = false;
        Agent.updateRotation = true;
    }

    Vector3 StoredPosition;
    float SelfSpeed;
    int counter;

    Vector3 AngularVelocity;
    Quaternion LastRotation;

    float CurrentAngularVelocity;

    float StoredSpeed, CurrentSpeed;

    bool HasHit;

    public void ResetAgent()
    {
        Triangulation = NavMesh.CalculateTriangulation();
        transform.position =StartingPosition;
        MakeInactive();
    }
    private void SynchronizeAnimatorAndAgent()
    {
        Vector3 worldDeltaPosition = Agent.nextPosition - transform.position;
        worldDeltaPosition.y = 0;
        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1, Time.deltaTime / 0.1f);
        SmoothDeltaPosition = Vector2.Lerp(SmoothDeltaPosition, deltaPosition, smooth);

        Velocity = SmoothDeltaPosition / Time.deltaTime;
        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            Velocity = Vector2.Lerp(Vector2.zero, Velocity, Agent.remainingDistance);
        }

    

        bool shouldMove = Velocity.magnitude > 0.5f && Agent.remainingDistance > Agent.stoppingDistance ;
        Animator.SetBool("CanMove", shouldMove);
        Animator.SetFloat("VelX", Velocity.x);


        Animator.SetFloat("VelY", Velocity.y);

        LookAt.lookAtTargetPosition = Agent.steeringTarget + transform.forward;

        float deltaMagnitude = worldDeltaPosition.magnitude;
        if (deltaMagnitude > Agent.radius / 2)
        {
            transform.position = Vector3.Lerp(Animator.rootPosition, Agent.nextPosition, smooth);
        }
    }

    public void StopMoving()
    {
        Agent.isStopped = true;
        StopAllCoroutines();
    }

    public void MakeActive()
    {
        Active = true;
    }

    public void MakeInactive()
    {
        Active = false;
    }

    private void OnAnimatorMove()
    {
        Vector3 rootPosition = Animator.rootPosition;
        rootPosition.y = Agent.nextPosition.y;
        transform.position = rootPosition;
        Agent.nextPosition = rootPosition;
    }

    private void Update()
    {



        if (Agent && DestinationTransform)
        {

            if (Agent.remainingDistance <= Agent.stoppingDistance && Agent.remainingDistance != Mathf.Infinity && Agent.remainingDistance != 0)
            {


                OnReachedDestination.Invoke();
                //DestinationTransform.position = DestinationDestination.position;
            }


            Vector3 destination = DestinationTransform.localPosition;

            Agent.SetDestination(destination);

            if (Vector3.SqrMagnitude(destination - LastDestination) > 0.01f)
            {
                Agent.SetDestination(destination);
                LastDestination = destination;
            }
        }
        if (Active)
        {
            SynchronizeAnimatorAndAgent();
        }
        else
        {
            Animator.SetBool("CanMove", false);
        }

    }
}
