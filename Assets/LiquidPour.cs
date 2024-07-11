using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidPour : MonoBehaviour
{
    public Renderer Liquid;

    float FillAmount;

    float Tilt;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         FillAmount = Liquid.material.GetFloat("_Fill");

        Tilt = Mathf.Abs(transform.up.y);

        if (Tilt < FillAmount) FillAmount -= 0.1f;
        //Debug.Log(Tilt + " ::: " + FillAmount);

            //if (Tilt > (90 - 90 * FillAmount) + 5f) FillAmount -= 0.01f;

          Liquid.material.SetFloat("_Fill", FillAmount);


    }
}
