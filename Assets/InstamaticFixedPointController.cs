using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstamaticFixedPointController : MonoBehaviour
{
    public Transform FollowPoint;
    public Rigidbody RB;


    // Update is called once per frame
    void FixedUpdate()
    {
        RB.MovePosition(FollowPoint.position);
    }
}
