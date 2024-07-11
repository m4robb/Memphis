using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using DG.Tweening;

public class LightFlicker : MonoBehaviour
{

    public HDAdditionalLightData HDLD;

    float StartLightIntensity;

    private void Start()
    {
        StartLightIntensity = HDLD.intensity;
    }
    void Update()
    {
        HDLD.intensity = StartLightIntensity * Mathf.PerlinNoise(Time.time * 3.5f, 0.0f);
    }
}
