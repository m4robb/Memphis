using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ToiletFlush : MonoBehaviour
{
    

    bool IsFlushing;

    float Interval;

    IEnumerator DoFlushTimed()
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        transform.DOLocalMoveY(-0.3381f, 3* Interval).SetDelay(1 * Interval).OnComplete(() =>
        {
            transform.DOLocalMoveY(-0.1852f, 6 * Interval).SetDelay(2 * Interval)  .OnComplete(() => { IsFlushing = false; });
        });
    }

    public void DoFlush()
    {

        Interval = 60 /(float)AudioGridTick.AudioGridInstance.bpm;
        if (IsFlushing) return;
        IsFlushing = true;
        StartCoroutine(DoFlushTimed());
    }
}
