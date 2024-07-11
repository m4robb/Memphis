using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

public class MainAudioController : MonoBehaviour
{
    // Start is called before the first frame update

    public AudioClip[] Melodies;
    public GameObject MelodySoundEmitter;
    public float HitStrength= 0;

    public AudioMixer DuckAble;

    int MelodyIndex = 0;
    public bool InitMelodies;

    public static MainAudioController MainAudioControllerInstance;
    void Awake()
    {
        MainAudioControllerInstance = this;
        StartCoroutine(DelayInit());
    }

    public void DoMelody(float HitStrength)
    {
        if (!InitMelodies) return;
        GameObject GO = Instantiate(MelodySoundEmitter);
        GO.GetComponent<AudioSource>().clip = Melodies[MelodyIndex];
        GO.GetComponent<AudioSource>().Play();
        GO.GetComponent<AudioSource>().DOFade(0, HitStrength).OnComplete(() =>
        {
            KillSoundGO(GO);
        });
        MelodyIndex++;
        if (MelodyIndex == Melodies.Length) MelodyIndex = 0;
    }

    public float StoredValue = 0;


    public float GetCurrentLevel(string _Channel)
    {
        float value = 0;
        bool result = DuckAble.GetFloat(_Channel, out value);
       

        if (result)
        {
            StoredValue = value;
            return value;
        }
            
        else
            return 0;
    }
    

    public void FadeDuckable(string _Channel, float _TargetLevel = 0f, float _Duration = 3f )
    {
        float Value = GetCurrentLevel(_Channel);
        float FadeValue = GetCurrentLevel(_Channel);
            DOTween.To(() => FadeValue, x => FadeValue = x, _TargetLevel, _Duration).OnUpdate(()=> {
               DuckAble.SetFloat(_Channel, FadeValue);
            });

    }

   void KillSoundGO(GameObject _GO)
    {
        Destroy(_GO);
    }

    IEnumerator DelayInit()
    {
        yield return new WaitForSeconds(2);

        InitMelodies = true;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
