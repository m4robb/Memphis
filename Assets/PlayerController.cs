//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Valve.VR;
//using Valve.VR.InteractionSystem;


//public class PlayerController : MonoBehaviour
//{

//    public bool DoLookAndMove;
//    public Transform Rig;
//    public Rigidbody RigRB;
//    public SteamVR_Behaviour_Pose HeadPose;
//    public SteamVR_Behaviour_Pose RightHand;
//    public SteamVR_Behaviour_Pose LeftHand;
//    public Transform Head;
//    public CharacterController RigCharacterController;
//    public AudioSource Footsteps;
//    public AudioSource AudioMic;
//    public AudioSource Breathing;

//    public float LocomotionSensitivity = 0.1f;
//    public float LocomotionMaxSpeed = 1.0f;
//    public float Gravity = 30;

//    public float LocomotionSpeed = 1.0f;

//    public MeshCollider Floor;

//    public KeeperBrain KB;

//    public SteamVR_Action_Boolean MovePress = null;
//    public SteamVR_Action_Vector2 MoveValue = null;

//    public float MovementDetectionThreshold = .1f;
//    public float NoiseThreshold = 5f;

//    // Start is called before the first frame update
//    void Start()
//    {
//        StartCoroutine(CaptureMic());
//    }

//    void DoTheLocomotionGazePoint()
//    {

        

//        Vector3 HeadDirection = Player.instance.hmdTransform.TransformDirection(new Vector3(0, 0, 1));

//        //Vector3 HandDirection = new Vector3(0, RightHand.transform.rotation.eulerAngles.y, 0);
//        //Vector3 HeadDirection =  Vector3.ProjectOnPlane(HandDirection, Vector3.up);

//        //Vector3 HeadDirection = new Vector3(0, Head.rotation.eulerAngles.y, 0);


//        Vector3 StoredPosition = Head.position;
//        Quaternion StoredRotation = Head.rotation;
//        Head.position = StoredPosition;
//        Head.rotation = StoredRotation;
//        Vector3 Movement = Vector3.zero;


//        if (MoveValue.axis.y > 0)
//        {
//            LocomotionSpeed += MoveValue.axis.y * LocomotionSensitivity;
            
//        } else
//        {
//            LocomotionSpeed -= LocomotionSensitivity;
//        }

//        LocomotionSpeed = Mathf.Clamp(LocomotionSpeed, 0f, LocomotionMaxSpeed);


//        Movement += LocomotionSpeed * HeadDirection;

//        Footsteps.volume = Mathf.Abs(LocomotionSpeed);

//        Movement.y -= Gravity * Time.deltaTime;

//        RigCharacterController.Move(Movement * Time.deltaTime);

//    }

//    void DoTheLocomotion()
//    {
        
//        Vector3 StoredPosition = Head.position;
//        Quaternion StoredRotation = Head.rotation;
//        //Rig.eulerAngles = new Vector3(0, Head.rotation.eulerAngles.y, 0);
//        Head.position = StoredPosition;
//        Head.rotation = StoredRotation;
//        //Vector3 OrientationEuler = new Vector3(0, Head.rotation.eulerAngles.y, 0);
//        Vector3 OrientationEuler = new Vector3(0, RightHand.transform.rotation.eulerAngles.y, 0);
//        Quaternion Orientation = Quaternion.Euler(OrientationEuler);
//        //Quaternion Orientation = Quaternion.Euler(HeadDirection);
//        Vector3 Movement = Vector3.zero;

//        if (MoveValue.axis.y > 0)
//        {
//            LocomotionSpeed += MoveValue.axis.y * LocomotionSensitivity;

//        }
//        else
//        {
//            LocomotionSpeed -= LocomotionSensitivity * 10f;
//        }

//        LocomotionSpeed = Mathf.Clamp(LocomotionSpeed,0, LocomotionMaxSpeed);
//        Movement += Orientation * (LocomotionSpeed * Rig.forward);
//        Footsteps.volume = Mathf.Abs(LocomotionSpeed);

//        Movement.y -= Gravity * Time.deltaTime;

//        RigCharacterController.Move(Movement * Time.deltaTime);
//    }

//    float GetAvgVol()
//    {
//        float[] data = new float[256];
//        float a = 0;
//        AudioMic.GetOutputData(data, 0);
//        foreach (float s in data)
//        {
//            a += Mathf.Abs(s);
//        }
//        return a * 10;
//    }

//    IEnumerator CaptureMic()
//    {
//        AudioMic.clip = Microphone.Start(null, true, 1, 44100);
//        AudioMic.loop = true;
//        while (!(Microphone.GetPosition(null) > 0)) { }
//        AudioMic.Play();
//        yield return null;
//    }


//    public float headsetVelocity;
//    public float rigVelocity;
//    Vector3 lastHeadsetPosition;
//    Quaternion LastHeadSetRotation;
//    public Transform headset;





//    public float MovementValue = 0;



//    void Update()
//    {
//        if(MovementValue > 0)
//        MovementValue -= 2f;

//        headsetVelocity = (headset.position - lastHeadsetPosition).magnitude / Time.deltaTime;

//        lastHeadsetPosition = headset.position;

//        if (!DoLookAndMove)
//        {
//            DoTheLocomotion();
//        }
//        else
//        {
//            DoTheLocomotionGazePoint();
//        }
       

//         //

//        if (RightHand != null)
//                MovementValue += RightHand.GetVelocity().magnitude;

//        MovementValue += RigCharacterController.velocity.magnitude;
//        MovementValue += headsetVelocity;
//       // MovementValue += GetAvgVol();
//        MovementValue = Mathf.Clamp(MovementValue, 0, 10);

//        rigVelocity = RigCharacterController.velocity.magnitude;

//       // Breathing.volume = Mathf.Clamp(MovementValue, 0.01f, 1);
//    }
//}
