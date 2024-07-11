using UnityEngine;
using UnityEngine.Events;

public class ArtifactConvertor : MonoBehaviour
{
    public GameObject Artifact;
    public UnityEvent OnTriggerEnterAction;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == Artifact && OnTriggerEnterAction!= null)
        {
            OnTriggerEnterAction.Invoke();
        }
    }
}
