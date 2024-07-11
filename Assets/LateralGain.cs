using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateralGain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector3 lastPos = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        //The headset initializes at Vector3.zero, and remains there during Start(), so initialize lastPos here
        if (lastPos == Vector3.zero) lastPos = transform.position;
        var offset = transform.position - lastPos;
        offset.y = 0;
        transform.parent.position += offset * 7f;
        lastPos = transform.position;
    }
}
