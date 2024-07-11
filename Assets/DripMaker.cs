using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DripMaker : MonoBehaviour
{

    public GameObject DripDrop;


    public void MakeDrop()
    {
        Instantiate(DripDrop, transform.position, transform.rotation);
    }
}
