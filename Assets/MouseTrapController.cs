using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Valve.VR;


public class MouseTrapController : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform ReParent;
    public Transform DeParent;
    //public VRShutter VRS;

    PendantController PC;
    //Interactable I;
    //Throwable T;


    Rigidbody RB;
    bool Trapped, EyesOpen;
    //Hand ThisHand;

    float ShakeSpeed;

    bool HasTriggeredTrap;

    Vector3 Newpos, Oldpos, Velocity;

    void Start()
    {

        //PC = GetComponent<PendantController>();
        //I = GetComponent<Interactable>();
        //T = GetComponent<Throwable>();
        RB = GetComponent<Rigidbody>();   
        Oldpos = transform.position;
    }

    //private void OnHandHoverBegin(Hand hand)
    //{
    //    ThisHand = hand;
    
     

    //}

    public void DeactivateTrap()
    {
            RB.isKinematic = false;
            gameObject.layer = 0;
            transform.parent = DeParent;
            //ThisHand.DetachObject(gameObject);
    }


    public void ActivateTrap()
    {
        //if (HasTriggeredTrap) return;

        //HasTriggeredTrap = true;
        Trapped = true;
       // VRS.FadeToBlack(.2f);
        StartCoroutine(DoFade());
        StartCoroutine(DoOpen());
        transform.parent = ReParent;
        RB.isKinematic = true;
        //if (I != null) I.enabled = false;
        //if (T != null) T.enabled = false;
        if (PC) PC.CanDrawLine = false;
        gameObject.layer = 8;

    }

    IEnumerator DoFade()
    {
        yield return new WaitForSeconds(1);
        EyesOpen = true;
       // VRS.FadeFromBlack(1 );
    }


    IEnumerator DoOpen()
    {
        yield return new WaitForSeconds(5);
        EyesOpen = true;

    }

    // Update is called once per frame
    void Update()
    {
        //if(ThisHand != null && Trapped == true)
        //{

        //    Newpos = transform.position;
        //    var media = (Newpos - Oldpos);
        //    Velocity = media / Time.deltaTime;
        //    Oldpos = Newpos;
        //    Newpos = transform.position;
        //    ShakeSpeed = Mathf.Round(Velocity.magnitude * 1000f) / 1000f;

        //   if(ShakeSpeed > 5 && EyesOpen)
        //    {

        //        Debug.Log(ShakeSpeed);
        //        DeactivateTrap();
        //        Trapped = false;
        //    }
        //    ReParent.position = ThisHand.skeleton.indexTip.position;
        //}
           
    }
}
