using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightFollow : MonoBehaviour
{
    public Transform FollowPoint;
    public Transform LightPoint;
    public float LerpMultix = 2;


    Vector3 FollowPointPosition;

    Vector3 StoredPosition;

    private void Start()
    {
        LightPoint.position = FollowPoint.position;
    }

    void Update()
    {

        if (FollowPoint)
        {

            FollowPointPosition = Vector3.Lerp(LightPoint.position, FollowPoint.position, LerpMultix * Time.deltaTime);

           
            transform.LookAt(FollowPointPosition);

            LightPoint.position = FollowPointPosition;
        }
        
    }
}
