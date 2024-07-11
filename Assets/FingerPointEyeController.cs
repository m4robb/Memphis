using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class FingerPointEyeController : MonoBehaviour
{
    Transform LookTarget;
    void Start()
    {
        LookTarget = gameObject.GetComponentInParent<HandAnimator>().transform;

        Debug.Log(LookTarget);
    }

    // Update is called once per frame
    void Update()
    {
        if (LookTarget) transform.LookAt(LookTarget.position + LookTarget.forward * .2f);
    }
}
