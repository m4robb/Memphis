//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using RootMotion.Demos;

//public class KeeperController : MonoBehaviour
//{

//    public UserControlAI UCAI;
//    public Transform HeadTransform;
//    public Transform TargetTransform;
//    public float LOSAngle;  // width of our line of sight (x-axis and y-axis)
//    public float mTargetDetectionDistance;  // depth of our line of sight (z-axis)
//    public PlayerController PC;

//    private RaycastHit _mHitInfo;   // allocating memory for the raycasthit
//    // to avoid Garbage
//    private bool _bHasDetectedEnnemy = false;   // tracking whether the player
//    // is detected to change color in gizmos

//    public void CheckForTargetInLineOfSight()
//    {

//        Vector3 targetDir = TargetTransform.position - HeadTransform.position;
//        float angle = Vector3.Angle(targetDir, HeadTransform.forward);

//        if (angle < LOSAngle)
//           Debug.Log("In sight angle");
//        else
//            Debug.Log(angle);

//        //_bHasDetectedEnnemy = Physics.SphereCast(HeadTransform.position, mRaycastRadius, HeadTransform.forward, out _mHitInfo, mTargetDetectionDistance);

//        //if (_bHasDetectedEnnemy)
//        //{
//        //    if (_mHitInfo.transform.CompareTag("Player"))
//        //    {
//        //        Debug.Log("Detected Player");
//        //        // insert fighting logic here
//        //    }
//        //    else
//        //    {
//        //        Debug.Log("No Player detected");
//        //        // no player detected, insert your own logic
//        //    }

//        //}
//        //else
//        //{
//        //    // no player detected, insert your own logic
//        //}
//    }

//    private void LateUpdate()
//    {

  
//        Debug.DrawLine(HeadTransform.position, HeadTransform.forward * 2);

//        CheckForTargetInLineOfSight();
//    }
//}
