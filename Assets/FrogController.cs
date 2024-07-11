//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
////using Valve.VR;

//public class FrogController : MonoBehaviour
//{
//    // Start is called before the first frame update

//    public Transform PlayerCamera;
//    //public SteamVR_Behaviour_Pose RightHand;
//    //public SteamVR_Behaviour_Pose LeftHand;
//    public Animator FrogAnimator;


//    void Start()
//    {
        
//    }

//    private RaycastHit _mHitInfo;   // allocating memory for the raycasthit
//    Vector3 RelativeDirection;

//    // Update is called once per frame
//    void Update()
//    {

//        float DistanceToHand = Vector3.Distance(transform.position, RightHand.transform.position);

//        RelativeDirection = PlayerCamera.position - transform.position;

//        if (Physics.Raycast(transform.position, RelativeDirection, out _mHitInfo, 100))
//        {
//            if (_mHitInfo.transform.CompareTag("Player") && DistanceToHand < .2f)
//            {
//                FrogAnimator.Play("Jump");
//                Debug.Log("hello");
//            }
//        }

//    }
//}
