using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowerBirdAlignmentController : MonoBehaviour
{

    Transform Target;

    Vector3 StoredPosition, CurrentPosition, TransformPosition;

    private void Start()
    {
        Target = Camera.main.transform;
        TransformPosition = transform.position;
        TransformPosition.x = Target.position.x;
        StoredPosition =  TransformPosition;
        transform.position = TransformPosition;

    }
    void Update()
    {
        TransformPosition = transform.position;
        TransformPosition.x = Target.position.x;
        CurrentPosition = Vector3.Lerp(StoredPosition, TransformPosition, Time.deltaTime * 2f);

        transform.position = CurrentPosition;

        StoredPosition = CurrentPosition;
 
    }
}
