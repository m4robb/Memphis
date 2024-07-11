using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChimeController : MonoBehaviour
{

    public Transform Cafe;
    public Transform Dancer;
    public Transform HeadCam;
    public Transform HeadCamStabilized;
    public UnityEvent PlatzSideBehind;
    public UnityEvent StreetSideBehind;
    public bool CanMakeVisible;
    bool FTrigger, BTrigger;

    IEnumerator DelayedStreetView()
    {
        yield return new WaitForSeconds(1);
        StreetSideBehind.Invoke();
    }
    void Update()
    {

        if (CanMakeVisible)
        {
            HeadCamStabilized.position = HeadCam.position;
            Vector3 HeadCamRotation = HeadCam.localEulerAngles;
            HeadCamRotation.x = 0;
            HeadCamRotation.z = 0;
            HeadCamStabilized.localEulerAngles = HeadCamRotation;
            Vector3 cafeDir = Cafe.position - transform.position;
            float angle = Vector3.Angle(cafeDir, HeadCamStabilized.forward);

            if (angle > 70.0f && PlatzSideBehind != null && !BTrigger)
            {
                PlatzSideBehind.Invoke();
                BTrigger = true;
            }


            Vector3 DancerDir = Dancer.position - transform.position;
            Vector3 HeadDir = HeadCam.forward;
            HeadDir.x = 0;
            HeadDir.z = 0;
            float angle2 = Vector3.Angle(DancerDir, HeadCamStabilized.forward);



            if (angle2 > 70.0f && StreetSideBehind != null && !FTrigger)
            {
                StartCoroutine(DelayedStreetView());
                FTrigger = true;
            }

        }

    }
}
