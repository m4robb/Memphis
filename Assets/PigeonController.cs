using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class PigeonController : MonoBehaviour
{
    public Transform FlightTarget;
    public Animator PigeonAnimator;
    public Transform Predator;

    public float Perimeter = 4;
    public float Offset = .5f;

    public UnityEvent TakeffAction;

   // public AudioSource TakeOff;
   // public AudioSource Cooing;

    bool Target;

    void OnEnable()
    {
        PigeonAnimator.SetFloat("Offset", Offset);
    }

    public void TakeFlight()
    {
        //PigeonAnimator.SetTrigger("TakeOff");
        //TakeOff.Play();
    }

    IEnumerator TakeFlightTimed(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        if (TakeffAction != null) TakeffAction.Invoke();
    }

    public void GoTowardsTarget()
    {
        Target = true;
    }

    Vector3 TargetDirection;
    float DistanceToPredator;
    void Update()
    {
        if(!Target) DistanceToPredator = Vector3.Distance(transform.position, Predator.position);

        if(DistanceToPredator < Perimeter && !Target)
        {
            Target = true;
            StartCoroutine(TakeFlightTimed((float) AudioGridTick.AudioGridInstance.GetDelayTime()));
        }
        if (Target && FlightTarget!= null)
        {

            Vector3 targetDirection = FlightTarget.position - transform.position;


            float singleStep = 2 * Time.deltaTime;


            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);


            transform.rotation = Quaternion.LookRotation(newDirection);

        }
    }
}
