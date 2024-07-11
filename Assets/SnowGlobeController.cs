using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SnowGlobeController : MonoBehaviour
{

    public UnityEvent Activate;
    public float DelayPeriod = 0;

    IEnumerator CallAction()
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime() + (float)AudioGridTick.AudioGridInstance.bpm / 60 * DelayPeriod);
        Activate.Invoke();
        
    }

    public void TurnOnGlobe()
    {

        if(Activate!= null)
        StartCoroutine(CallAction());
    }


}
