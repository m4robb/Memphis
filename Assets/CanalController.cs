using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using RootMotion.FinalIK;
using UnityEngine.Rendering;

public class CanalController : MonoBehaviour
{
    public GameObject Crocodile;
    public LookAtIK PunchLAIK;
    public Volume ItsRaining;
    public UnityEvent ONCompleteCycle;
    float LightIntensity;
    public Renderer Shutter;
    public void InitateCrocodile()
    {
        StartCoroutine(ActivateCrocodile());
    }


    bool PunchTrigger;



    public void ReduceLight()
    {
        float tValue = 0.001f;


        Shutter.material.DOFade(0, 0);
        Shutter.gameObject.SetActive(true);

        DOTween.To(() => tValue, x => tValue = x, 1f, 10f).OnUpdate(() =>
         {
             ItsRaining.weight = tValue;
         }).OnComplete(() => {
             Shutter.material.DOFade(1, 10).SetDelay(10).OnComplete(()=> {

                 Debug.Log("Go to next Scene");
                 if (ONCompleteCycle != null) ONCompleteCycle.Invoke();
             });
         });
    }

    public void InitatePunch()
    {

        if (PunchTrigger) return;
        PunchTrigger = true;

        float tValue = 0.001f;

        DOTween.To(() => tValue, x => tValue = x, 1f, 1f).OnUpdate(() =>
        {
            PunchLAIK.solver.IKPositionWeight = tValue;
        });

    }


    IEnumerator ActivateCrocodile()
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());

        Crocodile.SetActive(true);

    }

    IEnumerator ActivatePunch()
    {

        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());

        float tValue = 0.001f;

        DOTween.To(() => tValue, x => tValue = x, 1f, 1f).OnUpdate(() =>
        {
            PunchLAIK.solver.IKPositionWeight = tValue;
        });
    }

}
