using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class GlobalStart : MonoBehaviour
{
    public UnityEvent OnStart;


    IEnumerator StartEveryThing() {
        yield return new WaitForSeconds(.1f);
        if (OnStart != null) OnStart.Invoke();
    }

    private void Start()
    {
        StartCoroutine(StartEveryThing());
    }
}
