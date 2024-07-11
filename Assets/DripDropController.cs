using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DripDropController : MonoBehaviour
{
    float Counter = 0;

    public float Lifetime = 2;
    private void Update()
    {

        Counter += Time.deltaTime;

        if(Counter > Lifetime) Destroy(gameObject);

    }

}
