using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionAction : MonoBehaviour
{
    // Start is called before the first frame update

    public UnityEvent Touched;
    public UnityEvent UnTouched;

    public string LayerName = "Player";

    bool IsHandInteraction = true;
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (IsHandInteraction && collision.collider.gameObject.layer == LayerMask.NameToLayer(LayerName))
        {
            Touched.Invoke();
        }
        
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsHandInteraction && collision.collider.gameObject.layer == LayerMask.NameToLayer(LayerName))
        {
            UnTouched.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
