using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using DG.Tweening;

public class GiraffeController : MonoBehaviour
{
    public LookAtIK LAIK;
    public AimIK AIK;

    float Timer = 0;
    public float Duration = 10;

    bool IsLooking;

    public void StartLooking()
    {
        Timer = 0;
        IsLooking = true;
        float tValue = AIK.solver.IKPositionWeight;
        DOTween.To(() => tValue, x => tValue = x, .962f, 2f).OnUpdate(() =>
        {
            AIK.solver.IKPositionWeight = tValue;
            //LAIK.solver.IKPositionWeight = tValue;
        });
    }

    public void StopLooking()
    {
        IsLooking = false;
        float tValue = AIK.solver.IKPositionWeight;
        DOTween.To(() => tValue, x => tValue = x, 0, 5f).OnUpdate(() =>
        {
            AIK.solver.IKPositionWeight = tValue;
        });
    }



    private void Update()
    {

        Timer += Time.deltaTime;

        if(Timer > Duration && IsLooking)
        {
            StopLooking();
        }

    }

}
