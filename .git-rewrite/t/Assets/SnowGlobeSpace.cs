using UnityEngine;
using DG.Tweening;

public class SnowGlobeSpace : MonoBehaviour
{
    public Renderer R;


    public void DoSpace()
    {
        R.gameObject.SetActive(true);
        R.material.DOFade(.3f, 2);
    }

}
