using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerPointFishScale_Controller : MonoBehaviour
{
    
    void Start()
    {
        transform.localPosition = new Vector3(0.005f, 0.0032f, 0.0002f);
        transform.localEulerAngles = new Vector3(0 ,-90, -4.231f); 
    }


}
