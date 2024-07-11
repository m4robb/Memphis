using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BellController : MonoBehaviour
{

    public Transform SoundPosition;


    public void Echo(GameObject _SoundPrefab)
    {
        GameObject _GO = Instantiate(_SoundPrefab, SoundPosition);
        _GO.transform.parent = SoundPosition;
    }

}
