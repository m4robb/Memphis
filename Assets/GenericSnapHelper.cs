using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class GenericSnapHelper : MonoBehaviour
{
    public LayerMask LayerMask; 
    public UnityEvent OnSnap;

    public void CheckSnap(Transform _SnapObject)
    {

        Debug.Log("shnapps");

        //Vector3 _Closest = GetComponent<SphereCollider>().ClosestPoint(_SnapObject.position);

        //if(_Closest == _SnapObject.position)
        //{
        //    Debug.Log("Snap");
        //    if (OnSnap != null) OnSnap.Invoke();

        //    _SnapObject.position = transform.position;
        //    _SnapObject.rotation = transform.rotation;
        //}
    }





}


