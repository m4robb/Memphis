using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetGrab : MonoBehaviour
{
    [SerializeField] private LayerMask InteractableLayers;
    [SerializeField] private Transform GrabPoint;

    public Vector3 RotationOffset = Vector3.zero;

    public float GrabReach = .5f;

    bool IsGrabbing;

    Rigidbody GrabbedRB;

    Vector3 StoredPosition;

    bool HasReleased;

    public void EatPunch()
    {

    }

    public void DoGrab()
    {

        if (!GrabPoint) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, GrabReach);

        Debug.Log("DoGrab");

        foreach (var hitCollider in hitColliders)
        {

            if ((InteractableLayers & (1 << hitCollider.gameObject.layer)) != 0)
            {
                
                if (!hitCollider.transform.IsChildOf(transform))
                {

                    Debug.Log("found" + hitCollider.gameObject + hitCollider.gameObject.layer);

                    GrabbedRB = hitCollider.transform.gameObject.GetComponent<Rigidbody>();

                    if(!GrabbedRB) GrabbedRB =  hitCollider.transform.gameObject.GetComponentInParent<Rigidbody>();

                    if (GrabbedRB)
                    {

                        PuppetPickUpActions PPUA = GrabbedRB.GetComponent<PuppetPickUpActions>();

                        if(PPUA)
                        {
                            PPUA.PickMeUP();
                            StoredPosition = GrabPoint.position;
                            GrabbedRB.useGravity = false;
                            IsGrabbing = true;
                            break;
                        }

                        if (hitCollider.transform.gameObject.layer == 20)
                        {
                            StoredPosition = GrabPoint.position;
                            GrabbedRB.useGravity = false;
                            IsGrabbing = true;
                            break;
                        }


                    }
                }

            }

        }

        //if (Physics.SphereCast(GrabPoint.position, .01f, GrabPoint.up, out RaycastHit hit, .1f, InteractableLayers) && !IsGrabbing)
        //{
        //    Debug.Log(hit.transform.gameObject);

        //    if (hit.transform.gameObject.GetComponent<Rigidbody>())
        //    {
        //        StoredPosition = GrabPoint.position;
        //        GrabbedRB.useGravity = false;
        //        IsGrabbing = true;
        //    }

        //}
    }

    public void DoRelease()
    {
        IsGrabbing = false;

        if (GrabbedRB)
        {
            PuppetPickUpActions PPUA = GrabbedRB.GetComponent<PuppetPickUpActions>();

            if (PPUA)
            {
                GrabbedRB.isKinematic = false;
                PPUA.PutMeDown();
            }
            GrabbedRB.useGravity =true;
            GrabbedRB = null;
 
        }

    }

    void FixedUpdate()
    {

        if (IsGrabbing)
        {
            GrabbedRB.MoveRotation(Quaternion.Lerp(GrabbedRB.transform.rotation, GrabPoint.rotation * Quaternion.Euler(RotationOffset), Time.deltaTime * 5));
            GrabbedRB.MovePosition(Vector3.Lerp(GrabbedRB.transform.position, GrabPoint.position, Time.deltaTime * 5));

        }
        
    }
}
