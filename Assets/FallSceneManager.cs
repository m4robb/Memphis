using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TheVegetationEngine;
using Unity.Entities.UniversalDelegates;


public class FallSceneManager : MonoBehaviour
{
    public static FallSceneManager FallSceneManagerInstance;

    public TVEGlobalMotion TVEG;

    public bool IsStandalone;

    public string LimboDestination;

    public float CameraHeightAdjustment = 0;

    public float DefaultStandHeight = 1.5f;

    public float DefaultCrouchHeight = .7f;

    public List<PlayLongSoundTimed> TransitionAudios = new List<PlayLongSoundTimed>();

    public int SlideIndex = 0;
    AsyncOperation AO, AOPreload;


    private void Start()
    {





        if (FallSceneManagerInstance == null)
        {
            DontDestroyOnLoad(gameObject);
            FallSceneManagerInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
           
    }

    bool IsNextSceneLoaded;

    public void PreloadSceneStart(string sceneName)
    {
        StartCoroutine(Preload(sceneName));
    }

    public IEnumerator Preload(string sceneName)
    {
        

        AOPreload = SceneManager.LoadSceneAsync(sceneName);

        AOPreload.allowSceneActivation = false;

        while (!AOPreload.isDone)
        {

            yield return null;
        }

        Debug.Log($"[scene]:{sceneName}");

        //Resources.UnloadUnusedAssets();
    }



    public IEnumerator LoadSceneAsyncProcess(string sceneName)
    {

        AO = SceneManager.LoadSceneAsync(sceneName);

        while (!AO.isDone)
        {
         
            yield return null;
        }

        Debug.Log($"[scene]:{sceneName}");

        //Resources.UnloadUnusedAssets();
    }

    public void SetLimboDestination(string _Scene)
    {
        LimboDestination = _Scene;
    }

    public void ClearLimboDestination()
    {
        LimboDestination = "";
    }

    public void LoadNextScene(string _SceneName)
    {

       //StartCoroutine(LoadSceneAsyncProcess(_SceneName));

        if (AOPreload != null)
        {
            AOPreload.allowSceneActivation = true;
            AOPreload = null;
        }
        else
        {
            StartCoroutine(LoadSceneAsyncProcess(_SceneName));
        }

        // SceneManager.LoadSceneAsync(_SceneName, LoadSceneMode.Single);

    }

    public void AddAudioToPersistent(PlayLongSoundTimed _PST)
    {

        if (!TransitionAudios.Contains(_PST)) {
            Debug.Log(_PST);
            _PST.transform.parent = null;
            TransitionAudios.Add(_PST);

            DontDestroyOnLoad(_PST.gameObject); 
        }

    }

    public void ClearPersistentAudio()
    {

        for(int i = 0; i < TransitionAudios.Count; i++)
        {


            TransitionAudios[i].TransistionFade(4);
            StartCoroutine(DestroyAudio(TransitionAudios[i].gameObject));
        }

        TransitionAudios = new List<PlayLongSoundTimed>();

    }

    IEnumerator DestroyAudio(GameObject _GO)
    {
        yield return new WaitForSeconds(4.5f);
        Destroy(_GO);
    }

    void Update()
    {




        //if (Keyboard.current.spaceKey.wasPressedThisFrame)
        //{
        //    AO.allowSceneActivation = true;
        //}

        //if (Keyboard.current.digit0Key.wasPressedThisFrame)
        //{
        //    LoadNextScene("UnderWater01");
        //}


        //if (Keyboard.current.digit1Key.wasPressedThisFrame)
        //{
        //    LoadNextScene("KeeperTest01");
        //}

        //if (Keyboard.current.digit2Key.wasPressedThisFrame)
        //{
        //    LoadNextScene("Pool");
        //}

        //if (Keyboard.current.digit3Key.wasPressedThisFrame)
        //{
        //    LoadNextScene("Halls");
        //}

        //if (Keyboard.current.digit4Key.wasPressedThisFrame)
        //{
        //    LoadNextScene("Toystore");
        //}




        //if (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        //{
        //    Application.Quit();
        //}


    }
}
