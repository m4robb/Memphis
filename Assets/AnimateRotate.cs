using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateRotate : MonoBehaviour
{

    public float AnimationSpeed;

    Vector3 RotationValue;

    public string Axis = "Y";

    private void Start()
    {
         RotationValue = transform.localEulerAngles;
    }

    void Update()
    {
        if(Axis == "Y")
            RotationValue.y += AnimationSpeed * Time.deltaTime;
        if (Axis == "Z")
            RotationValue.z += AnimationSpeed * Time.deltaTime;
        if (Axis == "X")
            RotationValue.x += AnimationSpeed * Time.deltaTime;

        transform.localEulerAngles = RotationValue;
    }
}
