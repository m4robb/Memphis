using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DG.Tweening;


public class EndScene : MonoBehaviour
{
    public Renderer Shutter;
    public UnityEvent OnEnd;
    public float ShutterTimeout;
    public string NextScene;

    public bool IsOneStep;

    bool HasTriggered, IsFading;
    AsyncOperation AO;

     IEnumerator LoadSceneAsyncProcess()
    {

        AO = SceneManager.LoadSceneAsync(NextScene);
        Debug.Log("goToNextScene");
        while (!AO.isDone)
        {
            yield return null;
        }


    }

    public void FinishEndScene()
    {
        StartCoroutine(LoadSceneAsyncProcess());
    }


    public void EndThisScene()
    {
        if (HasTriggered) return;
        HasTriggered = true;
        Shutter.gameObject.SetActive(true);
        Shutter.material.DOFade(1, ShutterTimeout).OnComplete(() =>
        {
            if (OnEnd != null) OnEnd.Invoke();

            if(IsOneStep) StartCoroutine(LoadSceneAsyncProcess());

        });
    }
}
