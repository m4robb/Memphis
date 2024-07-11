using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LifeClockController : MonoBehaviour
{

    public Transform LeftCurtain;
    public Transform RightCurtain;
    public GameObject Contents;

    bool CurtainMoving;

 public void OpenCurtain()
    {

        if (CurtainMoving) return;
        CurtainMoving = true;


        LeftCurtain.DOScaleZ(.1f, 2);
        RightCurtain.DOScaleZ(.1f, 2).OnComplete(()=> {
            CurtainMoving = false;
        });
        Contents.SetActive(true);
    }

    public void CloseCurtain()
    {
        if (CurtainMoving) return;

        CurtainMoving = true;
        LeftCurtain.DOScaleZ(1f, 2);
        RightCurtain.DOScaleZ(1, 2).OnComplete(()=> {
            CurtainMoving = false;
            Contents.SetActive(false);
        });
    }

}
