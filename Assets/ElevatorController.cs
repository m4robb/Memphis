using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using DG.Tweening;

public class ElevatorController : MonoBehaviour
{
    public Animator DoorAnimation;
    public AudioSource OpenSound;
    public AudioSource ElevatorMotor;
    public AudioSource Ping;
    public Transform PlayerOrigin;
    public GameObject Lights;
    public UnityEvent OnCloseDoor;
    public UnityEvent OnMIGExit;

    public GameObject[] Segments;

    float StartPosition;

    Transform MainCamera;
    void Start()
    {
        StartPosition = PlayerOrigin.position.y;

        MainCamera = Camera.main.transform;
        //Lights.SetActive(false);
        //foreach (GameObject GO in Segments)
        //{
        //   GO.SetActive(false);
        //}
    }


    bool DoorTrigger, MIGExitTrigger;


    public IEnumerator OpenDoor(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        Lights.SetActive(true);
        OpenSound.Play();
        DoorTrigger = false;
        DoorAnimation.Play("DoorAnimationOpen");
    }

    public void MIGExit()
    {
        MIGExitTrigger = true;
        //
       
    }
    bool MIGHasLeftTheBuilding;

    public void MIGForceLeave()
    {
       if(!MIGHasLeftTheBuilding)
        {
            StartCoroutine(MIGExitExecute());
        }
    }
    public IEnumerator MIGExitExecute()
    {

        MIGHasLeftTheBuilding = true;
        yield return new WaitForSeconds(1 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);
        Ping.Play();
        yield return new WaitForSeconds(1 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);
        DoorAnimation.Play("DoorAnimationClose");
        yield return new WaitForSeconds(3 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);
        Lights.SetActive(false);
        if (OnMIGExit != null) OnMIGExit.Invoke();
    }

    public IEnumerator CloseDoor(float _Delay)
    {
        OpenSound.Play();
        yield return new WaitForSeconds(2 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);
        DoorAnimation.Play("DoorAnimationClose");
        yield return new WaitForSeconds(4 * 60 / (float)AudioGridTick.AudioGridInstance.bpm);
        Lights.SetActive(false);
        if (OnCloseDoor != null) OnCloseDoor.Invoke();
        //ElevatorMotor.Play();
        //Lights.SetActive(false);
        //FallSceneManager.FallSceneManagerInstance.StartCoroutine(FallSceneManager.FallSceneManagerInstance.LoadSceneAsyncProcess("Hinterhof01"));

    }

    public void ElevatorJump(float _Strength)
    {
        ElevatorMotor.Pause();
            PlayerOrigin.DOLocalMoveY(StartPosition - _Strength, .5f).SetEase(Ease.OutBounce).OnComplete(() =>
             {
                 ElevatorMotor.Play();
                 PlayerOrigin.DOLocalMoveY(StartPosition , .5f).SetDelay(.3f).SetEase(Ease.OutBounce);
             });


    }
    public void DoOpen(float _Delay)
    {
        if (DoorTrigger) return;
        DoorTrigger = true;
        StartCoroutine(OpenDoor((float)AudioGridTick.AudioGridInstance.GetDelayTime()));

    }

    public void DoClose(float _Delay)
    {

        Debug.Log("got here");
        if (DoorTrigger) return;
        DoorTrigger = true;
        StartCoroutine(CloseDoor((float)AudioGridTick.AudioGridInstance.GetDelayTime()));

    }

    bool Trigger;
    void Update()
    {
        float _Angle = Vector3.Angle(transform.position, MainCamera.forward);

        if (!Trigger && _Angle < 15 && MIGExitTrigger)
        {

           
            Trigger = true;
            StartCoroutine(MIGExitExecute());
        }

    }
}
