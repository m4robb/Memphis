using UnityEngine;
using DG.Tweening;
using RootMotion.FinalIK;

public class LookWeightController : MonoBehaviour
{
    public LookAtIK LAIK;
    public float DurationMultiplier = 2f;

    float Duration = 1;
    void Start()
    {
        LAIK.solver.IKPositionWeight = 0;
    }


    public void SetWeight(float weight)
    {
        float tValue = LAIK.solver.IKPositionWeight;
        DOTween.To(() => tValue, x => tValue = x, weight, Duration).OnUpdate(() =>
        {
            LAIK.solver.IKPositionWeight = tValue;
        });
    }

    bool IsLooping;
    void Update()
    {
        if (AudioGridTick.AudioGridInstance != null && !IsLooping)
        {
            IsLooping = true;
            Duration = 60 / (float)AudioGridTick.AudioGridInstance.bpm * DurationMultiplier;
        }
    }

}
