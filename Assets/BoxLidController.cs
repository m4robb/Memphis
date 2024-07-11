using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoxLidController : MonoBehaviour
{
  

    public void OpenCloseLid(float _Dir)
    {
        transform.DOLocalMove(new Vector3(_Dir, transform.localPosition.y, transform.localPosition.z), 2);
    }
}
