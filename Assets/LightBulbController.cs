using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBulbController : MonoBehaviour
{
    // Start is called before the first frame update

    public Material Filament;

    Color StartEmissive;

    bool LightIsOn = true;



    void Start()
    {
        StartEmissive = Filament.GetColor("_EmissiveColor");
        LightIsOn = true;
    }

    public void ToggleFilamentMaterial()
    {

        Color Off = Color.black;

        if (LightIsOn)
        {
            Debug.Log(StartEmissive);
            Filament.SetColor("_EmissiveColor", Off);
            LightIsOn = false;
            return;
        }

        if (!LightIsOn)
        {
            Filament.SetColor("_EmissiveColor", StartEmissive);
            LightIsOn = true;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
