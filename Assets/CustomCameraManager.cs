using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Events;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.XR.OpenXR;
using Unity.XR.CoreUtils;


#if UNITY_PS5
using PlaystationInput = UnityEngine.PS5.PS5Input;
#endif

public class CustomCameraManager : MonoBehaviour
{
    public XROrigin XRO;
    public string NextScene = "";
    public float DelayTime = 1;
    public float OffsetSpeed = .5f;

    public InputActionReference CameraSlide = null;
    public InputActionReference HMDUserPresence = null;
    public Renderer Shutter;
    public CameraCapture ScreenShotCamera;
    public CanvasGroup InterfaceScreen;
    public GameObject InterfaceScreenPrefab;
    public Transform CameraTransform;
    public Color ShutterColor = new Color(0,0,0);
    public Volume ShutterVolume;
    public float TiltValue = 30;
    public float StandHeight = 1.2f;
    public float CrouchHeight = .6f;
    public UnityEvent OnFadeIn;
    public UnityEvent OnFadeOut;
    public UnityEvent OnSpecialButtonIsPressed;
    public UnityEvent OnCustomShutterEvent;
    public UnityEvent OnHeadSetOn;
    public UnityEvent OnHeadSetOff;
    public InputActionReference SpecialButton = null;
    public InputActionReference CameraButton = null;
    public bool CustomShutter;

    List<UnityEngine.XR.InputDevice> HeadDevices;
    List<UnityEngine.XR.InputDevice> LeftHandDevices;

    int ClunkIndex = 0;

    float TempAdjust = 0;

    float StoredCameraAngle;

    float CameraHeightModifier = 0;

    Vector3 PitchAdj;
    Camera MainCamera; 
    private void Awake()
    {
        AudioListener.pause = false;
        Time.timeScale = 1;
        Shutter.gameObject.SetActive(true);
        Shutter.material.DOFade(1, 0f);

         
      
      
    }

