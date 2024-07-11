using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootController : MonoBehaviour
{
    public ConstantForce CF;

    public void CollideWith()
    {

        Vector3 CurrentForce = CF.force;
        CurrentForce.y += .1f;
        CF.force = CurrentForce;
    }
}
