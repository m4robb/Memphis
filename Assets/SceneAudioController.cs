using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SceneAudioController : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource MainMusic;

    public void StartMusic(AudioSource _AS)
    {
        _AS.volume = 0;
        _AS.enabled = true;
        _AS.Play();
        _AS.DOFade(1, 20);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
