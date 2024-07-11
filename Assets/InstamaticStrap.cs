using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstamaticStrap : MonoBehaviour
{
    public Transform FollowObject;
    public float Distance = .5f;
    public float LerpSpeed = 1;
    void OnEnable()
    {
        TimeElapsed = 0;
        StoredLocation = transform.position;
    }

    Vector3 StoredLocation, CurrentLocation;

    float TimeElapsed;
    void Update()
    {
        CurrentLocation = FollowObject.position;
        CurrentLocation.y -= Distance;
        CurrentLocation.x -= Distance * .2f;
        transform.position = Vector3.Lerp(StoredLocation, CurrentLocation, TimeElapsed/LerpSpeed);
        TimeElapsed += Time.deltaTime;
        StoredLocation = transform.position;
    }
}
