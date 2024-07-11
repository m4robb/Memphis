using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamingoController : MonoBehaviour
{
    public Rigidbody RB;
    void Start()
    {
        RB.AddForce(2, 0, 2);
        RB.AddRelativeTorque(0, -1, 0);
    }


}
