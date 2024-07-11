using UnityEngine;
using System.Collections;
using RealisticEyeMovements;
using RootMotion.Dynamics;

public class HeadWeightController : MonoBehaviour
{
    public EyeAndHeadAnimator EAHA;

    public float StartWeight = 0;

    public float Delay = 2;

    public float WeightLength = 10;



    bool HasTriggered;

    void Start()
    {
        if (EAHA == null) EAHA = GetComponent<EyeAndHeadAnimator>();
        EAHA.headWeight = StartWeight;
      
    }


    IEnumerator ExecuteHeadWeight(float _Weight)
    {
        yield return new WaitForSeconds(Delay);
        EAHA.headWeight = _Weight;
        StartCoroutine(ExecuteRemoveHeadWeight());

    }

    IEnumerator ExecuteRemoveHeadWeight()
    {
        
        yield return new WaitForSeconds(WeightLength);
        EAHA.headWeight = 0;

    }


    public void SetHeadWeight(float _Weight)
    {
        if (HasTriggered) return;
        HasTriggered = true;
        StartCoroutine(ExecuteHeadWeight(_Weight));
        
    }

    public void RemoveHeadWeight()
    {
        StartCoroutine(ExecuteHeadWeight(0));
    }
}
