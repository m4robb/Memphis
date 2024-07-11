using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BurnerController : MonoBehaviour
{
    public Transform[] BurnerArray;
    public VisualEffect ActiveBurner;


    public void TurnBurnerOn(int _BurnerIndex)
    {
        ActiveBurner.enabled = true;
        transform.position = BurnerArray[_BurnerIndex].position;
    }

    public void TurnBurnerOff()
    {
        ActiveBurner.enabled = false;
    }
}
