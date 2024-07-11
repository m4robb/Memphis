using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

using UnityEngine.Events;

public class TurntableArmController : MonoBehaviour
{

    public SyncSounds StereoController;
    public Animator Turntable;
    public AudioSource SFXSource;
    public AudioClip[] ClipArray;

    public Transform SocketTransform;

    public bool IsPlaying = false, IsMoving;

    public UnityEvent StartRecord;
    public UnityEvent SwitchOn;
    public UnityEvent SwitchOff;

    float Interval;

    bool CanMakeNoise, StartedPlaying;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable XRGI;

    public Transform CurrentDisc;



    public void ChooseRecord(Transform _SelectedDisc)
    {
        CurrentDisc = _SelectedDisc;
    }

    public void DoSnap()
    {
        if (!CurrentDisc) return;

        CurrentDisc.position = SocketTransform.position;
        CurrentDisc.eulerAngles = SocketTransform.eulerAngles;
    }

    IEnumerator TurnOffSFX(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        SFXSource.enabled = false;
    }

    IEnumerator MakeArmOff()
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        Vector3 V1 = transform.localEulerAngles;
        V1.x = -3.695f;

        if (SwitchOff != null) SwitchOff.Invoke();
        //StereoController.SwitchOff();
        SFXSource.PlayOneShot(ClipArray[0]);
        transform.DOLocalRotate(V1, Interval * 2).OnComplete(() =>
         {
             V1 = transform.localEulerAngles;
             V1.y = 88.281f;
             Debug.Log("arm off");
             SFXSource.PlayOneShot(ClipArray[0]);
             transform.DOLocalRotate(V1, Interval * 2).OnComplete(() =>
             {
                 Turntable.speed = 0;

                 CurrentDisc.GetComponent<Rigidbody>().isKinematic = true;
                 IsMoving = false;

                 if (XRGI) XRGI.enabled = true;
                 SFXSource.PlayOneShot(ClipArray[0]);
                 StartCoroutine(TurnOffSFX(ClipArray[0].length));
             });

             });
    }



    IEnumerator MakeArmOn()
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        Vector3 V1 = transform.localEulerAngles;
        V1.y = 111.461f;
        SFXSource.PlayOneShot(ClipArray[0]);
        transform.DOLocalRotate(V1, Interval * 2).OnComplete(() =>
        {
            Turntable.speed = 1;

            CurrentDisc.GetComponent<Rigidbody>().isKinematic = true;
            // Turntable.GetComponent<BoxCollider>().enabled = false;
            V1 = transform.localEulerAngles;
            V1.x = 0f;
            Debug.Log("arm on");
            SFXSource.PlayOneShot(ClipArray[0]);
            transform.DOLocalRotate(V1, Interval * 2).OnComplete(() =>
            {
                SFXSource.PlayOneShot(ClipArray[0]);
                if (SwitchOn != null) SwitchOn.Invoke();
                StereoController.SwitchOn();
                IsMoving = false;

                Debug.Log("arm oncomple");

                if (XRGI) XRGI.enabled = false;
                StartCoroutine(TurnOffSFX(ClipArray[0].length));
            });

        });

    }


    public void MoveArm()
    {

        if (IsMoving) return;
        Debug.Log("IsMOving " + CurrentDisc);

        if (XRGI == null && CurrentDisc.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>()!=null)
        {
           
            XRGI = CurrentDisc.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }



        if (Vector3.Distance(CurrentDisc.position,SocketTransform.position) > .01f) return;

        Debug.Log("HasMoved " + CurrentDisc);

        SFXSource.enabled = true;

        if (IsPlaying)
        {
            IsMoving = true;
            IsPlaying = false;
            StartCoroutine(MakeArmOff());
            return;
        }

        if (!IsPlaying)
        {
            IsMoving = true;
            IsPlaying = true;
            StartCoroutine(MakeArmOn());
            return;
        }
    }


 
    void Update()
    {
        if (AudioGridTick.AudioGridInstance != null && !CanMakeNoise)
        {
            Turntable.speed = 0;
            Interval = 60 / (float)AudioGridTick.AudioGridInstance.bpm;
            CanMakeNoise = true;
        }
    }
}
