using UnityEngine;
using DG.Tweening;

public class BirdInCage : MonoBehaviour
{
    public Animator BirdAnimator;
    public Transform FlightPosition;
    public PlayLongSoundTimed PLST;

    public Klak.Motion.BrownianMotion BM;

    public float FlightTime;

    Vector3 StartPosition;
    Vector3 StartRotation;

    float Timer = 0;

    bool IsInFlight;

    float QuantizedFlightTime = 0;
    
    void Start()
    {
        QuantizedFlightTime = 0;
        StartPosition = transform.localPosition;
        StartRotation = transform.localEulerAngles;
       
    }

    public void TakeFlight()
    {
        IsInFlight = true;
        Timer = 0;
        BirdAnimator.SetTrigger("Fly");
        PLST.PlayLongSound();
        transform.DOLocalMove(FlightPosition.localPosition, .2f).OnComplete(()=> {
            BM.enabled = true;
        });
    }
    public void ReturnToRoost()
    {
        BM.enabled = false;
        transform.DOLocalRotate(StartRotation,.2f);
        PLST.EndLongSound();
        transform.DOLocalMove(StartPosition, 60 / (float)AudioGridTick.AudioGridInstance.bpm ).OnComplete(() =>
        {
            BirdAnimator.SetTrigger("Land");
        });
    }
    // Update is called once per frame
    void Update()
    {

        if(AudioGridTick.AudioGridInstance && QuantizedFlightTime == 0)
        {
            QuantizedFlightTime = 60 / (float)AudioGridTick.AudioGridInstance.bpm  * FlightTime;
        }
        
        if (IsInFlight)
        {
            Timer += Time.deltaTime;
            if(Timer > QuantizedFlightTime) {
                ReturnToRoost();
                IsInFlight = false;
            }
        }
        
    }
}
