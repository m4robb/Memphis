using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideContainerController : MonoBehaviour
{
 

    public Collider ContainerCollider;
    public Transform NewParent;

    bool IsReParented;

    ////Interactable I;
    ////Throwable T;

    //void Start()
    //{
    //    I = GetComponent<Interactable>();
    //    T = GetComponent<Throwable>();
    //    if (I != null) I.enabled = false;
    //    if (T != null) T.enabled = false;
    //}

    //bool IsPointWithinCollider(Collider collider, Vector3 point)
    //{
    //    return (collider.ClosestPoint(point) - point).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
    //}

    //// Update is called once per frame
    //void Update()
    //{

    //    //Debug.Log(IsPointWithinCollider(ContainerCollider, transform.position));

    //    if(!IsPointWithinCollider(ContainerCollider, transform.position) && !IsReParented)
    //    {
    //        transform.parent = NewParent;
    //        IsReParented = true;
    //        if (I != null) I.enabled = true;
    //        if (T != null) T.enabled = true;
    //    }

    //}
}
