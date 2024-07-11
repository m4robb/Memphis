using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class KettleController : MonoBehaviour
{
    public VisualEffect WaterVFX;
    public float PourAngle = -45;
    int ParticleCount = 20;
    void Start()
    {
        ParticleCount = WaterVFX.GetInt("ParticleCount");
        WaterVFX.SetInt("ParticleCount", 0);
    }

    // Update is called once per frame
    void Update()
    {

         float CurrentAngle = 360 - transform.eulerAngles.x;
        if (CurrentAngle > PourAngle && CurrentAngle < 180)
        {

            
            WaterVFX.SetInt("ParticleCount", ParticleCount);
        }
        else
        {
            WaterVFX.SetInt("ParticleCount", 0);
        }
    }
}
