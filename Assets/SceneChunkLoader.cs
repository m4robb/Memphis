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


#if UNITY_PS5
using PlaystationInput = UnityEngine.PS5.PS5Input;
#endif

public class SceneChunkLoader : MonoBehaviour
{
    public GameObject[] Chunks;
    public string NextScene = "";
    public float DelayTime = 1;
    public HeightController HC;
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

    public UnityEvent OnFadeIn;
    public UnityEvent OnFadeOut;
    public UnityEvent OnSpecialButtonIsPressed;
    public UnityEvent OnCustomShutterEvent;
    public UnityEvent OnHeadSetOn;
    public XRBaseController LeftHand1;
    public XRBaseController LeftHand2;

    public XRBaseController RightHand1;
    public XRBaseController RightHand2;

    public FPSLookGrab FPSLG;

    public float WindPower = 0;

    public InputActionReference CameraPitch = null;
    public InputActionReference SpecialButton = null;
    public bool IsMultiPass;
    public bool CustomShutter;

    List<UnityEngine.XR.InputDevice> RightHandDevices;
    List<UnityEngine.XR.InputDevice> LeftHandDevices;
    List<UnityEngine.XR.InputDevice> HeadDevices;

    int ClunkIndex = 0;

    float TempAdjust = 0;

    float StoredCameraAngle;

    Vector3 PitchAdj;
    Camera MainCamera; 
    private void Awake()
    {
        AudioListener.pause = false;
        Time.timeScale = 1;
        Shutter.gameObject.SetActive(true);


        Shutter.material.DOFade(1, 0f);

       
      
       
        // yield return
      



        //XRGeneralSettings.Instance.Manager.InitializeLoader();
    }

    void Start()
    {
        var openXRSettings = OpenXRSettings.Instance;

        if (IsMultiPass)
        {
            openXRSettings.renderMode = OpenXRSettings.RenderMode.MultiPass;
        }
        else
        {
            openXRSettings.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
        }



        //XRSettings.eyeTextureResolutionScale = 1.3f;
        MainCamera = Camera.main;


 

        RightHandDevices = new List<UnityEngine.XR.InputDevice>();
        LeftHandDevices = new List<UnityEngine.XR.InputDevice>();
        HeadDevices = new List<UnityEngine.XR.InputDevice>();

        if (MainCamera)
            MainCamera.transform.GetComponent<HDAdditionalCameraData>().allowDynamicResolution = false;






        if (HC != null) TempAdjust = HC.TargetDistance;

        AudioListener.pause = false;
        Time.timeScale = 1;
  

    }

    IEnumerator LoadChunk(int _Index)
    {



        Chunks[_Index].SetActive(true);
        
        yield return new WaitForSeconds(DelayTime);
            _Index++;

        
        if (_Index < Chunks.Length)
            {
            StartCoroutine(LoadChunk(_Index));
        }
        else
        {
            Shutter.material.DOFade(0, 1).OnComplete(() => { Shutter.gameObject.SetActive(false); });
        }
    }


    bool CameraUp, CameraDown;



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

        Shutter.material.DOFade(1, 1f).OnComplete(() =>
        {
            if (FallSceneManager.FallSceneManagerInstance)
            { 

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
            _PL.QuickFade();
        }

        StartCoroutine(ExecuteSceneChange(_Scene));



    }


    public void SetCameraHeight(bool _IsPlus)
    {


        if (HC == null) return;

        if (_IsPlus)
        {

            TempAdjust += OffsetSpeed * Time.deltaTime;

        }
        else
        {

            TempAdjust -= OffsetSpeed * Time.deltaTime;

        }

        HC.TargetDistance = Mathf.Clamp(TempAdjust, 0f, 1f);

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

        bool ButtonPushed1 = false, ButtonValue1 = false, ButtonPushed2 = false, ButtonValue2 = false;

        foreach (UnityEngine.XR.InputDevice device in LeftHandDevices)
        {
            LeftGripIsPulled = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out LeftGripValue) && LeftGripValue;

            //ButtonPushed1 = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out ButtonValue1) && ButtonValue1;
            ButtonPushed1 = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out ButtonValue1) && ButtonValue1;
            //ButtonPushed2 = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out ButtonValue2) && ButtonValue2;
        }




        if (ButtonPushed1)
        {
            if (ButtonTrigger) return;

            Debug.Log("PressCameraButton");
            ButtonTrigger = true;
            StartCoroutine(ScreenShotCamera.ExecuteShot());
        }

