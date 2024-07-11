using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZibraPositionMatcher : MonoBehaviour
{
    public Transform TargetOject;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = TargetOject.position;
    }
}
