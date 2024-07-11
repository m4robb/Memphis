using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class Sector
{
    public GameObject SectorObject;
    public bool KeepAlive;
    public bool Persistent;
    public bool NoPeeking;
}


public class SectorLoader : MonoBehaviour
{
    public Sector[] SectorArray;

    bool NoPeekingTrigger;

    Camera MainCamera;

    void Start()
    {

 
        foreach (Sector _Sector in SectorArray)
        {
            if (_Sector.KeepAlive)
                _Sector.SectorObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            MainCamera = other.gameObject.GetComponentInParent<Camera>();

            foreach (Sector _Sector in SectorArray)
            {
                if (_Sector.KeepAlive && !_Sector.Persistent)
                    _Sector.SectorObject.SetActive(false);
                else if (!_Sector.KeepAlive && _Sector.Persistent)
                    _Sector.SectorObject.SetActive(false);
                else
                    _Sector.SectorObject.SetActive(true);
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {


        if(other.tag == "Player")
        {

            NoPeekingTrigger = true;
            foreach (Sector _Sector in SectorArray)
            {
                if (_Sector.KeepAlive)
                    _Sector.SectorObject.SetActive(true);
             else 
                _Sector.SectorObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (NoPeekingTrigger)
        //{
        //    foreach (Sector _Sector in SectorArray)
        //    {
        //        if (_Sector.NoPeeking && MainCamera != null)
        //        {
        //            Vector3 cafeDir = Cafe.position - transform.position;
        //            float angle = Vector3.Angle(cafeDir, HeadCam.forward);

        //            if (angle > 70.0f)
        //                Cafe.gameObject.SetActive(true);
        //        }
        //    }

        //}
    }
}
