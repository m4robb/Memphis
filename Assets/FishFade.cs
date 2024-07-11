using System;
using UnityEngine;
using DG.Tweening;

public class FishFade : MonoBehaviour
{
    public Renderer R;
    public Color FromColor;
    public Color ToColor;
    private bool FadeTrigger;

    private void Start()
    {
        R.material.DOColor(FromColor, 0);
    }

    public void FadeIn()
    {
        if(FadeTrigger) return;
        FadeTrigger = true;
        R.material.DOColor(ToColor, 4f).OnComplete(() =>
        {
            FadeTrigger = false;
        });
    }
    
    public void FadeOut()
    {
        if(FadeTrigger) return;
        FadeTrigger = true;
        R.material.DOColor(FromColor, 4f).OnComplete(() =>
        {
            FadeTrigger = false;
        });
    }
}
