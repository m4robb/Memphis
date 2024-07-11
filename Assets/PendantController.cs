using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendantController : MonoBehaviour
{
    // Start is called before the first frame update

    public LineRenderer LR;
    public Transform EndPoint;
    public Transform StartPoint;
    public ConfigurableJoint CJ;

    public bool CanDrawLine = true;


    void Start()
    {
       //CJ.linearLimit.limit = Vector3.Distance(StartPoint.position,EndPoint.position) ;

    }

    void LateUpdate()
    {

        if (!CanDrawLine) return;
        LR.SetPosition(0, StartPoint.localPosition + new Vector3(0,.07f,0));
        LR.SetPosition(1, EndPoint.localPosition);
    }
    void Update()
    {
        
    }
}
