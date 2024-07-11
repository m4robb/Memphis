using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootDeformerInput : MonoBehaviour
{
    [SerializeField] private float force = 10.0f;
    [SerializeField] private float forceOffset = 0.1f;

    private Vector3 lastPos; 
     void Start()
    {
        lastPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var displacement = transform.position - lastPos;
        lastPos = transform.position; 
         Ray _ray;
        RaycastHit _raycastHit;
        _ray = new Ray(this.transform.position, Vector3.down);
 
        if (Physics.Raycast(_ray, out _raycastHit, 0.7f))
        {
             Debug.DrawLine(_ray.origin, _raycastHit.point,Color.magenta);
            WaterDeformationManager deformer = _raycastHit.collider.GetComponent<WaterDeformationManager>();
            if (deformer && displacement.magnitude >=0.02f)
            {
                Vector3 point = _raycastHit.point;
                point += _raycastHit.normal * forceOffset;
                deformer.ApplyDeformation(point, force);
            }
        }
    }
}