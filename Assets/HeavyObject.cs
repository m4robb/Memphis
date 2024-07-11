using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HeavyObject : MonoBehaviour
{
    public Transform GrapplePoint;

    public LayerMask layerMask;


    private void OnTriggerEnter(Collider other)
    {
 
        if ((layerMask.value & (1 << other.transform.gameObject.layer)) != 0)
        {

            GrapplePoint.position = other.transform.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        Debug.Log(layerMask.value);
        if ((layerMask.value & (1 << collision.transform.gameObject.layer)) != 0)
        {
            Debug.Log("yo");
            GrapplePoint.position = collision.transform.position;
        }
    }

}