        if (!ButtonPushed1)
        {
            ButtonTrigger = false;
        }
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
    bool SpecialButtonIsPushed, IsCrouching;
    private void Update()
    {


        //Gyro();



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
                FallSceneManager.FallSceneManagerInstance.TVEG.windPower = WindPower;
            }
            IsStandalone = FallSceneManager.FallSceneManagerInstance.IsStandalone;
            HC.IsStandalone = IsStandalone;

            if (!IsStandalone)
            {

                if (LeftHand1) LeftHand1.gameObject.SetActive(true);
                if (RightHand1) RightHand1.gameObject.SetActive(true);
                if (FPSLG) FPSLG.enabled = false;

                CameraTransform.GetComponent<TrackedPoseDriver>().enabled = true;
            }
            else
            {

                if (LeftHand1) LeftHand1.gameObject.SetActive(false);
                if (RightHand1) RightHand1.gameObject.SetActive(false);
                if (FPSLG) FPSLG.enabled = true;
                CameraTransform.GetComponent<TrackedPoseDriver>().enabled = false;
            }

        }

        if (HeadDevices.Count == 0)
        {
            HeadDevices = new List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.Head, HeadDevices);
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
                 
                HMDOn = false;
                return;
            }
            else
            {
                HMDOn = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out HeadTriggerValue) && HeadTriggerValue;
            }

        }

        if (IsStandalone) HMDOn = true;




        if (HMDOn && !HMDTrigger)
        {
            HMDTrigger = true;

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
            HMDTrigger = false;
            InterfaceSceenIsOn = true;
            if (InterfaceScreenPrefab)
                InterfaceScreenPrefab.SetActive(true);

        }







        if (RightHandDevices.Count == 0)
        {
            var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, RightHandDevices);


        }

        if (LeftHandDevices.Count == 0)
        {

            LeftHandDevices = new List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, LeftHandDevices);
        }





        //if (!LeftGripIsPulled)
        //{
        //    LeftGripPull = false;

        //}

        //// Keyboard

        //if (Keyboard.current.sKey.wasPressedThisFrame)
        //{
        //    StartCoroutine(ScreenShotCamera.TakeScreenshot());
        //}

        ////if (CameraUp) SetCameraHeight(true);
        ////if (CameraDown) SetCameraHeight(false);

        //if (CameraSlide.action.ReadValue<Vector2>().y > .5f)
        //{
        //    LeftHand1.gameObject.SetActive(true);
        //    LeftHand2.gameObject.SetActive(true);

        //    CameraUp = true;
        //    CameraDown = false;
        //};

        //if (CameraSlide.action.ReadValue<Vector2>().y < -.5f)
        //{

        //    LeftHand1.gameObject.SetActive(true);
        //    LeftHand2.gameObject.SetActive(true);
        //    CameraUp = false;
        //    CameraDown = true;
        //};

        //if (Mathf.Abs(CameraSlide.action.ReadValue<Vector2>().y) <= .5f)
        //{
        //    CameraUp = false;
        //    CameraDown = false;
        //}

        //bool ButtonTrigger = false;

        if (SpecialButton != null)
        {
            if(SpecialButton.action.IsPressed() && !SpecialButtonIsPushed)
            {
                SpecialButtonIsPushed = true;
                if (!IsCrouching)
                {
                    HC.VerticalMultiplier = .5f;
                    HC.TargetDistance = HC.CrouchHeight;
                    IsCrouching = true;
                    return;
                }
                
                if (IsCrouching)
                {
                    HC.VerticalMultiplier = 1f;
                    IsCrouching = false;
                    HC.TargetDistance = TempAdjust;
                }
  
            }   
            


            if (!SpecialButton.action.IsPressed()) SpecialButtonIsPushed = false;
        }
        
        // if(SpecialButton.action.IsPressed() && SpecialButtonIsPushed)
        // { 
        //     HC.TargetDistance = TempAdjust;
        // }
        


        if (CameraSlide && IsStandalone)
        {
            if (CameraSlide.action != null)
            {
            Vector3 TempPos = MainCamera.transform.localPosition;
            TempPos.y += CameraSlide.action.ReadValue<Vector2>().y * Time.deltaTime * TiltValue * .03f;
                TempPos.y = Mathf.Clamp(TempPos.y, -HC.StandaloneHeight, HC.StandaloneHeight);
            MainCamera.transform.localPosition = TempPos;
            }

                

                //HC.StandaloneHeight += CameraSlide.action.ReadValue<Vector2>().y * Time.deltaTime * TiltValue * .005f;


        }

        if (CameraPitch != null && IsStandalone)
        {


            if(CameraPitch.action!= null)
                PitchAdj.x -= CameraPitch.action.ReadValue<Vector2>().y * Time.deltaTime * TiltValue;

            MainCamera.transform.localEulerAngles = PitchAdj;

        };
    }

}
