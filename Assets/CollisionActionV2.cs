using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionActionV2 : MonoBehaviour
{
    // Start is called before the first frame update
    public LayerMask layerMask;
    public float DelayTime = 0;
    public UnityEvent Touched;
    public UnityEvent UnTouched;

 

    bool IsHandInteraction = true;


    void Start()
    {
        
    }

    IEnumerator DoEntry()
    {
        yield return new WaitForSeconds(DelayTime);
        if (Touched != null)
            Touched.Invoke();

    }

    IEnumerator DoExit()
    {
        yield return new WaitForSeconds(DelayTime);
        if (Touched != null)
            UnTouched.Invoke();

    }

    private void OnCollisionEnter(Collision collision)
    {

        if ((layerMask.value & (1 << collision.transform.gameObject.layer)) != 0)
        {
            StartCoroutine(DoEntry());
        }
        
    }

    private void OnCollisionExit(Collision collision)
    {
        if ((layerMask.value & (1 << collision.transform.gameObject.layer)) != 0)
        {
            StartCoroutine(DoExit());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
