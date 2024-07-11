using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLayer : MonoBehaviour
{
    public LayerMask layerMask;

    LayerMask StartLayer;
    void Start()
    {
        StartLayer = gameObject.layer;
    }

    public void DoSwitch()
    {
        gameObject.layer = LayerMask.NameToLayer("Grabbable");
    }

    public void RevertSwitch()
    {
        gameObject.layer = LayerMask.NameToLayer("NavMesh");
    }
}
