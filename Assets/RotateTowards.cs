using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowards : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform Origin;
    public Transform Target;
    public Rigidbody PunchRigidbody;
    void Start()
    {
        
    }

    void Update()
    {
         
        Vector3 targetDirection = Target.position - Origin.position;

        
        float singleStep = 1f * Time.deltaTime;

         
        Vector3 newDirection = Vector3.RotateTowards(Origin.forward, targetDirection, singleStep, 0.0f);

       PunchRigidbody.MoveRotation(Quaternion.LookRotation(newDirection));

        //transform.rotation = Quaternion.LookRotation(newDirection);
    }
}
