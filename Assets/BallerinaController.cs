using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallerinaController : MonoBehaviour
{
    // Start is called before the first frame update

    public float speed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }
}
