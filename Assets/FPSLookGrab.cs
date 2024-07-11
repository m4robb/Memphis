using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#if UNITY_PS5
using PlaystationInput = UnityEngine.PS5.PS5Input;
#endif

public class FPSLookGrab : MonoBehaviour
{



    [SerializeField] private Transform PlayerCamera;
    public Transform RightHand;
    [SerializeField] private LayerMask InteractableLayers;
    [SerializeField] private CanvasGroup Reticule;
    public InputActionReference SelectTrigger = null;

    [HideInInspector]

    public Vector3 ControllerRotation;

    [HideInInspector]

    public Quaternion DirectionToObject; 
    public Transform HoldPosition;
    public Transform ReturnToPosition;
    public float ReachDistance = 1.5f;

    float StoredReachDistance;

    public int m_PlayerId = 2;

    public AudioSource AS, VS;






    void Start()
    {

        StoredReachDistance = ReachDistance;

    #if UNITY_EDITOR
        m_PlayerId = 2;
        //Application.targetFrameRate = 60;
#endif
#if UNITY_PS5
        m_PlayerId = Enumerable.Range(0, 3).Single(i =>  PlaystationInput.GetUsersDetails(i).userId == UnityEngine.PS5.Utility.initialUserId);
            VS.gamepadSpeakerOutputType = GamepadSpeakerOutputType.Vibration;
            Debug.Log(m_PlayerId);
#endif
  







    }



    void Gyro()
    {
        // Make the gyro rotate to match the physical controller

#if UNITY_PS5
          ControllerRotation = new Vector3(-PlaystationInput.PadGetLastOrientation(m_PlayerId).x,
                                                    -PlaystationInput.PadGetLastOrientation(m_PlayerId).y,
                                                    PlaystationInput.PadGetLastOrientation(m_PlayerId).z) * 100;
#endif
        //Debug.Log(ControllerRotation);
    }


    public bool RightTriggerPulled;

    Transform StoredParent;

    void CheckTrigger()
    {
        

        if (SelectTrigger && SelectTrigger.action.ReadValue<float>() != 0)
        {
            if (FPCI)
            {
                if (!RightTriggerPulled)
                {

                    RightHand.gameObject.SetActive(true);

                    float DistanceToObject = Vector3.Distance(FPCI.transform.position, PlayerCamera.position) - .4f;

                    if (DistanceToObject < ReachDistance)
                    {
                        Vector3 TVal = HoldPosition.localPosition;
                        TVal.z = Mathf.Clamp(DistanceToObject, .25f, .4f);
                        HoldPosition.localPosition = TVal;
                    }

                    Vector3 Size = GetComponent<Collider>().bounds.size;


                    ReturnToPosition.position = FPCI.transform.position;

                    StoredParent = FPCI.transform.parent;
                    FPCI.transform.parent = HoldPosition;
                    Offset = HoldPosition.localPosition;
                    Offset.y = -Size.y * .35f;
                    HoldPosition.localPosition += Offset;
                    Reticule.alpha = 0;

                    //RightHand.localPosition = FPCI.transform.localPosition;



                    RightTriggerPulled = true;
                    FPCI.RB.useGravity = false;
                }

               
                FPCI.HoldObject(this);
            }

        }
        else
        {

            if (RightTriggerPulled)
            {
                RightHand.gameObject.SetActive(false);
                FPCI.transform.parent = StoredParent;
                FPCI.RB.useGravity = true;
                FPCI.DropObject();
                RightTriggerPulled = false;
                Reticule.alpha = .1f;
                HoldPosition.localPosition = new Vector3(0,0,.4f);
                FPCI = null;
            }
           
        }
 
    }

    private FPCInteractable FPCI;

    Vector3 Offset;

    void CheckRaycast()
    {


        if (Physics.SphereCast(PlayerCamera.position,.1f, PlayerCamera.forward, out RaycastHit hit, ReachDistance, InteractableLayers))
        {

          

           FPCInteractable _FPCI = hit.transform.GetComponent<FPCInteractable>();

            if (_FPCI)
            {
                if (_FPCI != FPCI)
                {
                    Offset = hit.transform.position - hit.point;
                    Offset.z = 0;
                    FPCI = _FPCI;
                    Reticule.alpha = 1;


#if UNITY_PS5

                    Debug.Log("Found Item");
                        VS.PlayOnGamepad(m_PlayerId); 
#endif

                }
            }
            else
            {
                Reticule.alpha = .1f;
            }
        }
        else
        {
            if (!RightTriggerPulled)
            {
                FPCI = null;
                
            }
                
            Reticule.alpha = .1f;
        }



    }


    private void FixedUpdate()
    {
        
    }

    void Update()
    {
        //Gyro();

        Gyro();

        CheckTrigger();

        CheckRaycast();
    }
}
