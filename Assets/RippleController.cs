using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RippleController : MonoBehaviour
{
    public Renderer Water;

    public LayerMask layerMask;

    public AudioSource Splash;
    void Start()
    {
        Water.material.SetFloat("_NormalStrength", .2f);
        Water.material.SetFloat("_PanSpeed", .02f);

        
    }
    private void OnTriggerEnter(Collider other)
    {
        if ((layerMask.value & (1 << other.transform.gameObject.layer)) > 0)
            {

            Splash.PlayOneShot(Splash.clip);
            //Water.material.DOFloat(.4f, "_NormalStrength", 1);
            //Water.material.DOFloat(.1f, "_PanSpeed", 1);            
            Water.material.SetFloat("_NormalStrength", .3f);
            Water.material.SetFloat("_PanSpeed", .1f);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        Water.material.SetFloat("_NormalStrength", .2f);
        Water.material.SetFloat("_PanSpeed", .02f);

        //Water.material.DOFloat(.2f, "_NormalStrength", 1);
        //Water.material.DOFloat(.02f, "_PanSpeed", 1);
    }

    float WaterTrigger = 100;

    void Update()
    {
        if(Random.Range(0, 500) == 1)
        {
            Splash.PlayOneShot(Splash.clip);
        }
    }
}
