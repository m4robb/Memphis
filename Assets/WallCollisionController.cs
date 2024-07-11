using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using DG.Tweening;

public class WallCollisionController : MonoBehaviour
{
    // Start is called before the first frame update

    public Renderer ShutterRenderer;

    public Volume ppGlobalVolume; // Ref to the PostProcessing Volume
    private ColorParameter cp = null;

    private IEnumerator coroutine;

    enum FadingDirection { FadeIn, FadeOut }


    private void Awake()
    {
        ColorAdjustments colorAdjustments = null;
        if (!ppGlobalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            Debug.LogWarning("No color adjustments found!");
        }
        else
            cp = colorAdjustments.colorFilter;
    }

    void Start()
    {
        Collides.a = 1;
        NotCollides.a = 0;
    }

    Color Collides = Color.white;
    Color NotCollides = Color.white;

    private void OnCollisionEnter(Collision collision)
    {

        cp.value = Color.white * -5;
        ShutterRenderer.material.color = Collides;

    }

    private void OnCollisionStay(Collision collision)
    {
        cp.value = Color.white * -5;
        ShutterRenderer.material.color = Collides;
    }

    private void OnCollisionExit(Collision collision)
    {
        cp.value = Color.white;
            ShutterRenderer.material.color = NotCollides;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