    private void FixfloorOrigin()
    {
        List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems<XRInputSubsystem>(inputSubsystems);
        foreach (var inputSubsystem in inputSubsystems)
        {
            if (inputSubsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor)) { }
            if (inputSubsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device)) { }
        }
    }

    void Start()
    {
      
        MainCamera = Camera.main;

        HeadDevices = new List<UnityEngine.XR.InputDevice>();
        LeftHandDevices = new List<UnityEngine.XR.InputDevice>();

        if (MainCamera)
            MainCamera.transform.GetComponent<HDAdditionalCameraData>().allowDynamicResolution = false;



        XRO.CameraYOffset = StandHeight;


        //if (HC != null) TempAdjust = HC.TargetDistance;

        AudioListener.pause = false;
        Time.timeScale = 1;
  

    }

    IEnumerator LoadChunk(int _Index)
    {

        Shutter.material.DOFade(0, 1).OnComplete(() => { Shutter.gameObject.SetActive(false); });

 
        
        yield return new WaitForSeconds(DelayTime);

    }


    bool CameraUp, CameraDown;


        public void ChangeGravity( float _GravityY)
        {
            Physics.gravity = new Vector3(0, _GravityY, 0);
        }




    bool LeftGripIsPulled, LeftGripPull, LeftGripValue, InterfaceSceenIsOn;

    bool IsLeftInHand, IsLeftInHandValue;

    bool FadeToNextSceneTrigger;

    public bool HMDOn, HeadTriggerValue;
    public bool IsStandalone;

    int FreezeMargin = 0;

    IEnumerator ExecuteSceneChangeLimbo(string _Scene)
    {


        Shutter.gameObject.SetActive(true);
        ShutterColor.a = 0;
        Shutter.material.color = ShutterColor;

        yield return new WaitForSeconds(0.1f);

 

        Shutter.material.DOFade(1, 1f).OnComplete(() =>
        {
            if (FallSceneManager.FallSceneManagerInstance && FallSceneManager.FallSceneManagerInstance.LimboDestination != "")
            {

                    FallSceneManager.FallSceneManagerInstance.LoadNextScene(FallSceneManager.FallSceneManagerInstance.LimboDestination);
                    FallSceneManager.FallSceneManagerInstance.LimboDestination = "";
                    return;

                }
                else
                {
                FallSceneManager.FallSceneManagerInstance.ClearPersistentAudio();
                FallSceneManager.FallSceneManagerInstance.LoadNextScene(_Scene);
                }

        });


    }

    IEnumerator ExecuteSceneChange(string _Scene)
    {
        

        Shutter.gameObject.SetActive(true);
        ShutterColor.a = 0;
        Shutter.material.color = ShutterColor;

        yield return new WaitForSeconds(0.1f);

        Debug.Log("go to  " + _Scene);

        FallSceneManager.FallSceneManagerInstance.ClearPersistentAudio();

        Shutter.material.DOFade(1, .2f).OnComplete(() =>
        {
            if (FallSceneManager.FallSceneManagerInstance)
            {
                Debug.Log("RRRRRRRRRRRRRRRRRRRRRRRRRRRRRR");

              

                FallSceneManager.FallSceneManagerInstance.LoadNextScene(_Scene);
               
            }

        });


    }

    IEnumerator ExecuteJumpSceneChange(string _Scene)
    {
        yield return new WaitForSeconds(2f);

        if (FallSceneManager.FallSceneManagerInstance) FallSceneManager.FallSceneManagerInstance.LoadNextScene(_Scene);

    }

    bool JumpToNextSceneTrigger;

    public void CustomShutterOpen()
    {
        Shutter.material.DOFade(0, 2f).SetDelay(.5f).OnComplete(() =>
        {
            Shutter.gameObject.SetActive(false);
            if (OnFadeIn != null) OnFadeIn.Invoke();

        });
    }

    public void CustomShutterClose()
    {   
        Shutter.gameObject.SetActive(true);
        Debug.Log("custom close");
        Shutter.material.DOFade(1, 2f).OnComplete(() =>
        {
            
            if (OnFadeIn != null) OnFadeOut.Invoke();

        });
    }

    public void JumpToNextScene(string _Scene)
    {

        if (JumpToNextSceneTrigger) return;

       JumpToNextSceneTrigger = true;

        foreach (AudioLooper _AL in AudioGridTick.AudioGridInstance.AudioLoops)
        {
            _AL.QuickFade();
        }

        foreach (PlayLongSoundTimed _PL in AudioGridTick.AudioGridInstance.LongSounds)
        {
            _PL.QuickFade();
        }

        StartCoroutine(ExecuteJumpSceneChange(_Scene));


    }


    public void FadeToNextSceneLimbo(string _Scene)
    {



        if (FadeToNextSceneTrigger) return;

        FadeToNextSceneTrigger = true;

        foreach (AudioLooper _AL in AudioGridTick.AudioGridInstance.AudioLoops)
        {
            _AL.QuickFade();
        }

        foreach (PlayLongSoundTimed _PL in AudioGridTick.AudioGridInstance.LongSounds)
        {
            _PL.QuickFade();
        }

        StartCoroutine(ExecuteSceneChangeLimbo(_Scene));



    }
    public void FadeToNextScene(string _Scene)
    {

       

        if (FadeToNextSceneTrigger) return;

        FadeToNextSceneTrigger = true;

        foreach (AudioLooper _AL in AudioGridTick.AudioGridInstance.AudioLoops)
        {
            _AL.QuickFade();
        }

        foreach (PlayLongSoundTimed _PL in AudioGridTick.AudioGridInstance.LongSounds)
        {
            if(!_PL.IsPersistent) _PL.QuickFade();
        }

        StartCoroutine(ExecuteSceneChange(_Scene));



    }


    public void SetCameraHeight(bool _IsPlus)
    {


        //if (HC == null) return;

        //if (_IsPlus)
        //{

        //    TempAdjust += OffsetSpeed * Time.deltaTime;

        //}
        //else
        //{

        //    TempAdjust -= OffsetSpeed * Time.deltaTime;

        //}

        //HC.TargetDistance = Mathf.Clamp(TempAdjust, 0f, 1f);

    }

    bool IsManaged;

    GameObject GUIOverlay;
    void GoToSleep()
    {
        GUIOverlay = Instantiate(InterfaceScreenPrefab);
        InterfaceScreen.gameObject.SetActive(true);
        //AudioListener.pause = true;
        //Time.timeScale = 0;
    }


    float LeftPullTimer = 0;

    int LeftPullCount = 0;

    bool HMDTrigger;

    GameObject ScreenOverlay;

    private void DoCanvas()
    {
        GUIOverlay = Instantiate(InterfaceScreenPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }


    public int m_PlayerId = 2;



    bool ButtonTrigger;

private void LateUpdate()
    {

        //Gyro();

        //bool ButtonPushed1 = false, ButtonValue1 = false, ButtonPushed2 = false, ButtonValue2 = false;


        //foreach (UnityEngine.XR.InputDevice device in LeftHandDevices)
        //{

        //    ButtonPushed1 = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out ButtonValue1) && ButtonValue1;

        //}


        //if (ButtonPushed1)
        //{
        //    if (ButtonTrigger) return;

        //    Debug.Log("PressCameraButton");
        //    ButtonTrigger = true;
        //    StartCoroutine(ScreenShotCamera.ExecuteShot());
        //}

        //if (!ButtonPushed1)
        //{
        //    ButtonTrigger = false;
        //}
    }

        void Gyro()
        {
            // Make the gyro rotate to match the physical controller
#if UNITY_PS5

            Vector3 ControllerRotation = new Vector3(-PlaystationInput.PadGetLastOrientation(m_PlayerId).x,
                                                        -PlaystationInput.PadGetLastOrientation(m_PlayerId).y,
                                                        PlaystationInput.PadGetLastOrientation(m_PlayerId).z) * 100;
#endif
             
        }
    bool SpecialButtonIsPushed, CameraButtonIsPushed, IsCrouching;

    public float fixDuration = 3;
    float fixStart = 0;

    private void Update()
    {






        LeftPullTimer += Time.deltaTime;
        if (LeftPullTimer > 2)
        {
            LeftPullCount = 0;
        }


        if (FallSceneManager.FallSceneManagerInstance && !IsManaged)
        {
            IsManaged = true;
            if (NextScene != "") FallSceneManager.FallSceneManagerInstance.PreloadSceneStart(NextScene);

           

            if (FallSceneManager.FallSceneManagerInstance.TVEG)
            {
                FallSceneManager.FallSceneManagerInstance.TVEG.enabled = true;

            }
            IsStandalone = FallSceneManager.FallSceneManagerInstance.IsStandalone;

            FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment = PlayerPrefs.GetFloat("CameraHeight");

            Debug.Log(FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment);

            StandHeight += FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment;

            CrouchHeight += FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment;

            XRO.CameraYOffset = StandHeight;

            return;
        }

        if (HeadDevices.Count == 0)
        {
            HeadDevices = new List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.Head, HeadDevices);
        }

        if (LeftHandDevices.Count == 0)
        {

            LeftHandDevices = new List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, LeftHandDevices);
        }

        foreach (UnityEngine.XR.InputDevice device in HeadDevices)
        {


            HMDOn = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out HeadTriggerValue) && HeadTriggerValue;

            if (CameraTransform.localRotation.x - StoredCameraAngle == 0)
            {
                FreezeMargin++;
            }
            else
            {
                FreezeMargin = 0;
            }
            StoredCameraAngle = CameraTransform.localRotation.x;
            if (FreezeMargin > 5)
            {

                if (InterfaceScreenPrefab && !InterfaceScreenPrefab.activeSelf)
                    InterfaceScreenPrefab.SetActive(true);
                MainCamera.enabled = false;
                HMDOn = false;
                return;
            }
            else
            {
                MainCamera.enabled = true;
                if (InterfaceScreenPrefab && InterfaceScreenPrefab.activeSelf)
                    InterfaceScreenPrefab.SetActive(false);
                HMDOn = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out HeadTriggerValue) && HeadTriggerValue;
            }

        }

        if (IsStandalone) HMDOn = true;




        if (HMDOn && !HMDTrigger)
        {
            HMDTrigger = true;

            Debug.Log("HMDOn");

            InterfaceSceenIsOn = false;

            if (OnHeadSetOn != null) OnHeadSetOn.Invoke();


            if (!CustomShutter)
            {
                Shutter.material.DOFade(0, .2f).SetDelay(.5f).OnComplete(() =>
                {
                    Shutter.gameObject.SetActive(false);
                    if (OnFadeIn != null) OnFadeIn.Invoke();


                });
            } else
            {
                if (OnCustomShutterEvent != null) OnCustomShutterEvent.Invoke();
            }
            if (MainCamera)
                MainCamera.transform.GetComponent<HDAdditionalCameraData>().allowDynamicResolution = true;
            if(InterfaceScreenPrefab)
                InterfaceScreenPrefab.SetActive(false);

        }

        if (!HMDOn &&  HMDTrigger)
        {


            Debug.Log("HMDOFF");

            HMDTrigger = false;
            InterfaceSceenIsOn = true;
            if (OnHeadSetOff != null) OnHeadSetOff.Invoke();
            if (InterfaceScreenPrefab)
                InterfaceScreenPrefab.SetActive(true);

        }







        if (CameraButton != null)
        {
            if (CameraButton.action.IsPressed() && !CameraButtonIsPushed)
            {
                CameraButtonIsPushed = true;

                StartCoroutine(ScreenShotCamera.ExecuteShot());
            }



            if (!CameraButton.action.IsPressed()) CameraButtonIsPushed = false;
        }






        if (SpecialButton != null)
        {
            if(SpecialButton.action.IsPressed() && !SpecialButtonIsPushed)
            {
                SpecialButtonIsPushed = true;
                if (!IsCrouching)
                {

                    float tValue = StandHeight;
                    DOTween.To(() => tValue, x => tValue = x, CrouchHeight, 1f).OnUpdate(() =>
                    {
                        XRO.CameraYOffset = tValue;
                    });
                  
                     IsCrouching = true;
                    return;
                }
                
                if (IsCrouching)
                {

                    float tValue = CrouchHeight;
                    DOTween.To(() => tValue, x => tValue = x, StandHeight, 1f).OnUpdate(() =>
                    {
                        XRO.CameraYOffset = tValue;
                    });
                    //XRO.CameraYOffset = 1f;
                    //HC.VerticalMultiplier = 1f;
                    IsCrouching = false;
                    //HC.TargetDistance = TempAdjust;
                }
  
            }   
            


            if (!SpecialButton.action.IsPressed()) SpecialButtonIsPushed = false;
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {

            Debug.Log(FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment);

            FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment += .02f;

            PlayerPrefs.SetFloat("CameraHeight", FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment) ;

            StandHeight += .02f;
            CrouchHeight += .02f;

            XRO.CameraYOffset = StandHeight;

           
        }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {

            Debug.Log(FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment);

            FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment -= .02f;

            PlayerPrefs.SetFloat("CameraHeight", FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment);

            StandHeight -= .02f;
            CrouchHeight -= .02f;

            XRO.CameraYOffset = StandHeight;
           

        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            FadeToNextScene("RainySquare"); 
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            FadeToNextScene("SleepingManRoom01");
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            FadeToNextScene("Pool");
        }


        if (Keyboard.current.cKey.wasPressedThisFrame)
        {

            Debug.Log(FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment);

            FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment = 0;

            PlayerPrefs.SetFloat("CameraHeight", FallSceneManager.FallSceneManagerInstance.CameraHeightAdjustment);

            StandHeight = FallSceneManager.FallSceneManagerInstance.DefaultStandHeight;

            CrouchHeight = FallSceneManager.FallSceneManagerInstance.DefaultCrouchHeight;

            XRO.CameraYOffset = StandHeight;


        }



        if (fixStart == -1) return;

        if ((fixStart + fixDuration) <= Time.time)
        {
            fixStart = -1;
            Debug.Log("Floor tracking origin fix stop");
        }
        else
        {
            FixfloorOrigin();
        }
        // if(SpecialButton.action.IsPressed() && SpecialButtonIsPushed)
        // { 
        //     HC.TargetDistance = TempAdjust;
        // }




    }

}
