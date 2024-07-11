using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using RootMotion.FinalIK;

public class CrocoController : MonoBehaviour
{
    public Animator CrocAnimator;
    public LookAtIK LAIK;
    public AudioSource Voice;
    public AudioSource StepSound;
    public UnityEvent OnRoar;

    bool Trigger;

    public void LookAtTarget(Transform _Target)
    {

        if (Trigger) return;
        if (!LAIK) return;
        Trigger = true;
        LAIK.solver.target = _Target;
        float Value = 0;
        DOTween.To(() => Value, x => Value = x, 1, 2).OnUpdate(() => {
            LAIK.solver.IKPositionWeight = Value;
        }).OnComplete(()=> {

            Invoke("StopStaring", 4f);

        });

    }



    public void Roar()
    {

        if(OnRoar != null)
        {
            OnRoar.Invoke();
        }
       
    }

    public void Step()
    {

        if(Voice != null)
        {
        Voice.PlayOneShot(StepSound.clip);
        }
       
    }

    public void StopStaring()
    {
        if (!LAIK) return;
        float Value = LAIK.solver.IKPositionWeight;
        DOTween.To(() => Value, x => Value = x, 0, 2).OnUpdate(() =>
        {
            LAIK.solver.IKPositionWeight = Value;
        }).OnComplete(()=> { Trigger = false; });
    }

    public void CrossFadeAnimation(string _Anim)
    {
        CrocAnimator.CrossFadeInFixedTime(_Anim, 2f);
    }
}
