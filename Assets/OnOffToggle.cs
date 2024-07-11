using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffToggle : MonoBehaviour
{
    public GameObject TargetGO;
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        TargetGO.SetActive(!TargetGO.activeSelf);
    }

    private void OnTriggerEnter(Collider other)
    {
        TargetGO.SetActive(!TargetGO.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
