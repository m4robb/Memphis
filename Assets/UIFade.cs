using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIFade : MonoBehaviour
{
    public float DelayFade1;
    public float DelayFade2;
    CanvasGroup CG;
    void Start()
    {
        if (GetComponent<CanvasGroup>() != null)
        {
            CG = GetComponent<CanvasGroup>();
            CG.DOFade(1, 2).SetDelay(DelayFade1).OnComplete(() =>
            {
                CG.DOFade(0, 2).SetDelay(DelayFade2);
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
