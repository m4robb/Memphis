using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goatController : MonoBehaviour

    
{

    public Transform OrbitCenter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    float StartAngle = 90;
    void FixedUpdate()
    {

        var lookPos = OrbitCenter.localPosition - transform.localPosition;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        rotation *= Quaternion.Euler(0, 90, 0); // this adds a 90 degrees Y rotation
        transform.localRotation = rotation; //Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * .1f);


        //transform.LookAt(OrbitCenter.position + transform.forward);

    }
}
