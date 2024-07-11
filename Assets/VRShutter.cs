using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Valve.VR;


namespace Valve.VR
{


public class VRShutter : MonoBehaviour
{
    Renderer R;
    void Start()
    {
        R = GetComponent<Renderer>();
    }

    public void FadeToBlack(float _Duration = 1)
    {
        R.material.DOFade(1, _Duration);
    }
    public void FadeFromBlack(float _Duration = 1)
    {
        R.material.DOFade(0, _Duration);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
}
