using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amniotic : MonoBehaviour
{
    public AudioSource BoingNoise;
    void Start()
    {
        
    }


    private void OnCollisionEnter(Collision collision)
    {
        BoingNoise.PlayOneShot(BoingNoise.clip);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
