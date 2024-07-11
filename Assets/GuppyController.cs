using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GuppyController : MonoBehaviour
{

    public Animator Anim;

    public void EnterWater()
    {

        float Value = Anim.speed;

        DOTween.To(() => Value, x => Value = x, 1, 240 / (float)AudioGridTick.AudioGridInstance.bpm).OnUpdate(() => {
            Anim.speed = Value;
        });

    }
}
