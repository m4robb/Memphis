using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitchController : MonoBehaviour
{
    // Start is called before the first frame update

    public Light ConnectedLight;
    public GameObject LightGO;
    public GameObject AudioSourceGO;
    public AudioClip AudioClipSwitch;

    bool LightIsOn;
    void Start()
    {
        LightIsOn = ConnectedLight.enabled;
    }

    public void ToggleLight()
    {
        GameObject GO = Instantiate(AudioSourceGO, transform);
        GO.GetComponent<AudioSource>().clip = AudioClipSwitch;
        GO.GetComponent<AudioSource>().Play();
        StartCoroutine(KillSwitchSound(AudioClipSwitch.length, GO));

        if(LightIsOn)
        {

            if (LightGO != null) LightGO.SetActive(false);
            ConnectedLight.enabled = false;
            LightIsOn = false;
            return;
        }

        if (!LightIsOn)
        {
            if (LightGO != null) LightGO.SetActive(true);
            ConnectedLight.enabled = true;
            LightIsOn = true;
            return;
        }

    }

    IEnumerator KillSwitchSound(float _Delay, GameObject _GO)
    {
        yield return new WaitForSeconds(_Delay);

        //Destroy(_GO);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
