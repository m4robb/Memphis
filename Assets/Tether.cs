using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tether : MonoBehaviour
{
    public UnityEvent OnTetherReached;
    public float TetherLength = 4;
        
        public bool IsReady;

    Vector3 StartPosition;
    void Start()
    {
        StartPosition = transform.position;
    }

    public void MakeReady()
    {
        IsReady = true;
    }

    void Update()
    {

       
        if (Vector3.Distance(StartPosition, transform.position) > TetherLength && IsReady)
        {
            if (OnTetherReached != null) OnTetherReached.Invoke();
        }
    }
}
