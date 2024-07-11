using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingLine : MonoBehaviour
{
    public LineRenderer LR;
    public Transform TopPoint;
    public Transform BottomPoint;

    Vector3 StoredPosition;

    Vector3[] Points;
    void Update()
    {
        if (BottomPoint.position == StoredPosition) return;

        Points = new Vector3[2];
        Points[0] = BottomPoint.position;
        Points[1] = TopPoint.position;
        LR.SetPositions(Points);

        StoredPosition = BottomPoint.position;


    }
}
