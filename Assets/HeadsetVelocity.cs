using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadsetVelocity : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public float headsetVelocity;
    Vector3 lastHeadsetPosition;
    public Transform headset;


    public void Update()
    {
        headsetVelocity = (headset.position - lastHeadsetPosition).magnitude / Time.deltaTime;
        lastHeadsetPosition = headset.position;
    }
}
