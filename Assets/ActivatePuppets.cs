using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatePuppets : MonoBehaviour
{
    public GameObject[] Puppets;

    int index = 0;

    public void Activate()
    {
        index = 0;
        StartCoroutine(ShowPuppet());
    }

    public void DeActivate()
    {
        foreach (GameObject _GO in Puppets) {

            XRPuppetInteractable XRPI = _GO.GetComponent<XRPuppetInteractable>();

            if(XRPI && XRPI.IsPuppet)
            {
                XRPI.DropPuppet();
            }
        _GO.SetActive(false);
        }

    }


    IEnumerator ShowPuppet()
    {
        yield return new WaitForEndOfFrame();
        if(index < Puppets.Length)
        {
            Puppets[index].SetActive(true);
            index++;
            StartCoroutine(ShowPuppet());
        }
    }
}
