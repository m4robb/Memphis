using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockAddTarget : MonoBehaviour
{
    // Start is called before the first frame update
    public FlockController FC;
    public Transform Target;
    public void AddTarget()
    {
        FC._flockTarget = Target;
    }
}
