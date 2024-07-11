using System.Collections;
using UnityEngine;
using DG.Tweening;

public class MonkeyNoiseController : MonoBehaviour
{
    public AudioSource AS;

    public AudioClip[] ClipArray;

    IEnumerator MonkeyNoiseExecute(int _ClipIndex)
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        AS.PlayOneShot(ClipArray[_ClipIndex]);
    }

    public void StopMonkey()
    {
        AS.DOFade(0, 1).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void MonkeyNoise()
    {

        if (!CanMakeNoise) return;

        StartCoroutine(MonkeyNoiseExecute(Random.Range(0, ClipArray.Length)));

    }

    bool Trigger, CanMakeNoise;
    void Update()
    {
        if (AudioGridTick.AudioGridInstance != null && !Trigger)

        {


            Trigger = true;
            CanMakeNoise = true;

        }
    }

}
