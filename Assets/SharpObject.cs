using UnityEngine;
using UnityEngine.Events;

public class SharpObject : MonoBehaviour
{
    public UnityEvent SharpObjectIncident;

    bool HasDone;

    public LayerMask layerMask;


    private void OnCollisionEnter(Collision other)
    {

        if (HasDone) return;

        HasDone = true;
        Debug.Log("Cut");
        if ((layerMask.value & (1 << other.gameObject.layer)) != 0)
            Debug.Log("Cut2");
        if (SharpObjectIncident != null) SharpObjectIncident.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (HasDone) return;

        HasDone = true;

        Debug.Log("Cut");

        if ((layerMask.value & (1 << other.gameObject.layer)) != 0)
            Debug.Log("Cut2");
        if (SharpObjectIncident != null) SharpObjectIncident.Invoke();
    }

    }


