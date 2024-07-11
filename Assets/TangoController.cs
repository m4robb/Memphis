using UnityEngine;
using DG.Tweening;
using NUnit.Framework;

public class TangoController : MonoBehaviour
{
    public Animator TangoAnimator;

    float Duration = 1;
    void OnEnable()
    {
        TangoAnimator.SetFloat("Speed", 0);
    }


    public void SetAnimationSpeed(float speed)
    {
        float tValue = TangoAnimator.GetFloat("Speed");
        DOTween.To(() => tValue, x => tValue = x, speed, Duration).OnUpdate(() =>
        {

            TangoAnimator.Play("MainAction");
            TangoAnimator.SetFloat("Speed", tValue);
        });
    }
    bool IsLooping;
    void Update()
    {
        if (AudioGridTick.AudioGridInstance != null && !IsLooping)
        {
            IsLooping = true;
            Duration = 60 / (float)AudioGridTick.AudioGridInstance.bpm * 2f;
        }
        }
}
