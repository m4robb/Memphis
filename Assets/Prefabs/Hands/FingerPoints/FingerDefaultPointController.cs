using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerDefaultPointController : MonoBehaviour
{

    float Scale = 0;

    float TimeScale = 0;

    private void Start()
    {
        Scale = transform.localScale.x * .25f;
        TimeScale = Random.Range(1, 3);
    }
    void Update()
    {

            float _Scale = Scale * Mathf.PerlinNoise(Time.time * TimeScale, 0.0f);
            Vector3 CurrentScale = new Vector3(_Scale, _Scale, _Scale);
            transform.localScale = CurrentScale;

    }
}
