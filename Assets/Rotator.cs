using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Transform CenterTarget;

    public float DegreesPerSecond = -4;

    Vector3 StoredPosition;


    void Update()
    {

        transform.position = Vector3.Lerp(transform.position, CenterTarget.position, Time.deltaTime * .05f);
        transform.Rotate(new Vector3(0, DegreesPerSecond, 0) * Time.deltaTime);

        StoredPosition = transform.position;
    }
}
