using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdTrapController : MonoBehaviour
{
    ConfigurableJoint CJ;
    void Start()
    {
        CJ = GetComponent<ConfigurableJoint>();

    }

    public void  DropTrap(GameObject Toucher)
    {
        if(Toucher.name == "Club" && CJ != null)
            CJ.breakForce = .5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
