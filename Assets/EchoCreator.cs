using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoCreator : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource InitialSource;
    public GameObject EchoObject;
    public float InitialVolume = 1;
    public float EchoVolume;
    public Transform EchoTransform;

    private void Start()
    {

        if(InitialSource)
        InitialSource.enabled = false;
  
    }


    void DoEcho()
    {


       

        if (_AttachedAS != null)
        {
            ALFX.enabled = true;

            Debug.Log(_AttachedAS);


            _AttachedAS.volume = EchoVolume;

            

            //_AttachedAS.PlayOneShot(ALFX.AudioClipArray[0]);

        }
        else
        {

            InitialSource.volume = EchoVolume;

            ALFX._AS = InitialSource;

            InitialSource.enabled = true;

            InitialSource.volume = InitialVolume;

            // InitialSource.PlayOneShot(ALFX.AudioClipArray[0]);
        }


    }

    GameObject _GO;
    AudioSource _AttachedAS;
    AudioLooperSFX ALFX;

    public void StartEcho()
    {

        _GO = Instantiate(EchoObject);
        _AttachedAS = _GO.GetComponent<AudioSource>();
        if(EchoTransform != null)
        {
            _GO.transform.parent = EchoTransform;
        }
        else
        {
            _GO.transform.parent = transform;
        }
        
        _GO.transform.localPosition = Vector3.zero;

         ALFX = EchoObject.GetComponent<AudioLooperSFX>();





        Invoke("DoEcho", .01f);

    }

 
}
