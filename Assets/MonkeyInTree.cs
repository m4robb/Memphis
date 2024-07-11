using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeyInTree : MonoBehaviour
{
    public Transform Axis;

    Transform MainCamera;


    void Update()
    {
        if (!MainCamera)
        {
            MainCamera = Camera.main.transform;
            return;
        }

        //Vector3 direction = Axis.position - MainCamera.position;

        //Axis.rotation = Quaternion.FromToRotation(direction, Vector3.back);

        Axis.LookAt(MainCamera);
        Vector3 MonkeyDirection = new Vector3(0, 180 + Axis.localEulerAngles.y, 0);

        transform.localEulerAngles = MonkeyDirection;
    }
}
