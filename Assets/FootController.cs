using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootController : MonoBehaviour
{

    public Transform HeadPos;

    Vector3 TrackHead;
    void Update()
    {
        TrackHead = HeadPos.position;
        TrackHead.y = transform.position.y;
        transform.position = TrackHead;
    }
}
