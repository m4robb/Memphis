using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{

    public Transform StartPoint;
    public Transform EndPoint;
    public float Speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(Vector3.Distance(transform.position, EndPoint.position) < 1)
        {
            transform.position = StartPoint.position;
        }
        transform.Translate(Vector3.forward * Speed * Time.deltaTime);
    }
}
