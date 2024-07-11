using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderController : MonoBehaviour
{

    public LayerMask LayerMask;
    public Rigidbody RB;

    LayerMask StartLayer;


    bool Released;

    IEnumerator SetStatic()
    {
        yield return new WaitForSeconds(1);
        RB.isKinematic = true;
    }

        private void OnCollisionEnter(Collision other)
        {

            if ((LayerMask.value & (1 << other.gameObject.layer)) != 0)
            {
            if (Released)
                {
                //Released = false;
                //StartCoroutine(SetStatic());
                }
            }

               
        }

    public void Hover()
    {
        gameObject.layer = LayerMask.NameToLayer("Grabbable");
    }
    public void PickUp()
    {
        Released = false;
        RB.isKinematic = false;
        gameObject.layer = LayerMask.NameToLayer("Grabbable");
    }

    public void PutDown()
    {
        Released = true;
        RB.isKinematic = false;
        gameObject.layer = LayerMask.NameToLayer("NavMesh");
    }
}
