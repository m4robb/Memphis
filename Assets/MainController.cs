using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        UnityEngine.Rendering.TextureXR.maxViews = 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
