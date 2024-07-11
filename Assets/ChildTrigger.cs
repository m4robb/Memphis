    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildTrigger : MonoBehaviour
{
    public GameObject Parent;

    private void OnTriggerEnter(Collider other)
    {
        Parent.GetComponent<ParentTrigger>().TriggerDetected(other);
    }
}
