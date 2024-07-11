using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AgentAnimationController : MonoBehaviour
{
    public Animator AnimatorController;

    public RichAI RAI;

    public AIDestinationSetter AIDS;

    public Transform[] Stations;

    public Transform CameraPosition;

    void Start()
    {
        //AIDS.target = null;
    }


    Vector3 relVelocity, angularVelocity;
    float CharacterTurnAngle;
    Quaternion previousRotation, StoredRotatation;
    public bool isAtDestination = true, IsChasing;
    int StationIndex = 0;
    float WaitTiime;





    IEnumerator OnTargetReached(float _WaitIme)
    {
        yield return new WaitForSeconds(_WaitIme);

        Debug.Log("Animal move");

        if (StationIndex < Stations.Length-1)
        {
            StationIndex++;
        }
        else
        {
            StationIndex = 0;
        }
        AIDS.target = Stations[StationIndex];
    }

    float DistanceToCamera;
    void Update()
    {

        if (RAI.reachedEndOfPath)
        {

            if (!isAtDestination)
            {
                 StartCoroutine(OnTargetReached(Random.Range(5, 15f)));
            }
            isAtDestination = true;
        }
        else
        {
            isAtDestination = false;
        }

        relVelocity = transform.InverseTransformDirection(RAI.velocity);

        DistanceToCamera = Vector3.Distance(transform.position, CameraPosition.position);

        if (isAtDestination && DistanceToCamera < 4 && !IsChasing)
        {

            IsChasing = true;
            StartCoroutine(OnTargetReached(0));
            isAtDestination = false;

        }

        if(DistanceToCamera > 5)
        {
            IsChasing = false;
        }

     

        Quaternion deltaRotation = RAI.rotation * Quaternion.Inverse(previousRotation);

        

        deltaRotation.ToAngleAxis(out var Angle, out var axis);
        Angle *= Mathf.Deg2Rad;

        angularVelocity = (50) *  Angle * axis;

        CharacterTurnAngle = Mathf.Clamp(angularVelocity.y, -2f, 2f);

      // Debug.Log(CharacterTurnAngle);

        previousRotation = RAI.rotation;
        AnimatorController.SetFloat("Turn", Mathf.Lerp(AnimatorController.GetFloat("Turn"), CharacterTurnAngle, Time.deltaTime * 2));
        AnimatorController.SetFloat("Forward", Mathf.Lerp(AnimatorController.GetFloat("Forward"), relVelocity.z, Time.deltaTime * 2));
    }
}
