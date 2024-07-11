using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarPieceController : MonoBehaviour
{
    public Transform MainCamera;


    Vector3 EarPosition;
    Vector3 EarRotation;
    void Update()
    {
        EarRotation = MainCamera.eulerAngles;
        EarRotation.x = 0;
        EarRotation.z = 0;
        transform.eulerAngles = EarRotation;
    }
}
