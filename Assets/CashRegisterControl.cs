using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CashRegisterControl : MonoBehaviour
{
    public Transform Drawer;

    public Vector3 ClosePosition;
    public Vector3 OpenPosition;
    public GameObject Contents;
    bool IsOpen;
    bool IsMoving;

    public void OpenClose()
    {
        if (IsMoving) return;
        IsMoving = true;
        if (!IsOpen)
        {

            
            Contents.SetActive(true);
            Drawer.DOLocalMove(OpenPosition, 2).OnComplete(()=> {
                IsMoving = false;
            });
            IsOpen = true;
            return;
        }
        else
        {
            Drawer.DOLocalMove(ClosePosition, 2).OnComplete(()=> {
                IsMoving = false;
            
            });
            IsOpen = false;
        }
    }
}
