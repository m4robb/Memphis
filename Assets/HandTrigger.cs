using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTrigger : MonoBehaviour
{
    public SphereCollider[] Colliders;

    public void TurnOn()
    {
        foreach(SphereCollider _SC in Colliders)
        {
            _SC.enabled = true;
        }
    }

    public void TurnOff()
    {
        foreach (SphereCollider _SC in Colliders)
        {
            _SC.enabled = false;
        }
    }
}
