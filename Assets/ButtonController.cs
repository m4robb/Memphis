using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class ButtonController : MonoBehaviour
{

    public UnityEvent OnPress = null;
    public bool IsNegative;
    public float PushThreshold;

    Vector3 StartPosition;
    Vector3 CurrentPosition;
    Rigidbody RB;
    void Start()
    {
        StartPosition = transform.position;
        RB = GetComponent<Rigidbody>();
    }

    bool Trigger;

    private void OnCollisionExit(Collision collision)
    {
        OnPress.Invoke();
        Trigger = true;
    }
    void Update()
    {

        CurrentPosition = transform.position;

        //Debug.Log(Mathf.Abs(StartPosition.z - CurrentPosition.z));

        //if (Mathf.Abs(StartPosition.z - CurrentPosition.z) > PushThreshold && !Trigger)
        //{
        //    OnPress.Invoke();
        //    Trigger = true;

        //}

        if(Trigger) RB.position = Vector3.Lerp(CurrentPosition, StartPosition, Time.deltaTime * 10f);

        if (Mathf.Abs(StartPosition.z - CurrentPosition.z) == 0) Trigger = false;

        




    }
}
