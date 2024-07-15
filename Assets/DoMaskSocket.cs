using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class DoMaskSocket : MonoBehaviour
{

    public XRInteractionManager XRIM;
    public XRSocketInteractor XRSI;

   

    public XRGrabInteractable XRGI;
    public float SnapDistance = .4f;
    public Vector3 ScaleTo;
    public UnityEvent OnSnap;
    public UnityEvent OnSnapRelease;
    private Vector3 InitialScale;
    void Start()
    {
        InitialScale = transform.localScale;
        XRSI.socketActive = false;
    }

    private bool InSocket;

    public void Grab()
    {
        Debug.Log("Grabbing");

        SocketController.SocketControllerInstance.StoredSocketSnapEvent += OnSnapReceive;
        SocketController.SocketControllerInstance.StoredSocketReleaseEvent += OnSnapReleaseReceive;

        XRSI.enabled = true;
        IsGrabbing = true;
    }

    public void Release()
    {

        IsGrabbing = false;
    }

    public void Deactivate()
    {
        XRSI.enabled = false;
        XRSI.socketActive = false;
    }

    public void SetScale()
    {
        Debug.Log("Set Scale to " + ScaleTo);
        transform.DOScale(ScaleTo, .2f);
    }

    public void DropItem()
    {

        Debug.Log("DROP ITEM FROM SOCKET");

        SnapTrigger = false;
        XRSI.enabled = false;
        XRSI.socketActive = false;

        //IXRSelectInteractor iXRSI = XRSI;
        //XRIM.SelectExit(iXRSI, XRGI);
    }



    public void ResetScale()
    {
        Debug.Log("Reset Scale to " + InitialScale);
        transform.localScale = InitialScale;
        SocketController.SocketControllerInstance.StoredSocketSnapEvent -= OnSnapReceive;
        SocketController.SocketControllerInstance.StoredSocketReleaseEvent -= OnSnapReleaseReceive;
    }

    public void OnSnapReceive()
    {
        Debug.Log("SnapRecieve");
        if (OnSnap != null) OnSnap.Invoke();
    }

    public void OnSnapReleaseReceive()
    {

        if (OnSnapRelease != null) OnSnapRelease.Invoke();
    }




    private bool IsGrabbing;
    private IXRSelectInteractable MySelect;

    private bool IsReadyToReceive;

    void LetGoForSnap()
    {
        MySelect = XRGI;
        Debug.Log(MySelect);
        XRSI.StartManualInteraction(MySelect);
        Invoke("StopManualInteraction", .2f);
    }

    void StopManualInteraction()
    {
        XRSI.EndManualInteraction();
    }

    public bool SnapTrigger;

    // Update is called once per frame
    void Update()
    {


        if (IsGrabbing)
        {
            if (Vector3.Distance(transform.position, XRSI.transform.position) < SnapDistance)
            {

                if (!SnapTrigger)
                {
                    XRSI.enabled = true;
                    XRSI.socketActive = true;
                    SnapTrigger = true;
                    MySelect = null;
                    if (XRGI) Invoke("LetGoForSnap", .1f);
                }

            }


            else
            {
                SnapTrigger = false;
                XRSI.enabled = false;
                XRSI.socketActive = false;
            }
        }


    }
}

