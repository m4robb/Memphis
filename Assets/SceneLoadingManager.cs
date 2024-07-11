using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    IEnumerator LoadSceneAsync(string _SceneName)
    {


        AsyncOperation AsyncLoad = SceneManager.LoadSceneAsync(_SceneName, LoadSceneMode.Additive);


        while (!AsyncLoad.isDone)
        {
            yield return null;
        }
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(LoadSceneAsync("Building17_Interior"));
        }

    }
}
