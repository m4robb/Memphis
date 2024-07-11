using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishClockController : MonoBehaviour
{
    public float ClockSpeed = .1f;
    public float MinX, MaxX;


    float Direction = -1;
    void Start()
    {
        LocalPosition = transform.localPosition;
    }


    Vector3 LocalPosition;
    void Update()
    {
        if (transform.localPosition.x > MaxX)
        {
           

            Direction = -1;

  

        }

        if (transform.localPosition.x < MinX)
        {
            Direction = 1;


        }
        LocalPosition.x += ClockSpeed * Time.deltaTime * Direction;
        transform.localPosition = LocalPosition;
    }
}
