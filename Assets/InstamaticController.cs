using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstamaticController : MonoBehaviour
{
    public Transform FollowThis;
    public Rigidbody RB;
    public AudioSource CameraClick;

    int Exposures = 0;

    Vector3 TempRotation;

    public void TakePhoto()
    {
        CameraClick.PlayOneShot(CameraClick.clip);
    }

    void Update()
    {
        TempRotation.x = 0;
        TempRotation.z = 0;
        TempRotation.y = FollowThis.localEulerAngles.y;
        transform.eulerAngles = TempRotation;
    }
}
