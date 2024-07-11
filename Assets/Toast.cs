using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Toast : MonoBehaviour
{

    bool HasPopped;

    IEnumerator PlayOneShot(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
       


        
        transform.DOLocalRotate(new Vector3(Random.Range(0, 45), Random.Range(0, 45), Random.Range(0, 45)), _Delay);
        transform.DOLocalRotate(new Vector3(Random.Range(0, 45), Random.Range(0, 45), Random.Range(0, 45)), _Delay);
        transform.DOLocalMoveY(.6f, _Delay).OnComplete(() => {

            Rigidbody _RB = GetComponent<Rigidbody>();
 
            if (_RB) _RB.isKinematic = false;

            XRGrabHand _XGH = GetComponent<XRGrabHand>();

            if (_XGH) _XGH.enabled = true;
        });

    }
    public void Pop()
    {
        if (HasPopped) return;
        HasPopped = true;
        StartCoroutine(PlayOneShot((float)AudioGridTick.AudioGridInstance.GetDelayTime()));

    }
}
