using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LadderSystemController : MonoBehaviour
{
    public Transform TopMarker;
    public Transform BottomMarker;
    public Transform Step;
    public float TransportTime = 5;
    public float DelayTime = 5;
    public string TransportAxis = "Z";
    public bool UseThreshold;
    public float Threshold;
    public Transform PlayerRig;


    HeightController HC;
    Vector3 StartPosition;
    void Start()
    {
        StartPosition = Step.localPosition;
        HC = PlayerRig.gameObject.GetComponent<HeightController>();
    }
   public  void TransportUp()
    {

        if (UseThreshold && PlayerRig.localPosition.y > Threshold) return;
        DOTween.KillAll();
        HC.enabled = false;
        PlayerRig.DOMoveY(TopMarker.position.y, TransportTime).SetDelay(1f).SetEase(Ease.Linear);
   

        //HC.enabled = false;
        //if(TransportAxis == "Z")
        //    PlayerRig.DOLocalMoveZ(TopMarker.localPosition.z, TransportTime).SetDelay(1f).SetEase(Ease.Linear);
        //if (TransportAxis == "X")
        //    PlayerRig.DOLocalMoveX(TopMarker.localPosition.x, TransportTime).SetDelay(1f);
        //if (TransportAxis == "Y")
        //    PlayerRig.DOLocalMoveY(TopMarker.localPosition.y, TransportTime).SetDelay(1f);
    }
    public void TransportDown()
    {
        DOTween.KillAll();
        HC.enabled = true;

        //if (TransportAxis == "Z")
        //    PlayerRig.DOLocalMoveZ(StartPosition.z, .1f).SetDelay(DelayTime);
        //if (TransportAxis == "X")
        //    PlayerRig.DOLocalMoveX(StartPosition.x, .1f).SetDelay(DelayTime);
        //if (TransportAxis == "Y")
        //    PlayerRig.DOLocalMoveY(StartPosition.y, .1f).SetDelay(DelayTime);

    }


}
