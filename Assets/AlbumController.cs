using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlbumController : MonoBehaviour
{

    public GameObject Record;


    float AngleX = 0;

    IEnumerator RemoveFromSleeve()
    {
        yield return new WaitForSeconds(.45f);
        Record.transform.parent = transform.parent;
        Record.GetComponent<MeshRenderer>().enabled = true;
    }
    void LateUpdate()
    {

        //AngleX = transform.localEulerAngles.x;
        //AngleX = (AngleX > 180) ? AngleX - 360 : AngleX;

        //if (AngleX < -70)
        //{

        //    Record.GetComponent<MeshRenderer>().enabled = false;
        //    Record.GetComponent<Rigidbody>().isKinematic = false;
        //    Record.SetActive(true);
        //    StartCoroutine(RemoveFromSleeve());
            
        //}
    }
}
