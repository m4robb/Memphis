using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicHeadTarget : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform target;
    public float lerpSpeed = 6;

    void Start()
    {
        transform.position = target.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, target.position + target.up * -0.1f, Time.deltaTime * lerpSpeed);
        transform.rotation = target.rotation;
    }
}