using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using DG.Tweening;

public class LimboController : MonoBehaviour
{
    public InputActionReference PullTriggerLeft = null;
    public InputActionReference PullTriggerRight = null;

    public string NextScene;

    public Renderer Shutter;
    AsyncOperation AO;


    void Start()
    {
        Shutter.material.DOFade(0, 5).OnComplete(()=> {
            Shutter.gameObject.SetActive(false);
        });
    }



    public IEnumerator LoadSceneAsyncProcess()
    {

        AO = SceneManager.LoadSceneAsync(NextScene);

        while (!AO.isDone)
        {

            yield return null;
        }



        //Resources.UnloadUnusedAssets();
    }


    bool HasTriggeredFade;

    public void FadeAndChange()
    {

        if (HasTriggeredFade) return;

        HasTriggeredFade = true;

        Shutter.gameObject.SetActive(true);
        Shutter.material.DOFade(1, 5).OnComplete(() => {
            StartCoroutine(LoadSceneAsyncProcess());
        });
    }


    void Update()
    {

        if (PullTriggerLeft)
        {
            float TriggerLeft = PullTriggerLeft.action.ReadValue<float>();
            if (TriggerLeft > .1f) FadeAndChange();
        }

        if (PullTriggerRight)
        {
            float TriggerRight = PullTriggerRight.action.ReadValue<float>();
            if (TriggerRight > .1f) FadeAndChange();
        }

    }
}
