using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.guinealion.animatedBook;
using DG.Tweening;
using UnityEngine.Events;


public class BookReader : MonoBehaviour
{
    public LightweightBookHelper LWBH;

    public UnityEvent Open;

    public UnityEvent Close;

    public float RotateAngle = -110;

    public GameObject[] ControlArray;

    bool BookIsOpen;

    float Progess = 0, StartY;

    Transform MainCamera;

    Rigidbody RB;

    bool IsUp;
    Transform Axis;
    Vector3 RotateTo;
    private void Start()
    {
        Axis = transform;
        MainCamera = Camera.main.transform;
        RB = GetComponent<Rigidbody>();
        StartY = transform.position.y;

        RotateTo = transform.localEulerAngles;
    }

    public void PickUp()
    {
        foreach (GameObject _GO in ControlArray) _GO.SetActive(false);
    }

    public void PutDown()
    {
        foreach (GameObject _GO in ControlArray) _GO.SetActive(true);
    }

    public void BookLevitate()
    {
        
        
        if (!IsUp)
        {
            RotateTo.x = RotateAngle;
            IsUp = true;
            RB.isKinematic = true;
            transform.DOMoveY(MainCamera.transform.position.y - .1f, 2);
            transform.DOLocalRotate(RotateTo, 2);
        }

    }

    public void ReadBook()
    {
        if (!LWBH) return;
        

        if (LWBH.OpenAmmount > 0) return;

            LWBH.Progress = 10;
            //Vector3 TempVector = transform.localEulerAngles;
            //TempVector.x = -90;
            //transform.DOLocalRotate(TempVector, .5f);    
            LWBH.Open();

    }
    public void TurnPages()
    {
        if (!LWBH && LWBH.OpenAmmount < 1) return;

            LWBH.NextPage();

    }

    public void TurnPagesBack()
    {
        if (!LWBH && LWBH.OpenAmmount < 1) return;

        LWBH.PrevPage();


    }

    public void GrabBook()
    {
        //LWBH.OpenAmmount = 0;
    }

   


    public void CloseBook()
    {

        if (LWBH.OpenAmmount < 1) return;
        Vector3 TempVector = transform.localEulerAngles;
        //TempVector.x = -90;
        LWBH.Close();
    }


  
    private void Update()
    {
        if (IsUp)
        {
            float Distance = Vector3.Distance(transform.position, MainCamera.position);

            if (Distance > 1)
            {
    
                IsUp = false;
                RB.isKinematic = false;

            }
        }
       
        if ( LWBH && LWBH.OpenAmmount == 1 && !BookIsOpen)
        {
            if (Open != null) Open.Invoke();
            BookIsOpen = true;
        }

        if (LWBH && LWBH.OpenAmmount ==0  && BookIsOpen)
        {
            if (Close != null) Close.Invoke();
            BookIsOpen = false;
        }
    }

}
