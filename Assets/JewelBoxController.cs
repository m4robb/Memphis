using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JewelBoxController : MonoBehaviour
{
    // Start is called before the first frame update

    public AudioSource AudioSourceGO;
    public GameObject Box;

    public HingeJoint HJ;

    bool BoxIsOpen;

    void Start()
    {
        AudioSourceGO.enabled = false;
       // Box.SetActive(false);
       
    }

    public void OpenBox()
    {
       // Box.SetActive(true);
        AudioSourceGO.enabled = true;
        AudioSourceGO.Play();

    }
    public void CloseBox()
    {
        //Box.SetActive(false);
        AudioSourceGO.enabled = false;
        AudioSourceGO.Pause();

    }


    // Update is called once per frame

    float StoredMixerValue = 0;
    void Update()
    {
    
        if (Mathf.Abs(HJ.angle) > 20)
        {
            if (!BoxIsOpen)
            {
                BoxIsOpen = true;
                Debug.Log("box open " + HJ.angle);
                OpenBox();
                MainAudioController.MainAudioControllerInstance.FadeDuckable("Duckable", -80f);
            }

        } else
        {
            //Debug.Log("box close " + HJ.angle);
            if (BoxIsOpen)
            {
                StoredMixerValue = MainAudioController.MainAudioControllerInstance.StoredValue;
                MainAudioController.MainAudioControllerInstance.FadeDuckable("Duckable", StoredMixerValue);
                BoxIsOpen = false;
                CloseBox();
            }

        }
           
    }
}
