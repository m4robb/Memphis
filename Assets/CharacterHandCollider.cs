using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterHandCollider : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor XRDI;
    public string Tag;


    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("trigger ___________________________________________" + other.tag);

        if (other.tag == Tag)
        {
            XRDI.enabled = false;

            Invoke("Recover", 2);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        Debug.Log("collide ___________________________________________" + collision.transform.tag);

        if (collision.transform.tag == Tag)
        {
            XRDI.enabled = false;

            Invoke("Recover", 2);
        }
    }

    void Recover()
    {
        XRDI.enabled = true;
    }

    void Update()
    {
        
    }
}
