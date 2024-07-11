using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class SnapHelper : MonoBehaviour
{
    public LayerMask LayerMask;

    public UnityEvent OnSnap;

    public TurntableArmController TAC;

    public delegate void StoredSnapAction();

    public static SnapHelper SnapHelperInstance;

    public event StoredSnapAction storedSnapAction;

    bool Trigger;

    private void Start()
    {
        SnapHelperInstance = this;
    }

    void CheckDisc(GameObject _Disc)
    {

        DiscController _DC = _Disc.GetComponent<DiscController>();
        if (_DC) _DC.SetupTracks();
    }

    public void CheckSnap(Transform _SnapObject)
    {

        if (TAC && TAC.IsPlaying) return;

        Vector3 _Closest = GetComponent<SphereCollider>().ClosestPoint(_SnapObject.position);

        if(_Closest == _SnapObject.position)
        {

            if (OnSnap != null) OnSnap.Invoke();

            if (storedSnapAction != null)
            {
                storedSnapAction();
            }

            CheckDisc(_SnapObject.gameObject);
            _SnapObject.position = transform.position;
            _SnapObject.rotation = transform.rotation;
        }
    }





}


