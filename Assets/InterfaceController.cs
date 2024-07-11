using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Permissions;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Networking;
using System;
using DG.Tweening;
using Unity.Entities.UniversalDelegates;



public class InterfaceController : MonoBehaviour
{

    public GameObject ScreenshotsButton;

    public Button AccessScreenShots;

    public TextMeshProUGUI Instructions;

    public float SlideDuration = 10;

    public GameObject Panel;

    public UnityEvent HMDOn;
    public UnityEvent HMDOff;

    public RawImage RI;
     

    SceneChunkLoader SCL;

    Camera MainCamera;

    int SlideIndex;

    private void Start()
    {
    }
    void OnEnable()
    {
        RI.DOFade(0,0);
        MainCamera = Camera.main;
        SlideIndex = 0;
        //if(RI != null)
        //    RI.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height, Screen.height);
        //SCL = FindFirstObjectByType<SceneChunkLoader>();
        CheckForScreenshots();
    }

    DirectoryInfo dir;
    FileInfo[] info;


    IEnumerator GetTexture(string _FilePath)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(_FilePath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                if (RI != null && RI.gameObject.activeSelf)
                {
                    RI.texture = DownloadHandlerTexture.GetContent(uwr);
                    RI.DOFade(1, 1).OnComplete(() => { 
                        StartCoroutine(CycleThroughImages()); 
                    });
                }

            }
        }
    }


    IEnumerator CycleThroughImages(){

        yield return new WaitForSeconds(SlideDuration);

        if (SlideIndex < info.Length - 1)
        {
             SlideIndex++;
        }
        else
        {
            SlideIndex = 0;
        }

        RI.DOFade(0, 1).OnComplete(() => {
            Debug.Log(info[SlideIndex].FullName);
            StartCoroutine(GetTexture(info[SlideIndex].FullName)); 
        });






    }
    int CheckForScreenshots()
    {


       dir = new DirectoryInfo(Application.persistentDataPath);

       info = dir.GetFiles("*.png");



        if (info.Length > 0)
        {
            Instructions.text = "You have saved images. Click here to access.";

            Array.Sort(info, delegate (FileInfo f1, FileInfo f2)
            {
                return f2.CreationTime.CompareTo(f1.CreationTime);
            });

            StartCoroutine(GetTexture(info[SlideIndex].FullName));

        }
        else
        {
            Instructions.text = "For screenshots, use the menu button on your left controller";
        }

        return info.Length;
    }

    public static void OpenInWinFileBrowser()
    {
        bool openInsidesOfFolder = false;

        // try windows
        string winPath = Application.persistentDataPath.Replace("/", "\\"); // windows explorer doesn't like forward slashes

        if (Directory.Exists(winPath)) // if path requested is a folder, automatically open insides of that folder
        {
            openInsidesOfFolder = true;
        }

        Application.OpenURL(winPath);
        //    try
        //    {

        //       // System.Diagnostics.Process process = new System.Diagnostics.Process();
        //        System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
        //    }
        //    catch (System.ComponentModel.Win32Exception e)
        //    {

        //        Debug.Log(winPath);
        //        // tried to open win explorer in mac
        //        // just silently skip error
        //        // we currently have no platform define for the current OS we are in, so we resort to t$$anonymous$$s
        //        e.HelpLink = ""; // do anyt$$anonymous$$ng with t$$anonymous$$s variable to silence warning about not using it
        //    }
 }

        bool Trigger;

    private void OnApplicationQuit()
    {
        if(CheckForScreenshots() > 0) OpenInWinFileBrowser();
    }
    void Update()
    {
        if (!Trigger && SCL && SCL.HMDOn)
        {
            Trigger = true;
            //Panel.SetActive(false);
            //XRSettings.enabled = true;
            // HMDOn.Invoke();
        }
        if (Trigger && SCL && !SCL.HMDOn)
        {
            Trigger = false;
            //Panel.SetActive(true);
            //CheckForScreenshots();
            //if (CheckForScreenshots() > 0) OpenInWinFileBrowser();
            //XRSettings.enabled = false;
            //HMDOff.Invoke();
        }


    }
}
