using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassController : MonoBehaviour
{
    public Transform Target;
    public Transform Needle;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var lookPos = Target.position - transform.position;

        Quaternion rotation = Quaternion.LookRotation(lookPos);
        rotation.x = 0;
        rotation.z = 0;
        Needle.rotation = Quaternion.Slerp(Needle.rotation, rotation, Time.deltaTime * 2);
        Quaternion Stablize = Needle.localRotation;
        Stablize.x = 0;
        Stablize.z = 0;
        Needle.localRotation = Stablize;
    }
}
