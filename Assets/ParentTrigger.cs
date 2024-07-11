using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParentTrigger : MonoBehaviour
{
    public GameObject RootObject;
    public UnityEvent TriggerEvent;
    public LayerMask LayerMask;

    Transform[] _GOArray;
    void Start()
    {

        _GOArray = RootObject.GetComponentsInChildren<Transform>();

        foreach (Transform _GO in _GOArray)
        {
            ChildTrigger _CT = _GO.gameObject.AddComponent<ChildTrigger>();
            _CT.enabled = false;
            _CT.Parent = gameObject;
            StartCoroutine(EnableComponent(_GO.gameObject));

            
        }
    }
    IEnumerator EnableComponent(GameObject _GO)
    {
        yield return new WaitForSeconds(.1f);
   
        _GO.GetComponent<ChildTrigger>().enabled = true;
  
    }

    public void TriggerDetected(Collider other)
    {

        Debug.Log(other.gameObject.layer);

        if ((LayerMask.value & (1 << other.gameObject.layer)) != 0)

            if (TriggerEvent != null) TriggerEvent.Invoke();
    }
}
