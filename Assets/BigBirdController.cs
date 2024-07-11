using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using DG.Tweening;

public class BigBirdController : MonoBehaviour
{
    public RichAI RAI;
    public AIDestinationSetter AIDS;
    

    bool InFlight = true;

    FlockChildBigBird FlockChild;

    CrowWaypoints CWP;

    int WPIndex = 0;

    Animator AnimatorController;

    void Start()
    {
        RAI.enabled = false;
        AIDS.enabled = false;
        CWP = FindObjectOfType<CrowWaypoints>();
        AnimatorController = gameObject.GetComponentInChildren<Animator>();
        FlockChild = gameObject.GetComponent<FlockChildBigBird>();
    }

    public void Landed()
    {
        Debug.Log("___________________________________________________Landed");

        Vector3 FRotate = FlockChild.transform.localEulerAngles;
        FRotate.x = 0;
        FRotate.z = 0;
        FlockChild.enabled = false;
        FlockChild.transform.localEulerAngles=FRotate;
        //FlockChild._model.transform.DORotate(Vector3.zero, .1f);
        FlockChild._model.transform.DOLocalRotate(Vector3.zero, .1f);
       
        InFlight = false;

        StartCoroutine(StartWalking(Random.Range(1, 2f)));
       

    }

    Transform GetClosestWayPoint()
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Transform t in CWP.GroundPoints)
        {
            float dist = Vector3.Distance(t.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }


    void TakeOff()
    {

        Debug.Log("___________________________________________________TakeOff");
        FlockChild.enabled = true;
        FlockChild.CurrentLandingSpot.ReleaseFlockChild();
       
    }



    Vector3 relVelocity;
    bool isAtDestination;

    int StationIndex = 0;
    float WaitTime;

    IEnumerator StartWalking(float _WaitingTime)
    {
        Debug.Log("___________________________________________________StartWalking");
       
        yield return new WaitForSeconds(_WaitingTime);

        Vector3 FRotate = FlockChild.transform.eulerAngles;
        FRotate.x = 0;
        FRotate.z = 0;
        FlockChild.transform.DORotate(FRotate, 1f);
        FlockChild._model.transform.localEulerAngles = Vector3.zero;
        //FlockChild._model.transform.DOLocalRotateQuaternion(new Quaternion(0, 0, 0, 0), .01f);
        AIDS.target = GetClosestWayPoint();
        AnimatorController.CrossFadeInFixedTime("OnGround", 1f);
    }



    IEnumerator OnTargetReached(float _WaitingTime)
    {
        yield return new WaitForSeconds(_WaitingTime);

        TakeOff();

    }

    void Update()
    {
        if (InFlight)
        {
            RAI.enabled = false;
            AIDS.enabled = false;
        }
        else
        {
            RAI.enabled = true;
            AIDS.enabled = true;
            relVelocity = transform.InverseTransformDirection(RAI.velocity);
            AnimatorController.SetFloat("Forward", Mathf.Lerp(AnimatorController.GetFloat("Forward"), relVelocity.z, Time.deltaTime * 2));
            float  MaxSpeed= .3f * Mathf.PerlinNoise(Time.time * .3f, 0.0f);
            AnimatorController.SetFloat("WalkSpeed", MaxSpeed);
            RAI.maxSpeed = MaxSpeed;

            Vector3 FRotate = FlockChild._model.transform.localEulerAngles;
            FRotate.x = 0;
            FRotate.z = 0;
            FlockChild._model.transform.localEulerAngles = FRotate;

            if (RAI.reachedEndOfPath)
            {

                if (!isAtDestination)
                {
                    InFlight = true;
                    StartCoroutine(OnTargetReached(Random.Range(2, 5f)));
                }
                isAtDestination = true;
            }
            else
            {
                isAtDestination = false;
            }
        }
    }
}
