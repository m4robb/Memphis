using UnityEngine;
using DG.Tweening;

public class QuickFade : MonoBehaviour
{

    public AudioSource AS;

    public void DoQuickFadeOut(float _Duration)
    {
        AS.DOFade(0, _Duration);
    }
}
