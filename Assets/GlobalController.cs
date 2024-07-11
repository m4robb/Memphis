using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalController : MonoBehaviour
{

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {

            Application.Quit();
        }
    }
}
