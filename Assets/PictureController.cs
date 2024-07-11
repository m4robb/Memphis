using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PictureController : MonoBehaviour
{
    public  Renderer Picture;

    public void DoFadeIn()
    {

        Debug.Log("fadein");
        Picture.material.DOFade(1, 1);
    }
}
