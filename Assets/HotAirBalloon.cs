using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using DG.Tweening;

public class HotAirBalloon : MonoBehaviour
{
    public Light Burner;
    public AudioSource AS;
    HDAdditionalLightData HDLD;


    float BaseLightStrength;

    bool DoFlicker;

    private void Start()
    {
        HDLD = Burner.GetComponent<HDAdditionalLightData>();
        BaseLightStrength = Burner.intensity;
        Burner.enabled = false;

    }

    IEnumerator StopLight()
    {
        yield return new WaitForSeconds(3.0f);


        float tValue = HDLD.intensity;
        DOTween.To(() => tValue, x => tValue = x, 0f, .5f).OnUpdate(() =>
        {
            Burner.intensity = tValue;
        }).OnComplete(()=> {
            DoFlicker =false;
        });
    }

    public void DoBurn(bool IsTouching = false)
        {

        if(IsTouching)
        {
            AS.PlayOneShot(AS.clip);
        }
        Burner.enabled = true;
        Burner.intensity = 0;
 
        float tValue = 0;
        DOTween.To(() => tValue, x => tValue = x, BaseLightStrength, .5f).OnUpdate(() =>
        {
            Burner.intensity = tValue;
        }).OnComplete(() => {
            DoFlicker = true ;
        });

        StartCoroutine(StopLight());

        }

    private void Update()
    {
        if (DoFlicker)
        {
            Burner.intensity = BaseLightStrength * Mathf.PerlinNoise(Time.time * 20, 0.0f);
        }
    }
}
