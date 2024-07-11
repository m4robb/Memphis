using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoRinger : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject AudioSourceGO;
    public AudioClip AudioClipRinger;
    public float RingerDuration = 0;
    void Start()
    {
        
    }

    public void StartRing()
    {

        Debug.Log(RingerDuration);
        GameObject GO = Instantiate(AudioSourceGO, transform);
        GO.GetComponent<AudioSource>().clip = AudioClipRinger;
        GO.GetComponent<AudioSource>().Play();
        if (RingerDuration > 0)
            GO.GetComponent<AudioSource>().DOFade(0, RingerDuration).OnComplete(()=> {
                EndRing(GO);
            });
    }

    public void EndRing(GameObject _GO)
    {
        Destroy(_GO);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
