using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WindowController : MonoBehaviour
{
    public Renderer R;
    public float Range;
    public GameObject Darkroom;
    public GameObject Giraffe;
    public Transform HeadPosition;


    float Distance;
    bool CloseTrigger;

    Vector3 BusStartPosition;

    Material Mat;

    void Start()
    {
        Mat = R.material;
        Darkroom.SetActive(false);

    }

    void Update()

       
    {
        Distance = Vector3.Distance(transform.position, HeadPosition.position);


        if (Distance < Range)
        {

            Mat.SetFloat("_AlphaCutoff", Mathf.Clamp( (1-Distance / Range) * 3, 0, 1));
            Darkroom.SetActive(true);
        }
        else
        {

            Darkroom.SetActive(false);
            Mat.SetFloat("_AlphaCutoff", 0);
        }
    }
}
