using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManInGreyController : MonoBehaviour
{
    // Start is called before the first frame update
    bool CanSee;
    public float Threshold = 20;
    public Transform ManInGrey;

    Transform MainCamera;

    bool HasSeen;

    public void LookAt()
    {
        CanSee = true;
    }

    void Update()
    {
        if (!MainCamera)
        {
            MainCamera = Camera.main.transform;
            return;
        }

        if (HasSeen) return;

        float _Angle = Vector3.Angle(ManInGrey.position, MainCamera.forward);

        if (CanSee && _Angle < Threshold)
        {
            HasSeen = true;
            ManInGrey.gameObject.SetActive(true) ;
        }
    }
}
