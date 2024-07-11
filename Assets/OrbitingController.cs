using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingpController : MonoBehaviour
{
    // Start is called before the first frame update
    public float Speed = .001f;

    public Transform OrbitCenter;

    // Update is called once per frame
    void Update()
    {

        var lookPos = OrbitCenter.localPosition - transform.localPosition;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        rotation *= Quaternion.Euler(0, -90, 0); // this adds a 90 degrees Y rotation
        transform.localRotation = rotation;

        Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);
        transform.Translate(localForward * Speed, Space.Self);
    }
}
