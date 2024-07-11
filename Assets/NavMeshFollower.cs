using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshFollower : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent FollowerAgent;
    public Transform FollowTarget;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        FollowerAgent.SetDestination(FollowTarget.position);

    }
}
