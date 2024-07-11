using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class StageBackdop : MonoBehaviour
{
    public Transform RCurtain;
    public Transform LCurtain;

    public UnityEvent OnOpen;

    bool AreMoving;

    bool HasOpenedCurtains;

    public void OpenCurtains()
    {

        if (AreMoving) return;
        AreMoving = true;
        if (OnOpen != null && !HasOpenedCurtains) OnOpen.Invoke();
        HasOpenedCurtains = true;
        RCurtain.DOScaleZ(.2f, 2).OnComplete(()=>
        {
           
            AreMoving = false;
        });

        LCurtain.DOScaleZ(.25f, 2);
    }

    public void CloseCurtains()
    {

        if (AreMoving) return;
        AreMoving = true;
        RCurtain.DOScaleZ(1,4).OnComplete(() =>
        {
            AreMoving = false;
        });
        LCurtain.DOScaleZ(1, 2);

    }
}
