using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WindChime : MonoBehaviour
{
    public AudioSource Chime;
    public float PitchShift = 1;

    public ChimeController CC;

    public LineRenderer LR;

    public Transform BottomPoint;

    public Transform TopPoint;

    Vector3[] Points;

    int lengthOfLineRenderer = 2;

    private void Update()
    {

        Vector3 TP = TopPoint.position;
        TP = transform.position;
        TP.y = 5;
        TopPoint.position = TP;

        Points = new Vector3[2];
        Points[0] = BottomPoint.position;
        Points[1] = TopPoint.position;
        LR.SetPositions(Points);
    }

    int counter = 0;
    private void OnCollisionEnter(Collision collision)
    {




        Chime.pitch = PitchShift;
        Chime.PlayOneShot(Chime.clip);
        if (collision.transform.tag == "Player") CC.CanMakeVisible = true;
    }

}
