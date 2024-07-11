using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookController : MonoBehaviour
{
    // Start is called before the first frame update

    public float OpenDistance = 1f;
    public Transform GraspPoint;

    Animator BookAnimator;

    Camera MainCamera;

    bool IsOpen, IsAnimating;
    void Start()
    {
        MainCamera = Camera.main;
        BookAnimator = GetComponent<Animator>();
    }

    public void EndOfAnimation()
    {
        Debug.Log("End of animation");
        IsAnimating = false;
    }

    Quaternion defaultRotation;

    //private void OnAttachedToHand(Hand hand)
    //{
    //    BookAnimator.SetFloat("BookSpeed", 1);
    //    //GraspPoint.transform.position = hand.skeleton.indexTip.position;
    //}

    //private void OnDetachedFromHand(Hand hand)
    //{
    //    BookAnimator.SetFloat("BookSpeed", -1);
    //}

    //void Awake()
    //{
    //    defaultRotation = transform.rotation;
    //}

    //void LateUpdate()
    //{
    //    transform.rotation = defaultRotation;
    //}

    // Update is called once per frame
    void Update()
    {
       
        if(Vector3.Distance(transform.position,MainCamera.transform.position) < OpenDistance)
        {
            //BookAnimator.SetFloat("BookSpeed", 1);
        }

        if (Vector3.Distance(transform.position, MainCamera.transform.position) > OpenDistance)
        {

           // BookAnimator.SetFloat("BookSpeed", -1);

        }
    }
}
