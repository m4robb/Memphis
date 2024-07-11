using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerPointCatPoint_Controller : MonoBehaviour
{
    
    void Start()
    {
        transform.localPosition = new Vector3(0.0161f, 0.0019f, -0.0001f);
        transform.localEulerAngles = new Vector3(0 , Random.Range(90,110), 0); 
    }


}
