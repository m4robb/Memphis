using UnityEngine;

public class DoFCalculator : MonoBehaviour
{
    public LayerMask mask = -1;


    public Camera MainCamera;
    void Start()
    {
        
    }

    RaycastHit hit;

    void Update()
    {

         if (Physics.SphereCast(transform.position, .05f, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, mask.value))
         //if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, mask.value))
        {

            MainCamera.focusDistance = hit.distance;

 

        }
    }
}
