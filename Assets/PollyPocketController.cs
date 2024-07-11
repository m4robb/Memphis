using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PollyPocketController : MonoBehaviour
{
    public UnityEvent OnOpen;
    public UnityEvent OnClose;
    public Transform Lid;
    public GameObject Content;
    public GameObject ContentTop;

    public Vector3 OpenPosition;
    public Vector3 ClosePosition;

    bool IsOpen, IsMoving;

    public void Close()
    {
        if (!IsOpen) return;
        Lid.DOLocalRotate(ClosePosition, 3).OnComplete(() =>
        {    
            IsOpen = false;
            IsMoving = false;
        });
    }

    public void OpenClose()
    {

        if (IsMoving) return;
        IsMoving = true;
        if (!IsOpen) {
            if (OnOpen != null) OnOpen.Invoke();
            Lid.DOLocalRotate(OpenPosition, 3).OnComplete(() =>
            {
                IsOpen = true;
                IsMoving = false;
            });

            return;
        } else
        {

            Lid.DOLocalRotate(ClosePosition, 3).OnComplete(() =>
            {
                if (OnClose != null) OnClose.Invoke();
                IsOpen = false;
                IsMoving = false;
            });
        }
    }

}
