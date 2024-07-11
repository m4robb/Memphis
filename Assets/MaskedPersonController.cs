using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskedPersonController : MonoBehaviour
{
    Vector3 StartPosition;
    void Start()
    {
        StartPosition = transform.position; 

    }


    public void ResetPosition()
    {
        transform.position = StartPosition;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
