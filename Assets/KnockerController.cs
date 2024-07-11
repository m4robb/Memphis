using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockerController : MonoBehaviour
{
    public Renderer Shutter;
    public HingeJoint HJ;

    bool CanLeave, ColorTrigger;

    Color ShutterColor = new Color(1, 1, 1, 0);



    private void Update()
    {

        if (FallSceneManager.FallSceneManagerInstance != null)
        {
            CanLeave = true;
        }

        if (!CanLeave) return;

        float TempAngle = Mathf.Abs(HJ.angle) / 300 - .1f;

        Shutter.gameObject.SetActive(true);

        ShutterColor.a = Mathf.Clamp(TempAngle, 0, 1);

        Shutter.material.color = ShutterColor;

        if( TempAngle > .2f && !ColorTrigger)
        {
            FallSceneManager.FallSceneManagerInstance.LoadNextScene("Platz01");
            ColorTrigger = true;
        }


    }
}
