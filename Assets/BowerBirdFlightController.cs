using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowerBirdFlightController : MonoBehaviour
{
    public Animator Anim;

    float Timer;

    float NextTime = 5;

    bool GoLeft;

    void Update()
    {

        Timer += Time.deltaTime;

        if (Timer >= NextTime)
        {
           
            Timer = 0;
            NextTime = Random.Range(2, 4);
            if (GoLeft)
            {
                Anim.CrossFadeInFixedTime("TurnLeft", 1f);
                GoLeft = false;
                return;
            }
            if (!GoLeft)
            {
                Anim.CrossFadeInFixedTime("TurnRight", 1f);
                GoLeft = true;
                return;
            }


        }
    }
}
