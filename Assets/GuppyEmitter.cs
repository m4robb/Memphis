using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuppyEmitter : MonoBehaviour
{
    public GameObject Guppy;
    public int LoopLength;
    bool IsLooping, DoClickTrigger;

    int Counter = 0;

    int GuppyCount = 0;

    private void OnEnable()
    {
       
    }
    private void OnDisable()
    {
        AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;
    }

    public void DoClick()
    {
        Counter++;
        DoClickTrigger = true;
    }

    void Update()
    {
        if (AudioGridTick.AudioGridInstance != null && !IsLooping)
        {
            AudioGridTick.AudioGridInstance.storedAudioAction += DoClick;
            IsLooping = true;
        }

        if (DoClickTrigger && Counter >= LoopLength)
        {

            Debug.Log("fish");
            if (GuppyCount > 10) return;
            GuppyCount++;
            GameObject _GO = Instantiate(Guppy);
            _GO.transform.parent = transform;
            _GO.transform.localPosition = Vector3.zero;
            _GO.transform.localEulerAngles = new Vector3(Random.Range(0, 90), Random.Range(-90, 0), Random.Range(0, 90));
            DoClickTrigger = false;
            Counter = 0;
        }

    }
}
