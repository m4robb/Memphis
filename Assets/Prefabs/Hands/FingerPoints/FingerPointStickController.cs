using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerPointStickController : MonoBehaviour
{
    
    void Start()
    {
        transform.localPosition = new Vector3(0.009f, 0.0011f, -0.0049f);
        transform.localEulerAngles = new Vector3(6.63f ,-102, -1.4f); 
    }


}
